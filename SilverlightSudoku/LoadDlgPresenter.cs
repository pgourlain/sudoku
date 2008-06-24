using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Linq;
using System.Xml;

namespace SilverlightSudoku
{
    public class LoadDlgPresenter : BasePresenter<LoadDlg>
    {
        public static string FILENAME = "sudokuparties.xml";

        public LoadDlgPresenter(LoadDlg dlg)
            : base(dlg)
        {
        }

        internal void ViewReady()
        {
            EnumerateFiles();
        }

        private void EnumerateFiles()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(FILENAME))
                {
                    IsolatedStorageFileStream stm = isf.OpenFile(FILENAME, System.IO.FileMode.Open);
                    using (stm)
                    {
                        using (XmlReader xr = XmlReader.Create(stm))
                        {
                            XDocument doc = XDocument.Load(xr);
                            var qry = from partie in doc.Descendants("Partie")
                                      select new PartiePresenter { ID = partie.Attribute("ID").Value, Date = Convert.ToDateTime(partie.Attribute("Date").Value) };

                            Parties = qry.ToArray();
                        }
                    }
                }
            }
        }


        private PartiePresenter _Selected;
        public PartiePresenter Selected
        {
            get { return _Selected; }
            set
            {
                if (value != _Selected)
                {
                    _Selected = value;
                    NotifyPropertyChanged("Selected");
                    HasSelection = _Selected != null;
                }
            }
        }



        private bool _HasSelection;
        public bool HasSelection
        {
            get { return _HasSelection; }
            set
            {
                if (value != _HasSelection)
                {
                    _HasSelection = value;
                    NotifyPropertyChanged("HasSelection");
                }
            }
        }

        private PartiePresenter[] _Parties;
        public PartiePresenter[] Parties
        {
            get { return _Parties; }
            set
            {
                if (value != _Parties)
                {
                    _Parties = value;
                    NotifyPropertyChanged("Parties");
                }
            }
        }
    }

    public class PartiePresenter
    {
        public string ID { get; set; }
        public DateTime Date { get; set; }
    }
}
