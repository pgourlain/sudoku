using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Linq;


/*
 * TODO: sauver les notes de l'utilisateur, dans le xml
 * */
namespace GeniusSudoku.Core
{
    /// <summary>
    /// classe de gestion d'une grille sudoku
    /// </summary>
    public class Sudoku
    {
        /// <summary>
        /// tableau [,] moins performant que [][], mais plus pratique pour l'allocation
        /// </summary>
        private byte[,] _Cases;
        private byte[,] _Origines;
        private byte[,] _Solution;
        /// <summary>
        /// chiffre restant dans les colonnes
        /// </summary>
        private List<byte[]> _Colonnes;
        /// <summary>
        /// chiffres restant dans les lignes
        /// </summary>
        private List<byte[]> _Lignes;
        /// <summary>
        /// chiffres restant dans les block
        /// </summary>
        private List<byte[]> _Block;
        /// <summary>
        /// nombre de case restantes à remplir
        /// </summary>
        private int _NumberRemaining;
        private int _Rate;
        private string _IDPartie;
        private bool[,,,] _CheckedCell;
        private bool[,,,] _FlagCell;
        private int[] _Remainings; 

        public event EventHandler<EventArgs> OnFlagChanged;
        public event EventHandler<EventArgs> OnCheckChanged;
        public event EventHandler<EventArgs> OnGameBegin;
        public event EventHandler<EventArgs> OnCoupAdded;
        public event EventHandler<EventArgs> OnWin;
        public event EventHandler<RemainingEventArgs> OnRemainingChanged;

        public Sudoku()
        {
            _Colonnes = new List<byte[]>(9);
            _Lignes = new List<byte[]>(9);
            _Block = new List<byte[]>(9);
            _Remainings = new int[9];
            _IDPartie = "0";
            Init();
        }

