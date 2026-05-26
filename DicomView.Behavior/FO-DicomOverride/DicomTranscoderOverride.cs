using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.Codec;
using FellowOakDicom.Imaging.Render;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Input;

namespace DicomView.Behaviors
{
    internal class DicomTranscoderOverride : DicomTranscoder
    {
        private MethodInfo decode;
        private PropertyInfo inputCodec;
        public DicomTranscoderOverride(
            DicomTransferSyntax inputSyntax,
            DicomTransferSyntax outputSyntax,
            DicomCodecParams inputCodecParams = null,
            DicomCodecParams outputCodecParams = null) : base(inputSyntax, outputSyntax, inputCodecParams, outputCodecParams) 
        {
            decode = typeof(DicomTranscoder).GetMethod("Decode", BindingFlags.NonPublic | BindingFlags.Instance);
            inputCodec = typeof(DicomTranscoder).GetProperty("InputCodec", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public IPixelDataOverride DecodePixelDataOverride(DicomDataset dataset, int frame)
        {
            var pixelData = DicomPixelData.Create(dataset);

            // is pixel data already uncompressed?
            if (!dataset.InternalTransferSyntax.IsEncapsulated)
            {
                return PixelDataFactoryOverride.Create(pixelData, frame);
            }

            var buffer = pixelData.GetFrame(frame);

            // clone dataset to prevent changes to source
            var cloneDataset = dataset.Clone();

            var oldPixelData = DicomPixelData.Create(cloneDataset, true);
            oldPixelData.AddFrame(buffer);

            var newDataset = decode.Invoke(this, new object[] { cloneDataset, OutputSyntax, inputCodec.GetValue(this), InputCodecParams }) as DicomDataset;
            var newPixelData = DicomPixelData.Create(newDataset);

            return PixelDataFactoryOverride.Create(newPixelData, 0);
        }
    }
}
