using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simplex_method
{
    public class SMX_MD
    {
        private double[] c;
        private double[,] A;
        private double[] b;
        private double[,] table; // таблица которую мы будем преобразовывать
        private string[] FreeX; // самая верхняя строчка в таблице, которая содержит свободные иксы
        private string[] DependX; // самый левый столбец содержащий зависимые иксы

        public SMX_MD(double[] c, double[,] A, double[] b)
        {
            if(A.GetLength(0) != b.Length || A.GetLength(1) != c.Length)
            {
                throw new Exception("Неверное соотношение размеров матриц!");
            }

            this.c = c;
            this.A = A;
            this.b = b;
            FillTable();

            FreeX = new string[c.Length];
            int k = 0; // заполнение свободных иксов
            for(int i = 0; i < c.Length; i++)
            {
                FreeX[i] = $"X{i+1}";
                k++;
            }

            DependX = new string[b.Length]; // заполнение зависимых иксов
            for(int i = 0;i < b.Length; i++)
            {
                k++;
                DependX[i] = $"X{k}";
            }
        }

        public bool FindOptSolve()
        {
            if(FindOprSolve())
            {
                int l = 0; // счетчик отрицательных элементов в строке F
                for (int t = 1; t < c.Length + 1; t++) if (table[t, b.Length] < 0) { l++; }
                if (l == c.Length) { return true; } // если все элементы в строке F отрицательные и найдено опорное решение, то мы нашли оптимальное решение

                for (int i = 1; i < c.Length + 1; i++)
                {
                    if(table[i, b.Length] > 0)
                    {
                        int razr_stolb = i;

                        double min = 100000; // поиск разрешающей строки

                        int flag2 = 0;
                        int razr_str = -1;
                            
                        for(int z = 0; z < b.Length; z++)
                        {
                            double k = table[0, z] / table[razr_stolb, z];
                            if (k < min && k > 0)
                            {
                                min = k;
                                razr_str = z;
                                flag2++;
                            }
                        }

                        if(flag2 == 0) { continue; }
                        fix_table(razr_str, razr_stolb);
                        return FindOptSolve();
                    }
                }
            }

            return false;
        }

        public void FillTable() // начальное заполнение таблицы, используется только в конструкторе
        {
            double[,] table1 = new double[c.Length + 1, b.Length + 1];

            for (int i = 0; i < b.Length; i++) { table1[0, i] = b[i]; } // заполнение первой колонки начальной таблицы
            table1[0,b.Length] = 0;

            for (int i = 0; i < b.Length; i++) 
            {
                for (int j = 1; j < c.Length + 1; j++)
                {
                    table1[j, i] = A[i, j - 1];
                }
            }

            for (int i = 1; i < c.Length + 1; i++) { table1[i,b.Length] = -c[i - 1]; } // на этом моменте заполнили всю начальную таблицу
            table = table1;
        }

        public bool FindOprSolve() // поиск опорного решения
        {
            int l = 0; // счетчик положительных элементов в столбце свободных членов
            for (int t = 0; t < b.Length; t++) if (table[0, t] >= 0) { l++; }
            if (l == b.Length) { return true; }

            for (int i = 0; i < b.Length + 1; i++)
            {
                if (table[0, i] < 0)
                {
                    for (int j = 1; j < c.Length + 1; j++)
                    {
                        if (table[j, i] < 0)
                        {
                            int razr_stolb = j;

                            double min = 100000; // поиск разрешающей строки

                            int flag1 = 0;
                            int razr_str = -1;
                            
                            for(int z = 0; z < b.Length; z++)
                            {
                                double k = table[0, z] / table[razr_stolb, z];
                                if (k < min && k > 0)
                                {
                                    min = k;
                                    razr_str = z;
                                    flag1++;
                                }
                            }

                            if(flag1 == 0) { continue; }

                            fix_table(razr_str, razr_stolb);

                            return FindOprSolve();
                        }
                    }
                }
            }

            return false;
        }

        public void fix_table(int razr_str, int razr_stolb) // перестройка таблицы при заданном разрешающем элементе
        {
            double[,] table1 = new double[c.Length + 1, b.Length + 1];

            double r_e = table[razr_stolb, razr_str]; // разрешающий элемент

            string _x = FreeX[razr_stolb-1];
            FreeX[razr_stolb-1] = DependX[razr_str];
            DependX[razr_str] = _x;

            table1[razr_stolb, razr_str] = 1/r_e;

            for(int i = 0; i < c.Length+1; i++) {
                if(i != razr_stolb) table1[i, razr_str] = table[i, razr_str]/r_e;
            }

            for (int i = 0; i < b.Length + 1; i++)
            {
                if (i != razr_str) table1[razr_stolb, i] = -table[razr_stolb, i] / r_e;
            }

            for(int i = 0;i < b.Length+1; i++)
            {
                for(int j = 0;  j < c.Length + 1; j++)
                {
                    if (i != razr_str && j != razr_stolb) table1[j, i] = table[j, i] - (table[razr_stolb,i] * table[j, razr_str]) / r_e;
                }
            }

            table = table1;
        }

        public void Print() // выводит таблицу
        {
            for(int i = 0; i < b.Length+1; i++)
            {
                if (i != b.Length) Console.Write(DependX[i] + "\t"); else Console.Write("F" + "\t");
                
                for(int j = 0;j < c.Length + 1;j++) Console.Write(Math.Round(table[j, i],2) + "\t"); 
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.Write("\t");
            Console.Write("S" + "\t");
            foreach (object obj in FreeX) { Console.Write(obj + "\t"); }
        }   
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            SMX_MD a = new SMX_MD(new double[] { -7,-8,-3 }, new double[,] { { 3,1,1 }, { 1,4,0.5 }, { 1,0,2} }, new double[] { 4,7,8}); // условие задачи принимаются в каноническом виде
            a.FindOptSolve();
            a.Print();
            Console.WriteLine();
        }
    }
}
