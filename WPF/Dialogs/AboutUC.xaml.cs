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
using Microsoft.Samples.KMoore.WPFSamples.AnimatingTilePanel;
using WPFGeniusSudoku.Dialogs;
using System.Diagnostics;

namespace WPFGeniusSudoku.Dialogs
{
    /// <summary>
    /// Interaction logic for AboutUC.xaml
    /// </summary>
    public partial class AboutUC : System.Windows.Controls.UserControl
    {
        public AboutUC()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(AboutUC_Loaded);
        }

        void AboutUC_Loaded(object sender, RoutedEventArgs e)
        {
            String st = "Genius Sudoku";

            DialogStringAnimation.AddStringAnimation(st, this.tilePanel, 40);
        }

        void CloseClick(object sender, RoutedEventArgs e)
        {
            ModalService.CloseModal();
        }

        void OnGotoSite(object sender, RoutedEventArgs e)
        {
            string url = ((Hyperlink)sender).NavigateUri.OriginalString;
            Process.Start(new ProcessStartInfo(url));
        }
    }
}