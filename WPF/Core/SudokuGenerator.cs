using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace GeniusSudoku.Core
{
    class SudokuGenerator
    {
        private Random FRnd;
        private int zr = 362436069;
        private int wr = 521288629;

        int b, w, f, s1, m0, c1, c2, r1, l, i1, m1, m2, a, i, j, k, r, c, d, n = 729, m = 324, x, y, s;
        int mi1, mi2, q7, part, nt, nodes, solutions, min, sam1, clues;
        int[] Rows = new int[325], Cols = new int[730], Ur = new int[730], Uc = new int[325], V = new int[325], W = new int[325];
        int[][] Col = new int[730][], Row = new int[325][];

        int[] P = new int[88], A = new int[88], C = new int[88], I = new int[88], Two = new int[888];
        char[] B = "0111222333111222333111222333444555666444555666444555666777888999777888999777888999".ToCharArray();
        char[][] H = new char[326][];
        char[] L = ".123456789".ToCharArray();

        private int MWC()
        {
            return ((zr = 36969 * (zr & 65535) + (zr >> 16)) ^ (wr = 18000 * (wr & 65535) + (wr >> 16)));
        }

        private void ReinitI()
        {
            for (int iIndex = 0; iIndex < 88; iIndex++)
                I[iIndex] = 0;
        }

        public SudokuGenerator()
        {
            for (i = 0; i < Row.Length; i++)
                Row[i] = new int[10];
            for (i = 0; i < Col.Length; i++)
                Col[i] = new int[5];
            for (i = 0; i < H.Length; i++)
                H[i] = new char[7];
            FRnd = new Random(System.Environment.TickCount);
        }

        public SudokuData[] Generate(int seed, int samples, int rate, int aLevel)
        {
            List<SudokuData> Result = new List<SudokuData>();
            zr ^= seed;
            wr ^= seed;
            if (samples <= 0)
                return new SudokuData[0];

            if (rate < 0)
                rate = 0;

            for (i = 0; i < 888; i++)
            {
                j = 1;
                while (j <= i)
                    j += j;
                Two[i] = j - 1;
            }
            r = 0;
            for (x = 1; x <= 9; x++)
                for (y = 1; y <= 9; y++)
                    for (s = 1; s <= 9; s++)
                    {
                        r++;
                        Cols[r] = 4;
                        Col[r][1] = x * 9 - 9 + y;
                        Col[r][2] = (B[x * 9 - 9 + y] - 48) * 9 - 9 + s + 81;
                        Col[r][3] = x * 9 - 9 + s + 81 * 2;
                        Col[r][4] = y * 9 - 9 + s + 81 * 3;
                    }
            for (c = 1; c <= m; c++)
                Rows[c] = 0;
            for (r = 1; r <= n; r++)
                for (c = 1; c <= Cols[r]; c++)
                {
                    a = Col[r][c];
                    Rows[a]++;
                    Row[a][Rows[a]] = r;
                }
            c = 0;
            for (x = 1; x <= 9; x++)
                for (y = 1; y <= 9; y++)
                {
                    c++;
                    H[c][0] = 'r';
                    H[c][1] = (char)(x + 48);
                    H[c][2] = 'c';
                    H[c][3] = (char)(y + 48);
                    H[c][4] = (char)0;
                }
            c = 81;
            for (b = 1; b <= 9; b++)
                for (s = 1; s <= 9; s++)
                {
                    c++;
                    H[c][0] = 'b';
                    H[c][1] = (char)(b + 48);
                    H[c][2] = 's';
                    H[c][3] = (char)(s + 48);
                    H[c][4] = (char)0;
                }
            c = 81 * 2;
            for (x = 1; x <= 9; x++)
                for (s = 1; s <= 9; s++)
                {
                    c++;
                    H[c][0] = 'r';
                    H[c][1] = (char)(x + 48);
                    H[c][2] = 's';
                    H[c][3] = (char)(s + 48);
                    H[c][4] = (char)0;
                }
            c = 81 * 3;
            for (y = 1; y <= 9; y++)
                for (s = 1; s <= 9; s++)
                {
                    c++;
                    H[c][0] = 'c';
                    H[c][1] = (char)(y + 48);
                    H[c][2] = 's';
                    H[c][3] = (char)(s + 48);
                    H[c][4] = (char)0;
                }
            sam1 = 0;
        m0s:
            sam1++;
            if (sam1 > samples)
                return Result.ToArray();
        m0:
            for (i = 1; i <= 81; i++)
                A[i] = 0;
            part = 0;
            q7 = 0;
        mr1:
            i1 = (MWC() >> 8) & 127;
            if (i1 > 80)
                goto mr1;
            i1++;
            if (A[i1] > 0)
                goto mr1;
        mr3:
            s = (MWC() >> 9) & 15;
            if (s > 8)
                goto mr3;
            s++;
            A[i1] = s;
            m2 = Solve();
            q7++;
            if (m2 < 1)
                A[i1] = 0;
            if (m2 != 1)
                goto mr1;
            //récupérer la solution pour afficher les erreurs
            //now we have a unique-solution sudoku. Now remove clues to make it minimal
            part++;
            if (Solve() != 1)
                goto m0;
            SudokuData data = new SudokuData();
            for (i = 1; i <= 81; i++)
            {
            mr4:
                x = (MWC() >> 8) & 127;
                if (x >= i)
                    goto mr4;
                x++;
                P[i] = P[x];
                P[x] = i;
            }
            for (i1 = 1; i1 <= 81; i1++)
            {
                s1 = A[P[i1]];
                A[P[i1]] = 0;
                if (Solve() > 1)
                    A[P[i1]] = s1;
            }
            //Begin PGO, ajout de valeur pour rendre plus facile la résolution
            AjouterNombre(aLevel);
            //End PGO

            nt = 0;
            if (rate > 0)
            {
                nt = 0;
                mi1 = 9999;
                for (f = 0; f < 100; f++)
                {
                    Solve();
                    nt += nodes;
                    if (nodes < mi1)
                    {
                        mi1 = nodes;
                        mi2 = C[clues];
                    }
                }
                printf("rating:{0} ,  ", nt);
                if (rate > 1)
                    printf("hint:{0}    ", new string(H[mi2]));
            }
            data.Rate = nt;
            data.Datas = new byte[9, 9];
            Result.Add(data);
            data.Datas = Tableau(A);
            for (i = 1; i <= 81; i++)
                printf("{0}", L[A[i]]);
            printf("\n");
            goto m0s;

        }

        private byte[,] Tableau(int[] aTab)
        {
            byte[,] Result = new byte[9, 9];
            for (i = 1; i <= 81; i++)
            {
                int index = i - 1;
                Result[index % 9, index / 9] = (byte)aTab[i];
            }
            return Result;
        }

        private void AjouterNombre(int aLevel)
        {
            if (aLevel >= 100)
                return;
            for (int iBlock = 0; iBlock < 9; iBlock++)
            {
                int nbValue = 0;
                for (int index = iBlock * 9; index < (iBlock + 1) * 9; index++)
                {
                    if (A[index + 1] > 0)
                        nbValue++;
                }
                //ajout de nombre de valeur qu'il faut en fonction du level
                AjouterNombre(iBlock, 4 - nbValue);
            }
        }

        private void AjouterNombre(int iBlock, int nbValue)
        {
            if (nbValue < 0)
                return;
            for (int iAjout = 0; iAjout < nbValue; iAjout++)
            {
                while (true)
                {
                    i1 = Random(1, 10);
                    i1 = iBlock * 9 + i1;
                    if (A[i1] <= 0)
                    {
                        while (true)
                        {
                            int pseudovalue = Random(1, 10);
                            A[i1] = pseudovalue;
                            ReinitI();
                            if (Solve() == 1)
                                break;
                            A[i1] = 0;
                        }
                        break;
                    }
                }
            }
        }

        private int Random(int min, int max)
        {
            return FRnd.Next(min, max);
        }

        [Conditional("DEBUG")]
        private void printf(string sFormat, params object[] args)
        {
            Debug.Write(string.Format(sFormat, args));
        }

        #region Resolution du sudoku
        private int Solve()
        {
            for (i = 0; i <= n; i++) Ur[i] = 0;
            for (i = 0; i <= m; i++) Uc[i] = 0;
            clues = 0;
            for (i = 1; i <= 81; i++)
                if (A[i] > 0)
                {
                    clues++; r = i * 9 - 9 + A[i];
                    for (j = 1; j <= Cols[r]; j++)
                    {
                        d = Col[r][j]; if (Uc[d] > 0) return 0; Uc[d]++;
                        for (k = 1; k <= Rows[d]; k++) { Ur[Row[d][k]]++; }
                    }
                }
            for (c = 1; c <= m; c++)
            {
                V[c] = 0;
                for (r = 1; r <= Rows[c]; r++)
                    if (Ur[Row[c][r]] == 0) V[c]++;
            }

            i = clues; m0 = 0; m1 = 0; solutions = 0; nodes = 0;
        m2: i++;
            I[i] = 0;
            min = n + 1;
            if (i > 81 || m0 > 0)
                goto m4;
            if (m1 > 0)
            {
                C[i] = m1;
                goto m3;
            }
            w = 0;
            for (c = 1; c <= m; c++)
                if (Uc[c] == 0)
                {
                    if (V[c] < 2)
                    {
                        C[i] = c;
                        goto m3;
                    }
                    if (V[c] <= min)
                    {
                        w++;
                        W[w] = c;
                    };
                    if (V[c] < min)
                    {
                        w = 1;
                        W[w] = c;
                        min = V[c];
                    }
                }
        mr: c2 = MWC() & Two[w];
            if (c2 >= w)
                goto mr;
            C[i] = W[c2 + 1];
        m3: c = C[i];
            I[i]++;
            if (I[i] > Rows[c])
                goto m4;
            r = Row[c][I[i]];
            if (Ur[r] > 0)
                goto m3;
            m0 = 0;
            m1 = 0;
            nodes++;//if(nodes>9999 && part==0)return 0;
            for (j = 1; j <= Cols[r]; j++)
            {
                c1 = Col[r][j];
                Uc[c1]++;
            }
            for (j = 1; j <= Cols[r]; j++)
            {
                c1 = Col[r][j];
                for (k = 1; k <= Rows[c1]; k++)
                {
                    r1 = Row[c1][k]; Ur[r1]++; if (Ur[r1] == 1)
                        for (l = 1; l <= Cols[r1]; l++)
                        {
                            c2 = Col[r1][l]; V[c2]--;
                            if (Uc[c2] + V[c2] < 1) m0 = c2; if (Uc[c2] == 0 && V[c2] < 2) m1 = c2;
                        }
                }
            }
            if (i == 81) solutions++; if (solutions > 1) goto m9; goto m2;
        m4: i--; c = C[i]; r = Row[c][I[i]]; if (i == clues) goto m9;
            for (j = 1; j <= Cols[r]; j++)
            {
                c1 = Col[r][j]; Uc[c1]--;
                for (k = 1; k <= Rows[c1]; k++)
                {
                    r1 = Row[c1][k]; Ur[r1]--;
                    if (Ur[r1] == 0) for (l = 1; l <= Cols[r1]; l++) { c2 = Col[r1][l]; V[c2]++; }
                }
            }
            if (i > clues) goto m3;
        m9:
            return solutions;
        }
        #endregion
    }
}
