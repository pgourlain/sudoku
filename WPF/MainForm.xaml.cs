using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using WPFGeniusSudoku.Services;

namespace WPFGeniusSudoku
{
    /// <summary>
    /// Interaction logic for MainForm.xaml
    /// </summary>
    public partial class MainForm : Window
    {
        public MainForm()
        {
            InitializeComponent();
            ModalService.ModalHost = this.ModalHost;
            ModalService.ModalHostContent = this.ModalContent;
        }

        void SizeWindow(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            int xChange = (int)e.HorizontalChange;
            int yChange = (int)e.VerticalChange;

            double x = this.Left;
            double y = this.Top;
            double width = this.Width;
            double height = this.Height;

            switch (((FrameworkElement)sender).Name)
            {
                case "sizeWindowUpLeft":
                    x += xChange;
                    y += yChange;
                    width += -xChange;
                    height += -yChange;
                    break;
                case "sizeWindowUp":
                    y += yChange;
                    height += -yChange;
                    break;
                case "sizeWindowUpRight":
                    y += yChange;
                    width += xChange;
                    height += -yChange;
                    break;
                case "sizeWindowLeft":
                    x += xChange;
                    width += -xChange;
                    break;
                case "sizeWindowRight":
                    width += xChange;
                    break;
                case "sizeWindowDownLeft":
                    x += xChange;
                    width += -xChange;
                    height += yChange;
                    break;
                case "sizeWindowDown":
                    height += yChange;
                    break;
                case "sizeWindowDownRight":
                    width += xChange;
                    height += yChange;
                    break;
            }
            this.Left = x;
            this.Top = y;
            this.Height = height;
            this.Width = width;
        }

        void MoveWindow(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            int xChange = (int)e.HorizontalChange;
            int yChange = (int)e.VerticalChange;
            this.Left += xChange;
            this.Top += yChange;
        }

        void CloseApplication(object sender, System.Windows.RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        void MinimizeApplication(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        void MaximizeApplication(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        void OnModalClose(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ModalHost.Visibility = Visibility.Collapsed;
            foreach (UIElement element in this.ModalContent.Children)
            {
                if (element is IDisposable)
                    ((IDisposable)element).Dispose();
            }
            this.ModalContent.Children.Clear();
        }
    }
}