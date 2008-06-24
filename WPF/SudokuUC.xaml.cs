using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFGeniusSudoku.Services;
using WPFGeniusSudoku.Dialogs;
using System.Windows.Media.Animation;

namespace WPFGeniusSudoku
{
    /// <summary>
    /// Interaction logic for SudokuUC.xaml
    /// </summary>
    public partial class SudokuUC : UserControl
    {
        public SudokuUC()
        {
            InitializeComponent();
            //object style = this.Resources["CellStyle"];
            //if (style is Style)
            //{
            //    object o = ((Style)style).Resources["sbOnPlayGood"];
            //    ((Storyboard)o).Completed += new EventHandler(SudokuUC_Completed);
            //}
        }

        void SudokuUC_Completed(object sender, EventArgs e)
        {
            SayGood(sender, null);
        }

        void OnNewGame(object sendr, RoutedEventArgs e)
        {
            bool value = rbEasy.IsChecked?? false;
            sudokugrid1.NewGame( value ? 0 : 1 );
        }

        void OnClearUserNotes(object sender, RoutedEventArgs e)
        {
            sudokugrid1.ClearNotes();
        }

        void OnAbout(object sender, RoutedEventArgs e)
        {
            AboutUC uc = new AboutUC();
            ModalService.ShowModal(uc);
        }

        void OnGameWon(object sender, RoutedEventArgs e)
        {
            WonUC uc = new WonUC();
            ModalService.ShowModal(uc);
        }

        void SayGood(object sender, RoutedEventArgs e)
        {
            Say("Good");
        }

        private void Say(string p)
        {
            //SpeechSynthesizer s = new SpeechSynthesizer();
            //s.SpeakAsync("Good");
        }

        void SayBad(object sender, RoutedEventArgs e)
        {
            Say("Bad");
        }
    }
}