        #region méthodes privées
        private byte[] NewLine()
        {
            return new byte[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }

        private byte[,] CopyTableau(byte[][] aTableau)
        {
            byte[,] Result = new byte[9, 9];
            int y = 0;
            foreach (byte[] ligne in aTableau)
            {
                int x = 0;
                foreach (byte value in ligne)
                    Result[x++, y] = value;
                y++;
            }
            return Result;
        }

        private byte[,] CopyTableau(byte[,] aTableau)
        {
            byte[,] copy = new byte[9, 9];
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    copy[y, x] = aTableau[y, x];

            return copy;
        }

        private void Init()
        {
            _Colonnes.Clear();
            _Lignes.Clear();
            _Block.Clear();
            _Origines = new byte[9, 9];
            _Cases = new byte[9, 9];
            _CheckedCell = new bool[9, 9, 9, 9];
            _FlagCell = new bool[9, 9, 9, 9];
            _NumberRemaining = 81;
            
            for (int i = 0; i < 9; i++)
            {
                _Colonnes.Add(NewLine());
                _Lignes.Add(NewLine());
                _Block.Add(NewLine());
                _Remainings[i] = 9;
            }

        }
        #region Résolution
        private struct point
        {
            public int x, y;
            public point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        // Maps sub square index to m_sudoku
        private point[,] m_subIndex =
            new point[,]
			{
				{ new point(0,0),new point(0,1),new point(0,2),new point(1,0),new point(1,1),new point(1,2),new point(2,0),new point(2,1),new point(2,2)},
				{ new point(0,3),new point(0,4),new point(0,5),new point(1,3),new point(1,4),new point(1,5),new point(2,3),new point(2,4),new point(2,5)},
				{ new point(0,6),new point(0,7),new point(0,8),new point(1,6),new point(1,7),new point(1,8),new point(2,6),new point(2,7),new point(2,8)},
				{ new point(3,0),new point(3,1),new point(3,2),new point(4,0),new point(4,1),new point(4,2),new point(5,0),new point(5,1),new point(5,2)},
				{ new point(3,3),new point(3,4),new point(3,5),new point(4,3),new point(4,4),new point(4,5),new point(5,3),new point(5,4),new point(5,5)},
				{ new point(3,6),new point(3,7),new point(3,8),new point(4,6),new point(4,7),new point(4,8),new point(5,6),new point(5,7),new point(5,8)},
				{ new point(6,0),new point(6,1),new point(6,2),new point(7,0),new point(7,1),new point(7,2),new point(8,0),new point(8,1),new point(8,2)},
				{ new point(6,3),new point(6,4),new point(6,5),new point(7,3),new point(7,4),new point(7,5),new point(8,3),new point(8,4),new point(8,5)},
				{ new point(6,6),new point(6,7),new point(6,8),new point(7,6),new point(7,7),new point(7,8),new point(8,6),new point(8,7),new point(8,8)}
		};

        // Maps sub square to index
        private byte[,] m_subSquare =
            new byte[,]
			{
				{0,0,0,1,1,1,2,2,2},
				{0,0,0,1,1,1,2,2,2},
				{0,0,0,1,1,1,2,2,2},
				{3,3,3,4,4,4,5,5,5},
				{3,3,3,4,4,4,5,5,5},
				{3,3,3,4,4,4,5,5,5},
				{6,6,6,7,7,7,8,8,8},
				{6,6,6,7,7,7,8,8,8},
				{6,6,6,7,7,7,8,8,8}
		};

        private bool Solve(byte[,] Result)
        {
            // Find untouched location with most information
            int xp = 0;
            int yp = 0;
            byte[] Mp = null;
            int cMp = 10;

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    // Is this spot unused?
                    if (Result[y, x] == 0)
                    {
                        // Set M of possible solutions
                        byte[] M = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                        // Remove used numbers in the vertical direction
                        for (int a = 0; a < 9; a++)
                            M[Result[a, x]] = 0;

                        // Remove used numbers in the horizontal direction
                        for (int b = 0; b < 9; b++)
                            M[Result[y, b]] = 0;

                        // Remove used numbers in the sub square.
                        int squareIndex = m_subSquare[y, x];
                        for (int c = 0; c < 9; c++)
                        {
                            point p = m_subIndex[squareIndex, c];
                            M[Result[p.x, p.y]] = 0;
                        }

                        int cM = 0;
                        // Calculate cardinality of M
                        for (int d = 1; d < 10; d++)
                            cM += M[d] == 0 ? 0 : 1;

                        // Is there more information in this spot than in the best yet?
                        if (cM < cMp)
                        {
                            cMp = cM;
                            Mp = M;
                            xp = x;
                            yp = y;
                        }
                    }
                }
            }

            // Finished?
            if (cMp == 10)
                return true;

            // Couldn't find a solution?
            if (cMp == 0)
                return false;

            // Try elements
            for (int i = 1; i < 10; i++)
            {
                if (Mp[i] != 0)
                {
                    Result[yp, xp] = Mp[i];
                    if (Solve(Result))
                        return true;
                }
            }

            // Restore to original state.
            Result[yp, xp] = 0;
            return false;
        }
        #endregion

        /// <summary>
        /// est-ce que aNumbr est present dans la liste aNumbers
        /// </summary>
        /// <param name="aNumbers"></param>
        /// <param name="aNumber"></param>
        /// <returns></returns>
        private bool IsNumberPresent(byte[] aNumbers, int aNumber)
        {
            foreach (int value in aNumbers)
            {
                if (value == aNumber)
                    return true;
            }
            return false;
        }


        private void UpdateValues(int x, int y, int value)
        {
            int index = Math.Abs(value)-1;
            UpdateValue(_Colonnes, x, index, value);
            UpdateValue(_Lignes, y, index, value);
            //int a = x / 3 + (y / 3) * 3;
            int a = m_subSquare[y, x];
            UpdateValue(_Block, a, index, value);
            if (value < 0)
            {
                _NumberRemaining--;
                DoDecRemainning(index);
                CheckIsWin();
            }
            else if (value > 0)
            {
                _NumberRemaining++;
                DoIncRemainning(index);
            }
            System.Diagnostics.Debug.WriteLine(_NumberRemaining);
        }

