using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightSudoku
{
    public partial class LoadDlg : UserControl, IWpfView
    {
        public LoadDlg()
        {
            Presenter = new LoadDlgPresenter(this);
            InitializeComponent();
            this.DataContext = Presenter;
            this.Loaded += new RoutedEventHandler(LoadDlg_Loaded);
        }

        void LoadDlg_Loaded(object sender, RoutedEventArgs e)
        {
            Presenter.ViewReady();
        }

        public LoadDlgPresenter Presenter { get; private set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (OnClose != null)
                OnClose(this, EventArgs.Empty);
        }

        public event EventHandler OnClose;

        public bool IsCanceled { get; set; }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
            if (OnClose != null)
                OnClose(this, EventArgs.Empty);

        }
    }
}
