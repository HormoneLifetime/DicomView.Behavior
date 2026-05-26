using DicomView.Behaviors;
using DicomView.Behaviors.DicomBehaviors;
using DicomView.Behaviors.DicomBehaviors.BehaviorCore;
using DicomView.Behaviors.DicomBehaviors.Controller;
using DicomView.Behaviors.DicomInterface;
using DicomView.Behaviors.FellowOakDicomImage;
using DicomView.Behaviors.ROI;
using DicomView.Behaviors.ROI.Drawers;
using DicomView.Behaviors.ROI.Shapes;
using Microsoft.Xaml.Behaviors;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using static System.Resources.ResXFileRef;

namespace DicomView.Display
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        private void TestAdjustWindow(Vector vector)
        {
            foreach (var image in (this.DataContext as MainWindowViewModel).Images)
            {
                image.AdjustWindow(vector);
            }
        }

        private void TestAdjustWindow2(Vector vector)
        {
            foreach (var image in (this.DataContext as MainWindowViewModel).Images2)
            {
                image.AdjustWindow(vector);
            }
        }

        private void ItemsMoved(FrameworkElement source, FrameworkElement target)
        {
            var collection = (this.DataContext as MainWindowViewModel).Images;
            var oldIndex = collection.IndexOf(source.DataContext as Fo_DicomCTImage);
            var newIndex = collection.IndexOf(target.DataContext as Fo_DicomCTImage);
            collection.Move(oldIndex, newIndex);
        }

        private void ItemsMoved2(FrameworkElement source, FrameworkElement target)
        {
            var collection = (this.DataContext as MainWindowViewModel).Images2;
            var oldIndex = collection.IndexOf(source.DataContext as Fo_DicomCTImage);
            var newIndex = collection.IndexOf(target.DataContext as Fo_DicomCTImage);
            collection.Move(oldIndex, newIndex);
        }

        private void AddROI(DicomROIBaseShape shape)
        {
            foreach (var image in (this.DataContext as MainWindowViewModel).Images)
            {
                var roi = shape.Clone() as DicomROIBaseShape;
                roi.SpacingX = image.SpacingX;
                roi.SpacingY = image.SpacingY;
                roi.SetAttach(image);
                image.AddROI(roi);
            }
        }

        private void AddROI2(DicomROIBaseShape shape)
        {
            foreach (var image in (this.DataContext as MainWindowViewModel).Images2)
            {
                var roi = shape.Clone() as DicomROIBaseShape;
                roi.SpacingX = image.SpacingX;
                roi.SpacingY = image.SpacingY;
                roi.SetAttach(image);
                image.AddROI(roi);
            }
        }

        private void DicomImageShell_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            behaviorsController.SetAttach(sender as DicomImageShell, (TriggerButton)Math.Pow(2, (double)e.ChangedButton));
        }

        private void GenerateContextMenu(FrameworkElement obj)
        {
            var behaviors = Interaction.GetBehaviors(obj);
            obj.ContextMenu = obj.ContextMenu ?? new ContextMenu();
            foreach (var behavior in behaviors)
            {
                if (behavior is IBehaviorController<DicomImageShell> controller)
                {
                    foreach (var core in controller.BehaviorCores)
                    {
                        if (core is IToMenuItem item)
                            obj.ContextMenu.Items.Add(item.ToMenuItem());
                    }
                }
            }
        }

        private void images_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateContextMenu(images);
        }

        private void DicomImageShell_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateContextMenu(sender as FrameworkElement);
        }
    }
}