        private void DoDecRemainning(int index)
        {
            _Remainings[index]--;
            if (OnRemainingChanged != null)
                OnRemainingChanged(this, new RemainingEventArgs(index + 1, _Remainings[index])); 
        }

        private void DoIncRemainning(int index)
        {
            _Remainings[index]++;
            if (OnRemainingChanged != null)
                OnRemainingChanged(this, new RemainingEventArgs(index + 1, _Remainings[index]));
        }

        private void UpdateValue(List<byte[]> aTableau, int x, int y, int value)
        {
            byte[] ligne = aTableau[x];
            value += ligne[y];
            ligne[y] = (byte)value;
            aTableau[x] = ligne;
        }

        private void CheckIsWin()
        {
            if (_NumberRemaining == 0)
            {
                if (IsWin())
                {
                    DoOnWin();
                }
            }
        }

        private void DoOnWin()
        {
            if (OnWin != null)
                OnWin(this, EventArgs.Empty);
        }

        private void DoCheckChanged()
        {
            if (OnCheckChanged != null)
                OnCheckChanged(this, EventArgs.Empty);
        }

        private void DoFlagChanged()
        {
            if (OnFlagChanged != null)
                OnFlagChanged(this, EventArgs.Empty);
        }

        #endregion

        public byte this[int x, int y]
        {
            get
            {
                return _Cases[x, y];
            }
        }

        public bool IsFixedNumber(int x, int y)
        {
            return _Origines[x, y] != 0;
        }

        public bool IsNumberPossible(int x, int y, byte aNumber)
        {
            if (x < 0 || x >= 9 || y < 0 || y >= 9 || aNumber <= 0 || aNumber > 9)
                return false;
            bool b = IsNumberPresent(_Colonnes[x], aNumber);
            b = b && IsNumberPresent(_Lignes[y], aNumber);
            x = m_subSquare[y, x];
            b = b && IsNumberPresent(_Block[x], aNumber);
            return b;
        }

