using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

using PdfSharp.Pdf;
using com.eightlabs.BulkImageToPdf.ViewModels;
using System.IO;

namespace com.eightlabs.BulkImageToPdf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private MainWindowViewModel vm;

        public MainWindow()
        {
            vm = new MainWindowViewModel();
            DataContext = vm;

            InitializeComponent();

            vm.Screens.CurrentChanged += new EventHandler(Screens_CurrentChanged);

        }

        void Screens_CurrentChanged(object sender, EventArgs e)
        {
            //fade it in
            // Create a storyboard to contain the animations.
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, 250);

            // Create a DoubleAnimation to fade the not selected option control
            DoubleAnimation animation = new DoubleAnimation();

            animation.From = 0.0;
            animation.To = 1.0;
            animation.Duration = new Duration(duration);
            // Configure the animation to target de property Opacity
            Storyboard.SetTargetName(animation, this.content.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Control.OpacityProperty));
            // Add the animation to the storyboard
            storyboard.Children.Add(animation);

            // Begin the storyboard
            storyboard.Begin(this);
        }

      
        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data is System.Windows.DataObject && ((System.Windows.DataObject)e.Data).ContainsFileDropList())
            {
                StringCollection files = ((System.Windows.DataObject)e.Data).GetFileDropList();

                if (files.Count > 0)
                {
                    this.vm.AddFiles(files);

                    this.vm.Screens.MoveCurrentToNext();
                }

            }
        }

        private void view_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                //fade it in
                // Create a storyboard to contain the animations.
                Storyboard storyboard = new Storyboard();
                TimeSpan duration = new TimeSpan(0, 0, 0, 0, 250);

                // Create a DoubleAnimation to fade the not selected option control
                DoubleAnimation animation = new DoubleAnimation();

                animation.From = 0.0;
                animation.To = 1.0;
                animation.Duration = new Duration(duration);
                // Configure the animation to target de property Opacity
                Storyboard.SetTargetName(animation, ((FrameworkElement)sender).Name);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Control.OpacityProperty));
                // Add the animation to the storyboard
                storyboard.Children.Add(animation);

                // Begin the storyboard
                storyboard.Begin(this);
            }
        }

    }
}
