using System;
using System.Collections.Generic;
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
using WPFGeniusSudoku.Services;

namespace WPFGeniusSudoku.Dialogs
{
    /// <summary>
    /// Interaction logic for WonUC.xaml
    /// </summary>

    public partial class WonUC : System.Windows.Controls.UserControl
    {
        public WonUC()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(WonUC_Loaded);
        }

        void WonUC_Loaded(object sender, RoutedEventArgs e)
        {
            DialogStringAnimation.AddStringAnimation("Won !", this.tilePanel, 100);
        }

        void CloseClick(object sender, RoutedEventArgs e)
        {
            ModalService.CloseModal();
        }
    }
}