        public void SetTableau(byte[,] aTableau)
        {
            Init();

            _Cases = CopyTableau(aTableau);
            _Origines = CopyTableau(aTableau);
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    byte value = aTableau[x, y];
                    if (value > 0)
                    {
                        UpdateValues(x, y, -value);
                    }
                }
            }
            _Solution = CopyTableau(_Origines);
            Solve(_Solution);
        }

        public void SetTableau(byte[][] aTableau)
        {
            SetTableau(CopyTableau(aTableau));
        }

        public void SetTableau(string aFixed, string aUser)
        {
            if (string.IsNullOrEmpty(aFixed))
                throw new Exception("problème dans la restauration depuis un fichier xml !!!");
            byte[,] aTab = new byte[9, 9];
            int index = 0;
            foreach (char c in aFixed)
            {
                int y = index / 9;
                int x = index % 9;
                aTab[x, y] = Byte.Parse(c.ToString());
                index++;
            }
            SetTableau(aTab);
            if (!String.IsNullOrEmpty(aUser))
            {
                index = 0;
                foreach (char c in aUser)
                {
                    if (c != '0')
                    {
                        int y = index / 9;
                        int x = index % 9;
                        AddCoup(x, y, Byte.Parse(c.ToString()));
                    }
                    index++;
                }
            }
        }

        public bool AddCoup(int x, int y, byte value)
        {
            if (_Origines[x, y] == 0)
            {
                byte oldvalue = _Cases[x, y];
                if (oldvalue > 0)
                {
                    UpdateValues(x, y, oldvalue);
                }
                _Cases[x, y] = value;
                if (value > 0)
                {
                    UpdateValues(x, y, -value);
                }
                if (OnCoupAdded != null)
                    OnCoupAdded(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <summary>
        /// nouvelle partie
        /// </summary>
        /// <param name="aCode"></param>
        /// <param name="aLevel"></param>
        public void New(string aCode, int aLevel)
        {
            _IDPartie = aCode;
            SudokuGenerator gen = new SudokuGenerator();
            SudokuData[] data = gen.Generate(int.Parse(aCode), 1, 1, aLevel);
            if (data.Length > 0)
            {
                SetTableau(data[0].Datas);
                _Rate = data[0].Rate;
            }
            if (OnGameBegin != null)
                OnGameBegin(this, EventArgs.Empty);
        }

        /// <summary>
        /// nouvelle partie
        /// </summary>
        /// <param name="aLevel"></param>
        public void New(int aLevel)
        {
            Random rnd = new Random(System.Environment.TickCount);

            int id = rnd.Next(1000000000);
            New(id.ToString(), aLevel);
        }
        
        /// <summary>
        /// recommencer la nouvelle partie
        /// </summary>
        public void ReNew()
        {
            SetTableau(_Origines);
            if (OnGameBegin != null)
                OnGameBegin(this, EventArgs.Empty);
        }

        public int Rate
        {
            get { return _Rate; }
        }

        public string IDPartie
        {
            get { return _IDPartie; }
        }

        public bool IsWin()
        {
            if (_NumberRemaining > 0)
                return false;
            for (int x = 0; x < 9; x++)
            {
                int sommeY = 0;
                int sommeX = 0;
                for (int y = 0; y < 9; y++)
                {
                    sommeY += _Cases[x, y];
                    sommeX += _Cases[y, x];
                }
                if (sommeY != 45)
                    return false;
                if (sommeX != 45)
                    return false;
            }
            return true;
        }

        public bool LineOrColIsComplete(int x, int y, out bool lineIsComplete, out bool colIsComplete)
        {
            int sommeY = 0;
            int sommeX = 0;
            for (int i = 0; i < 9; i++)
            {
                sommeY += _Cases[i, y];
                sommeX += _Cases[x, i];
            }
            lineIsComplete = sommeY == 45;
            colIsComplete = sommeX == 45;
            return (sommeY == 45) || (sommeX == 45);
        }
        #region sauvegarde et restauration de parties

        private XElement CreatePartie(string ID, params string[] Attrs)
        {
            XElement xml = new XElement("Partie",
                                new XAttribute("ID", ID),
                                new XAttribute("Date", DateTime.Now.ToString()),
                                new XAttribute("Fixed", FixedNumbers()),
                                new XAttribute("User", UserNumbers()),
                                new XAttribute("Rate", _Rate.ToString()),
                                new XAttribute("Notes", Notes()),
                                new XAttribute("Flags", Flags()));
            if (Attrs != null)
            {
                for (int i = 0; i < Attrs.Length; i += 2)
                    xml.Add(new XAttribute(Attrs[i], Attrs[i + 1]));
            }
            return xml;
        }

        private string FromTabNotes(ref bool[, , ,] aTab)
        {
            StringBuilder sb = new StringBuilder(81);
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    for (int ymin = 0; ymin < 9; ymin++)
                    {
                        for (int xmin = 0; xmin < 9; xmin++)
                        {
                            if (aTab[x, y, xmin, ymin])
                                sb.Append("1");
                            else
                                sb.Append("0");
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private void ToTabsNotes(ref bool[, , ,] aTab, string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 6561)
                throw new ArgumentException("value est incorrect !", "value");
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    for (int ymin = 0; ymin < 9; ymin++)
                    {
                        for (int xmin = 0; xmin < 9; xmin++)
                        {
                            int offset = y * (9*9*9) + x *9*9 + ymin *9 + xmin;
                            char c = value[offset];
                            bool b = Convert.ToBoolean(int.Parse(new string(c, 1)));

                            aTab[x, y, xmin, ymin] = b;
                        }
                    }
                }
            }
        }

        private string Notes()
        {
            return CompressString(FromTabNotes(ref _CheckedCell));
        }

        private string Flags()
        {
            return CompressString(FromTabNotes(ref _FlagCell));
        }

        public void Save(string aFileName, string ID, params string[] Attrs)
        {
            XDocument document = null;
            try
            {
                document = Load(aFileName);
            }
            catch
            {
            }
            if (document == null)
            {
                document = new XDocument();
            }


            var qry = from partie in document.Descendants("Partie")
                      where partie.Attribute("ID").Value == ID
                      select partie;

            //XmlWriter xw = XmlWriter.Create();

            if (qry != null)
            {
                XElement elem = qry.FirstOrDefault();
                if (elem != null)
                    elem.Remove();
            }
            if (document.Element("Parties") == null)
            {
                document.Add(new XElement("Parties"));
            }

            document.Element("Parties").Add(CreatePartie(ID, Attrs));
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream stm = isf.CreateFile(aFileName);
                using (stm)
                {
                    using (XmlWriter xw = XmlWriter.Create(stm))
                    {
                        document.Save(xw);
                    }
                }
            }

        }

        private XDocument Load(string aFileName)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream stm;
                if (isf.FileExists(aFileName))
                {
                    using (stm = isf.OpenFile(aFileName, FileMode.Open))
                    {
                        using (XmlReader xr = XmlReader.Create(stm))
                        {
                            return XDocument.Load(xr);
                        }
                    }
                }
            }
            return null;
        }

        private string UserNumbers()
        {
            return Numbers(false);
        }

        private string FixedNumbers()
        {
            return Numbers(true);
        }

        private string Numbers(bool fixednumber)
        {
            StringBuilder sb = new StringBuilder(81);
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    bool isfixed = fixednumber ^ IsFixedNumber(x, y);
                    if (!isfixed)
                        sb.Append(_Cases[x, y]);
                    else
                        sb.Append("0");
                }
            }
            return sb.ToString();
        }

        public void Restore(string aFileName, string ID)
        {
            XDocument doc = Load(aFileName);
            if (doc != null)
            {
                Restore(doc, ID);
            }
        }

        public void Restore(XDocument aDoc, string ID)
        {
            var qry = from partie in aDoc.Descendants("Partie")
                      where partie.Attribute("ID").Value == ID
                      select partie;
            XElement n = qry.FirstOrDefault();
            if (n != null)
            {
                _IDPartie = n.Attribute("ID").Value;
                SetTableau(n.Attribute("Fixed").Value, n.Attribute("User").Value);
                if (n.Attribute("Notes") != null)
                {
                    RestoreNotes(n.Attribute("Notes").Value);
                }
                if (n.Attribute("Rate") != null)
                {
                    _Rate = int.Parse(n.Attribute("Rate").Value);
                }
            }
        }

        private void RestoreFlags(string value)
        {
            ToTabsNotes(ref _FlagCell, UnCompressString(value));
        }

        private void RestoreNotes(string value)
        {
            ToTabsNotes(ref _CheckedCell, UnCompressString(value));
        }

        private string CompressString(string s)
        {
            //problème je n'ai pas GZipStream sur Mobile
            return s;
        }

        private string UnCompressString(string s)
        {
            //problème je n'ai pas GZipStream sur Mobile
            return s;
        }

        #endregion

        public bool IsError(int x, int y)
        {
            if (_Solution != null)
            {
                return (_Solution[x, y] != _Cases[x, y]);
            }
            return false;
        }

        public int Block(int x, int y)
        {
            return m_subSquare[y, x];            
        }

        public bool IsCheckedCell(int X, int Y, int xCell, int yCell)
        {
            return _CheckedCell[X, Y, xCell, yCell];
        }

        public bool IsFlagged(int X, int Y, int xCell, int yCell)
        {
            return _FlagCell[X, Y, xCell, yCell];            
        }

        public void CheckCell(int X, int Y, int xCell, int yCell, bool value)
        {
            _CheckedCell[X, Y, xCell, yCell] = value;
            DoCheckChanged();
        }

        public void FlagCell(int X, int Y, int xCell, int yCell, bool value)
        {
            _FlagCell[X,Y , xCell, yCell] = value;
            DoFlagChanged();
        }

        public void ClearNotes()
        {
            _CheckedCell = new bool[9, 9, 9, 9];
            _FlagCell = new bool[9, 9, 9, 9];
            DoFlagChanged();
            DoCheckChanged();
        }

    }
}
