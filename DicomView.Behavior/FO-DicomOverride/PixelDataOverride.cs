// Copyright (c) 2012-2026 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).
#nullable disable

using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.Algorithms;
using FellowOakDicom.Imaging.LUT;
using FellowOakDicom.Imaging.Mathematics;
using FellowOakDicom.Imaging.Render;
using FellowOakDicom.IO;
using FellowOakDicom.IO.Buffer;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace DicomView.Behaviors
{

    /// <summary>
    /// Pixel data interface implemented by various pixel format classes
    /// </summary>
    public unsafe interface IPixelDataOverride : IPixelData
    {
        /// <summary>
        /// Render the pixel data after applying <paramref name="lut"/> to the output array (allocated by user)
        /// </summary>
        /// <param name="lut">Lookup table to render the pixels into output pixels</param>
        /// <param name="output">The output array to store the result in</param>
        void Render(ILUT lut, int* output);

        IPixelDataOverride RescaleOverride(double scale);
    }

    /// <summary>
    /// Pixel data factory to create <see cref="IPixelDataOverride"/> and <see cref="SingleBitPixelData"/> from 
    /// <see cref="DicomPixelData"/>
    /// </summary>
    public static class PixelDataFactoryOverride
    {
        /// <summary>
        /// Create <see cref="IPixelDataOverride"/> form <see cref="DicomPixelData"/> 
        /// according to the input <paramref name="pixelData"/> <see cref="PhotometricInterpretation"/>
        /// </summary>
        /// <param name="pixelData">Input pixel data</param>
        /// <param name="frame">Zero-based frame index.</param>
        /// <returns>Implementation of <see cref="IPixelDataOverride"/> according to <see cref="PhotometricInterpretation"/></returns>
        public static IPixelDataOverride Create(DicomPixelData pixelData, int frame)
        {
            PhotometricInterpretation pi = pixelData.PhotometricInterpretation;

            if (pi == null)
            {
                // generally ACR-NEMA
                var samples = pixelData.SamplesPerPixel;
                if (samples == 0 || samples == 1)
                {
                    pi = pixelData.Dataset.Contains(DicomTag.RedPaletteColorLookupTableData)
                        ? PhotometricInterpretation.PaletteColor
                        : PhotometricInterpretation.Monochrome2;
                }
                else
                {
                    // assume, probably incorrectly, that the image is RGB
                    pi = PhotometricInterpretation.Rgb;
                }
            }

            if (pixelData.BitsStored == 1)
            {
                if (pixelData.Dataset.GetSingleValue<DicomUID>(DicomTag.SOPClassUID)
                    == DicomUID.MultiFrameSingleBitSecondaryCaptureImageStorage)
                {
                    // Multi-frame Single Bit Secondary Capture is stored LSB -> MSB
                    return new SingleBitPixelDataOverride(
                        pixelData.Width,
                        pixelData.Height,
                        PixelDataConverter.ReverseBits(pixelData.GetFrame(frame)));
                }
                else
                {
                    // Need sample images to verify that this is correct
                    return new SingleBitPixelDataOverride(pixelData.Width, pixelData.Height, pixelData.GetFrame(frame));
                }
            }
            else if (pi == PhotometricInterpretation.Monochrome1 || pi == PhotometricInterpretation.Monochrome2
                     || pi == PhotometricInterpretation.PaletteColor)
            {
                if (pixelData.BitsAllocated == 8 && pixelData.HighBit == 7 && pixelData.BitsStored == 8)
                {
                    return new GrayscalePixelDataU8Override(pixelData.Width, pixelData.Height, pixelData.GetFrame(frame));
                }
                else if (pixelData.BitsAllocated <= 16)
                {
                    return pixelData.PixelRepresentation == PixelRepresentation.Signed
                        ? new GrayscalePixelDataS16Override(
                            pixelData.Width,
                            pixelData.Height,
                            pixelData.BitDepth,
                            pixelData.GetFrame(frame))
                        : (IPixelDataOverride)new GrayscalePixelDataU16Override(
                            pixelData.Width,
                            pixelData.Height,
                            pixelData.BitDepth,
                            pixelData.GetFrame(frame));
                }
                else if (pixelData.BitsAllocated <= 32)
                {
                    return pixelData.PixelRepresentation == PixelRepresentation.Signed
                        ? new GrayscalePixelDataS32Override(
                            pixelData.Width,
                            pixelData.Height,
                            pixelData.BitDepth,
                            pixelData.GetFrame(frame))
                        : (IPixelDataOverride)new GrayscalePixelDataU32Override(
                            pixelData.Width,
                            pixelData.Height,
                            pixelData.BitDepth,
                            pixelData.GetFrame(frame));
                }
                else
                {
                    throw new DicomImagingException($"Unsupported pixel data value for bits stored: {pixelData.BitsStored}");
                }
            }
            else if (pi == PhotometricInterpretation.Rgb || pi == PhotometricInterpretation.YbrFull
                     || pi == PhotometricInterpretation.YbrFull422 || pi == PhotometricInterpretation.YbrPartial422)
            {
                var buffer = pixelData.GetFrame(frame);

                if (pixelData.PlanarConfiguration == PlanarConfiguration.Planar)
                {
                    buffer = PixelDataConverter.PlanarToInterleaved24(buffer);
                }

                if (pi == PhotometricInterpretation.YbrFull)
                {
                    buffer = PixelDataConverter.YbrFullToRgb(buffer);
                }
                else if (pi == PhotometricInterpretation.YbrFull422)
                {
                    // Fix issue#1049: check for planar configuration in case of PhotometricInterpretation.YbrFull422 was never done
                    if (pixelData.PlanarConfiguration == PlanarConfiguration.Planar)
                    {
                        throw new DicomImagingException("Unsupported planar configuration for YBR_FULL_422");
                    }
                    buffer = PixelDataConverter.YbrFull422ToRgb(buffer, pixelData.Width);
                }
                else if (pi == PhotometricInterpretation.YbrPartial422)
                {
                    buffer = PixelDataConverter.YbrPartial422ToRgb(buffer, pixelData.Width);
                }

                return new ColorPixelData24Override(pixelData.Width, pixelData.Height, buffer);
            }
            else
            {
                throw new DicomImagingException($"Unsupported pixel data photometric interpretation: {pi.Value}");
            }
        }

        /// <summary>
        /// Create <see cref="SingleBitPixelData"/> form <see cref="DicomOverlayData"/> 
        /// according to the input <paramref name="overlayData"/>
        /// </summary>
        /// <param name="overlayData">The input overlay data</param>
        /// <returns>The result overlay stored in <see cref="SingleBitPixelData"/></returns>
        public static SingleBitPixelDataOverride Create(DicomOverlayData overlayData)
        {
            return new SingleBitPixelDataOverride(overlayData.Columns, overlayData.Rows, overlayData.Data);
        }
    }

    /// <summary>
    /// Grayscale unsigned 8 bits <see cref="IPixelDataOverride"/> implementation
    /// </summary>
    public class GrayscalePixelDataU8Override : GrayscalePixelDataU8, IPixelDataOverride
    {
        #region Public Constructor

        /// <summary>
        /// Initializes an instance of the <see cref="GrayscalePixelDataU8Override"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="data">Byte buffer of data.</param>
        public GrayscalePixelDataU8Override(int width, int height, IByteBuffer data) : base(width, height, data) { }

        /// <summary>
        /// Initializes an instance of the <see cref="GrayscalePixelDataU8"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="data">Data byte array.</param>
        protected internal GrayscalePixelDataU8Override(int width, int height, byte[] data) : base(width, height, data) { }
        #endregion


        #region Public Methods
        public IPixelDataOverride RescaleOverride(double scale)
        {
            var w = (int)(Width * scale);
            var h = (int)(Height * scale);
            if (w == Width && h == Height) return this;

            var data = BilinearInterpolation.RescaleGrayscale(Data, Width, Height, w, h);
            return new GrayscalePixelDataU8Override(w, h, data);
        }

        /// <inheritdoc />
        public unsafe void Render(ILUT lut, int* output)
        {
            var data = Data;
            if (lut == null)
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = data[i];
                    }
                }
                );
            }
            else
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = (int)lut[data[i]];
                    }
                }
                );
            }
        }
        #endregion
    }

    /// <summary>
    /// Single bit pixel <see cref="IPixelDataOverride"/> implementation(for binary pixels) usually used for overlay pixel data
    /// </summary>
    public class SingleBitPixelDataOverride : GrayscalePixelDataU8Override
    {
        #region Public Constructor

        /// <summary>
        /// Initializes an instance of the <see cref="SingleBitPixelDataOverride"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="data">Byte data buffer.</param>
        public SingleBitPixelDataOverride(int width, int height, IByteBuffer data)
            : base(width, height, ExpandBits(width, height, data.Data))
        {
        }

        #endregion

        #region Static Methods

        private const byte One = 1;

        private const byte Zero = 0;

        private static byte[] ExpandBits(int width, int height, byte[] input)
        {
            var bits = new BitArray(input);
            var output = new byte[width * height];
            for (int i = 0, l = width * height; i < l; i++)
            {
                output[i] = bits[i] ? One : Zero;
            }
            return output;
        }

        #endregion
    }

    /// <summary>
    /// Grayscale signed 16 bits <see cref="IPixelDataOverride"/> implementation
    /// </summary>
    public class GrayscalePixelDataS16Override : IPixelDataOverride
    {
        #region Private Members

        private readonly BitDepth _bits;

        #endregion

        #region Public Constructor

        /// <summary>
        /// Initializes an instance of the <see cref="GrayscalePixelDataS16Override"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="bitDepth">Bit depth of pixel data.</param>
        /// <param name="data">Byte data buffer.</param>
        public GrayscalePixelDataS16Override(int width, int height, BitDepth bitDepth, IByteBuffer data)
        {
            _bits = bitDepth;
            Width = width;
            Height = height;

            var shortData = ByteConverter.ToArray<short>(data, bitDepth.BitsAllocated);

            if (bitDepth.BitsStored != 16)
            {
                // Normally, HighBit == BitsStored-1, and thus shiftLeft == shiftRight, and the two
                // shifts in the loop below just replaces the top shift bits by the sign bit.
                // Separating shiftLeft from shiftRight handles exotic cases where low-order bits
                // should also be discarded.
                int shiftLeft = bitDepth.BitsAllocated - bitDepth.HighBit - 1;
                int shiftRight = bitDepth.BitsAllocated - bitDepth.BitsStored;
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        // Remove masked high and low bits by shifting them out of the data type,
                        // getting the sign correct using arithmetic (sign-extending) right shift.
                        var d = (short)(shortData[i] << shiftLeft);
                        shortData[i] = (short)(d >> shiftRight);
                    }
                }
                );
            }

            Data = shortData;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="GrayscalePixelDataU32"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="data">Pixel data in internal data format.</param>
        private GrayscalePixelDataS16Override(int width, int height, short[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        #endregion

        #region Public Properties

        /// <inheritdoc />
        public int Width { get; }

        /// <inheritdoc />
        public int Height { get; }

        /// <inheritdoc />
        public int Components => 1;

        /// <summary>
        /// Gets pixel data in internal format.
        /// </summary>
        public short[] Data { get; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public DicomRange<double> GetMinMax(int padding)
        {
            if (Data == null || Data.Length == 0)
            {
                return default(DicomRange<double>);
            }

            var range = new DicomRange<double>(double.MaxValue, double.MinValue);
            Data.Where(v => v != padding).Each(v => range.Join(v));
            return range;
        }

        /// <inheritdoc />
        public DicomRange<double> GetMinMax()
        {
            if (Data == null || Data.Length == 0)
            {
                return default(DicomRange<double>);
            }

            var range = new DicomRange<double>(double.MaxValue, double.MinValue);
            Data.Each(v => range.Join(v));
            return range;
        }

        /// <inheritdoc />
        public double GetPixel(int x, int y)
        {
            var data = Data;
            return data[y * Width + x];
        }

        /// <inheritdoc />
        public IPixelData Rescale(double scale)
        {
            var w = (int)(Width * scale);
            var h = (int)(Height * scale);
            if (w == Width && h == Height) return this;

            var data = BilinearInterpolation.RescaleGrayscale(Data, Width, Height, w, h);
            return new GrayscalePixelDataS16Override(w, h, data);
        }

        public IPixelDataOverride RescaleOverride(double scale)
        {
            var w = (int)(Width * scale);
            var h = (int)(Height * scale);
            if (w == Width && h == Height) return this;

            var data = BilinearInterpolation.RescaleGrayscale(Data, Width, Height, w, h);
            return new GrayscalePixelDataS16Override(w, h, data);
        }
        /// <inheritdoc />
        public void Render(ILUT lut, int[] output)
        {
            var data = Data;
            if (lut == null)
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = data[i];
                    }
                }
                );
            }
            else
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = (int)lut[data[i]];
                    }
                }
                );
            }
        }

        public unsafe void Render(ILUT lut, int* output)
        {
            var data = Data;
            if (lut == null)
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = data[i];
                    }
                }
                );
            }
            else
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = (int)lut[data[i]];
                    }
                }
                );
            }
        }

        /// <inheritdoc />
        public Histogram GetHistogram(int channel)
        {
            if (channel != 0) throw new ArgumentOutOfRangeException(nameof(channel), channel, "Expected channel 0 for grayscale image.");

            var histogram = new Histogram(_bits.MinimumValue, _bits.MaximumValue);

            var data = Data;

            for (var i = 0; i < data.Length; i++) histogram.Add(data[i]);

            return histogram;
        }

        #endregion
    }

    /// <summary>
    /// Grayscale unsigned 16 bits <see cref="IPixelData"/> implementation
    /// </summary>
    public class GrayscalePixelDataU16Override : IPixelDataOverride
    {
        #region Private Members

        private readonly BitDepth _bits;

        #endregion

        #region Public Constructor

        /// <summary>
        /// Initializes an instance of the <see cref="GrayscalePixelDataU16"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="bitDepth">Bit depth of pixel data.</param>
        /// <param name="data">Byte data buffer.</param>
        public GrayscalePixelDataU16Override(int width, int height, BitDepth bitDepth, IByteBuffer data)
        {
            _bits = bitDepth;
            Width = width;
            Height = height;

            var ushortData = ByteConverter.ToArray<ushort>(data, bitDepth.BitsAllocated);

            if (bitDepth.BitsStored != 16)
            {
                // Normally, HighBit == BitsStored-1, and thus shiftLeft == shiftRight, and the two
                // shifts in the loop below just zeroes the top shift bits.
                // Separating shiftLeft from shiftRight handles exotic cases where low-order bits
                // should also be discarded.
                int shiftLeft = bitDepth.BitsAllocated - bitDepth.HighBit - 1;
                int shiftRight = bitDepth.BitsAllocated - bitDepth.BitsStored;

                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        // Remove masked high and low bits by shifting them out of the data type. 
                        var d = (ushort)(ushortData[i] << shiftLeft);
                        ushortData[i] = (ushort)(d >> shiftRight);
                    }
                }
                );
            }

            Data = ushortData;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="GrayscalePixelDataU32"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="data">Pixel data in internal data format.</param>
        private GrayscalePixelDataU16Override(int width, int height, ushort[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        #endregion

        #region Public Properties

        /// <inheritdoc />
        public int Width { get; }

        /// <inheritdoc />
        public int Height { get; }

        /// <inheritdoc />
        public int Components => 1;

        /// <summary>
        /// Gets pixel data in internal format.
        /// </summary>
        public ushort[] Data { get; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public DicomRange<double> GetMinMax(int padding)
        {
            if (Data == null || Data.Length == 0)
            {
                return default(DicomRange<double>);
            }

            var range = new DicomRange<double>(double.MaxValue, double.MinValue);
            Data.Where(v => v != padding).Each(v => range.Join(v));
            return range;
        }

        /// <inheritdoc />
        public DicomRange<double> GetMinMax()
        {
            if (Data == null || Data.Length == 0)
            {
                return default(DicomRange<double>);
            }

            var range = new DicomRange<double>(double.MaxValue, double.MinValue);
            Data.Each(v => range.Join(v));
            return range;
        }

        /// <inheritdoc />
        public double GetPixel(int x, int y)
        {
            var data = Data;
            return data[y * Width + x];
        }

        /// <inheritdoc />
        public IPixelData Rescale(double scale)
        {
            var w = (int)(Width * scale);
            var h = (int)(Height * scale);
            if (w == Width && h == Height) return this;

            var data = BilinearInterpolation.RescaleGrayscale(Data, Width, Height, w, h);
            return new GrayscalePixelDataU16Override(w, h, data);
        }

        public IPixelDataOverride RescaleOverride(double scale)
        {
            var w = (int)(Width * scale);
            var h = (int)(Height * scale);
            if (w == Width && h == Height) return this;

            var data = BilinearInterpolation.RescaleGrayscale(Data, Width, Height, w, h);
            return new GrayscalePixelDataU16Override(w, h, data);
        }
        /// <inheritdoc />
        public void Render(ILUT lut, int[] output)
        {
            var data = Data;
            if (lut == null)
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = data[i];
                    }
                }
                );
            }
            else
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = (int)lut[data[i]];
                    }
                }
                );
            }
        }

        public unsafe void Render(ILUT lut, int* output)
        {
            var data = Data;
            if (lut == null)
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = data[i];
                    }
                }
                );
            }
            else
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = (int)lut[data[i]];
                    }
                }
                );
            }
        }
        /// <inheritdoc />
        public Histogram GetHistogram(int channel)
        {
            if (channel != 0) throw new ArgumentOutOfRangeException(nameof(channel), channel, "Expected channel 0 for grayscale image.");

            var histogram = new Histogram(_bits.MinimumValue, _bits.MaximumValue);

            var data = Data;
            for (var i = 0; i < data.Length; i++) histogram.Add(data[i]);

            return histogram;
        }

        #endregion
    }

    /// <summary>
    /// Grayscale signed 32 bits <see cref="IPixelDataOverride"/> implementation
    /// </summary>
    public class GrayscalePixelDataS32Override : IPixelDataOverride
    {
        #region Private Members

        #endregion

        #region Public Constructor

        /// <summary>
        /// Initializes an instance of the <see cref="GrayscalePixelDataS32Override"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="bitDepth">Bit depth of pixel data.</param>
        /// <param name="data">Byte data buffer.</param>
        public GrayscalePixelDataS32Override(int width, int height, BitDepth bitDepth, IByteBuffer data)
        {
            Width = width;
            Height = height;

            var intData = ByteConverter.ToArray<int>(data, bitDepth.BitsAllocated);

            // Normally, HighBit == BitsStored-1, and thus shiftLeft == shiftRight, and the two
            // shifts in the loop below just replaces the top shift bits by the sign bit.
            // Separating shiftLeft from shiftRight handles exotic cases where low-order bits
            // should also be discarded.
            int shiftLeft = bitDepth.BitsAllocated - bitDepth.HighBit - 1;
            int shiftRight = bitDepth.BitsAllocated - bitDepth.BitsStored;
            Parallel.For(0, Height, y =>
            {
                for (int i = Width * y, e = i + Width; i < e; i++)
                {
                    // Remove masked high and low bits by shifting them out of the data type,
                    // getting the sign correct using arithmetic (sign-extending) right shift.
                    var d = intData[i] << shiftLeft;
                    intData[i] = d >> shiftRight;
                }
            }
            );

            Data = intData;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="GrayscalePixelDataU32"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="data">Pixel data in internal data format.</param>
        private GrayscalePixelDataS32Override(int width, int height, int[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        #endregion

        #region Public Properties

        /// <inheritdoc />
        public int Width { get; }

        /// <inheritdoc />
        public int Height { get; }

        /// <inheritdoc />
        public int Components => 1;

        /// <summary>
        /// Gets pixel data in internal format.
        /// </summary>
        public int[] Data { get; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public DicomRange<double> GetMinMax(int padding)
        {
            if (Data == null || Data.Length == 0)
            {
                return default(DicomRange<double>);
            }

            var range = new DicomRange<double>(double.MaxValue, double.MinValue);
            Data.Where(v => v != padding).Each(v => range.Join(v));
            return range;
        }

        /// <inheritdoc />
        public DicomRange<double> GetMinMax()
        {
            if (Data == null || Data.Length == 0)
            {
                return default(DicomRange<double>);
            }

            var range = new DicomRange<double>(double.MaxValue, double.MinValue);
            Data.Each(v => range.Join(v));
            return range;
        }

        /// <inheritdoc />
        public double GetPixel(int x, int y)
        {
            return Data[y * Width + x];
        }

        /// <inheritdoc />
        public IPixelData Rescale(double scale)
        {
            var w = (int)(Width * scale);
            var h = (int)(Height * scale);
            if (w == Width && h == Height) return this;

            var data = BilinearInterpolation.RescaleGrayscale(Data, Width, Height, w, h);
            return new GrayscalePixelDataS32Override(w, h, data);
        }

        public IPixelDataOverride RescaleOverride(double scale)
        {
            var w = (int)(Width * scale);
            var h = (int)(Height * scale);
            if (w == Width && h == Height) return this;

            var data = BilinearInterpolation.RescaleGrayscale(Data, Width, Height, w, h);
            return new GrayscalePixelDataS32Override(w, h, data);
        }
        /// <inheritdoc />
        public void Render(ILUT lut, int[] output)
        {
            var data = Data;
            if (lut == null)
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = data[i];
                    }
                }
                );
            }
            else
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = (int)lut[data[i]];
                    }
                }
                );
            }
        }
        public unsafe void Render(ILUT lut, int* output)
        {
            var data = Data;
            if (lut == null)
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = data[i];
                    }
                }
                );
            }
            else
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = (int)lut[data[i]];
                    }
                }
                );
            }
        }

        /// <inheritdoc />
        public Histogram GetHistogram(int channel)
        {
            throw new NotSupportedException("Histograms are not supported for signed 32-bit images.");
        }

        #endregion
    }

    /// <summary>
    /// Grayscale unsigned 32 bits <see cref="IPixelDataOverride"/> implementation
    /// </summary>
    public class GrayscalePixelDataU32Override : IPixelDataOverride
    {
        #region Private Members

        #endregion

        #region Public Constructor

        /// <summary>
        /// Initializes an instance of the <see cref="GrayscalePixelDataU32"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="bitDepth">Bit depth of pixel data.</param>
        /// <param name="data">Byte data buffer.</param>
        public GrayscalePixelDataU32Override(int width, int height, BitDepth bitDepth, IByteBuffer data)
        {
            Width = width;
            Height = height;

            var uintData = ByteConverter.ToArray<uint>(data, bitDepth.BitsAllocated);

            if (bitDepth.BitsStored != 32)
            {
                // Normally, HighBit == BitsStored-1, and thus shiftLeft == shiftRight, and the two
                // shifts in the loop below just zeroes the top shift bits.
                // Separating shiftLeft from shiftRight handles exotic cases where low-order bits
                // should also be discarded.
                int shiftLeft = bitDepth.BitsAllocated - bitDepth.HighBit - 1;
                int shiftRight = bitDepth.BitsAllocated - bitDepth.BitsStored;

                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        // Remove masked high and low bits by shifting them out of the data type. 
                        var d = uintData[i] << shiftLeft;
                        uintData[i] = d >> shiftRight;
                    }
                }
                );
            }

            Data = uintData;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="GrayscalePixelDataU32Override"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="data">Pixel data in internal data format.</param>
        private GrayscalePixelDataU32Override(int width, int height, uint[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        #endregion

        #region Public Properties

        /// <inheritdoc />
        public int Width { get; }

        /// <inheritdoc />
        public int Height { get; }

        /// <inheritdoc />
        public int Components => 1;

        /// <summary>
        /// Gets pixel data in internal format.
        /// </summary>
        public uint[] Data { get; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public DicomRange<double> GetMinMax(int padding)
        {
            if (Data == null || Data.Length == 0)
            {
                return default(DicomRange<double>);
            }

            var range = new DicomRange<double>(double.MaxValue, double.MinValue);
            Data.Where(v => v != padding).Each(v => range.Join(v));
            return range;
        }

        /// <inheritdoc />
        public DicomRange<double> GetMinMax()
        {
            if (Data == null || Data.Length == 0)
            {
                return default(DicomRange<double>);
            }

            var range = new DicomRange<double>(double.MaxValue, double.MinValue);
            Data.Each(v => range.Join(v));
            return range;
        }

        /// <inheritdoc />
        public double GetPixel(int x, int y)
        {
            return Data[y * Width + x];
        }

        /// <inheritdoc />
        public IPixelData Rescale(double scale)
        {
            var w = (int)(Width * scale);
            var h = (int)(Height * scale);
            if (w == Width && h == Height) return this;

            var data = BilinearInterpolation.RescaleGrayscale(Data, Width, Height, w, h);
            return new GrayscalePixelDataU32Override(w, h, data);
        }

        public IPixelDataOverride RescaleOverride(double scale)
        {
            var w = (int)(Width * scale);
            var h = (int)(Height * scale);
            if (w == Width && h == Height) return this;

            var data = BilinearInterpolation.RescaleGrayscale(Data, Width, Height, w, h);
            return new GrayscalePixelDataU32Override(w, h, data);
        }
        /// <inheritdoc />
        public void Render(ILUT lut, int[] output)
        {
            var data = Data;
            if (lut == null)
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = (int)data[i];
                    }
                }
                );
            }
            else
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = unchecked((int)lut[(int)data[i]]);
                    }
                }
                );
            }
        }

        public unsafe void Render(ILUT lut, int* output)
        {
            var data = Data;
            if (lut == null)
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = (int)data[i];
                    }
                }
                );
            }
            else
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width; i < e; i++)
                    {
                        output[i] = unchecked((int)lut[(int)data[i]]);
                    }
                }
                );
            }
        }
        /// <inheritdoc />
        public Histogram GetHistogram(int channel)
        {
            throw new NotSupportedException("Histograms are not supported for unsigned 32-bit images.");
        }

        #endregion
    }

    /// <summary>
    /// Color 24 bits <see cref="IPixelData"/> implementation used for RGB
    /// </summary>
    public class ColorPixelData24Override : IPixelDataOverride
    {
        #region Private Members

        #endregion

        #region Public Constructor

        /// <summary>
        /// Initializes an instance of the <see cref="ColorPixelData24"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="data">Byte data buffer.</param>
        public ColorPixelData24Override(int width, int height, IByteBuffer data)
        {
            Width = width;
            Height = height;
            Data = data.Data;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="ColorPixelData24"/> class.
        /// </summary>
        /// <param name="width">Pixel data width.</param>
        /// <param name="height">Pixel data height.</param>
        /// <param name="data">Pixel data in internal data format.</param>
        private ColorPixelData24Override(int width, int height, byte[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        #endregion

        #region Public Properties

        /// <inheritdoc />
        public int Width { get; }

        /// <inheritdoc />
        public int Height { get; }

        /// <inheritdoc />
        public int Components => 3;

        /// <summary>
        /// Gets pixel data in byte array format.
        /// </summary>
        public byte[] Data { get; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public DicomRange<double> GetMinMax(int padding)
        {
            throw new InvalidOperationException(
                "Calculation of min/max pixel values is not supported for 24-bit color pixel data.");
        }

        /// <inheritdoc />
        public DicomRange<double> GetMinMax()
        {
            throw new InvalidOperationException(
                "Calculation of min/max pixel values is not supported for 24-bit color pixel data.");
        }

        /// <inheritdoc />
        public double GetPixel(int x, int y)
        {
            var data = Data;
            var p = (y * Width + x) * 3;
            return (data[p++] << 16) | (data[p++] << 8) | data[p];
        }

        /// <inheritdoc />
        public IPixelData Rescale(double scale)
        {
            var w = (int)(Width * scale);
            var h = (int)(Height * scale);
            if (w == Width && h == Height) return this;

            var data = BilinearInterpolation.RescaleColor24(Data, Width, Height, w, h);
            return new ColorPixelData24Override(w, h, data);
        }

        public IPixelDataOverride RescaleOverride(double scale)
        {
            var w = (int)(Width * scale);
            var h = (int)(Height * scale);
            if (w == Width && h == Height) return this;

            var data = BilinearInterpolation.RescaleColor24(Data, Width, Height, w, h);
            return new ColorPixelData24Override(w, h, data);
        }
        /// <inheritdoc />
        public void Render(ILUT lut, int[] output)
        {
            const int alphaFF = 0xff << 24;

            var data = Data;
            if (lut == null)
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width, p = i * 3; i < e; i++)
                    {
                        output[i] = alphaFF | (data[p++] << 16) | (data[p++] << 8) | data[p++];
                    }
                }
                );
            }
            else
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width, p = i * 3; i < e; i++)
                    {
                        output[i] = alphaFF | ((int)lut[data[p++]] << 16) | ((int)lut[data[p++]] << 8) | (int)lut[data[p++]];
                    }
                }
                );
            }
        }

        public unsafe void Render(ILUT lut, int* output)
        {
            const int alphaFF = 0xff << 24;

            var data = Data;
            if (lut == null)
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width, p = i * 3; i < e; i++)
                    {
                        output[i] = alphaFF | (data[p++] << 16) | (data[p++] << 8) | data[p++];
                    }
                }
                );
            }
            else
            {
                Parallel.For(0, Height, y =>
                {
                    for (int i = Width * y, e = i + Width, p = i * 3; i < e; i++)
                    {
                        output[i] = alphaFF | ((int)lut[data[p++]] << 16) | ((int)lut[data[p++]] << 8) | (int)lut[data[p++]];
                    }
                }
                );
            }
        }
        /// <inheritdoc />
        public Histogram GetHistogram(int channel)
        {
            if (channel < 0 || channel > 2)
                throw new ArgumentOutOfRangeException(
                    nameof(channel),
                    channel,
                    "Expected channel between 0 and 2 for 24-bit color image.");

            var histogram = new Histogram(byte.MinValue, byte.MaxValue);

            var data = Data;
            for (var i = channel; i < data.Length; i += 3) histogram.Add(data[i]);

            return histogram;
        }

        #endregion
    }
}
