using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using DicomView.Behaviors.FellowOakDicomImage;
using DicomView.Behaviors.DicomBehaviors;
using DicomView.Behaviors.ROI;

namespace DicomView.Display
{
    internal class MainWindowViewModel : BindableBase
    {
        ObservableCollection<Fo_DicomCTImage> images;
        public ObservableCollection<Fo_DicomCTImage> Images
        {
            get { return images; }
            set { SetProperty(ref images, value); }
        }

        ObservableCollection<Fo_DicomCTImage> images2;
        public ObservableCollection<Fo_DicomCTImage> Images2
        {
            get { return images2; }
            set { SetProperty(ref images2, value); }
        }

        private BehaviorType _leftButton = BehaviorType.None;
        public BehaviorType LeftButton
        {
            get { return _leftButton; }
            set { SetProperty(ref _leftButton, value); }
        }

        private ROIType? _currentROI = null;
        public ROIType? CurrentROI
        {
            get { return _currentROI; }
            set
            {
                SetProperty(ref _currentROI, value);
            }
        }

        private BehaviorType _rightButton = BehaviorType.None;
        public BehaviorType RightButton
        {
            get { return _rightButton; }
            set { SetProperty(ref _rightButton, value); }
        }

        public MainWindowViewModel()
        {
            images = new ObservableCollection<Fo_DicomCTImage>();
            images2 = new ObservableCollection<Fo_DicomCTImage>();
            images.Add(new Fo_DicomCTImage(DicomFile.Open(@"..\..\..\TestData\GH1728.dcm")));
            images.Add(new Fo_DicomCTImage(DicomFile.Open(@"..\..\..\TestData\GH1308.dcm")));
            images.Add(new Fo_DicomCTImage(DicomFile.Open(@"..\..\..\TestData\GH1728.dcm")));
            images.Add(new Fo_DicomCTImage(DicomFile.Open(@"..\..\..\TestData\GH1728.dcm")));
            images2.Add(new Fo_DicomCTImage(DicomFile.Open(@"..\..\..\TestData\GH1728.dcm")));
            images2.Add(new Fo_DicomCTImage(DicomFile.Open(@"..\..\..\TestData\GH1308.dcm")));
            images2.Add(new Fo_DicomCTImage(DicomFile.Open(@"..\..\..\TestData\GH1728.dcm")));
            images2.Add(new Fo_DicomCTImage(DicomFile.Open(@"..\..\..\TestData\GH1728.dcm")));
        }
    }
}
