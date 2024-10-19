using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simplex_method
{
    public class SMX_MD
    {
        private double[] c; // массив содержащий коэфициенты при переменных в функции F.
        private double[,] A; // матрица содержащая коэфициенты при переменны в неравенствах.
        private double[] b; // массив содержащий числа, которые стоят в правых частях неравенств.
        private double[,] table; // таблица которую мы будем преобразовывать.
        private string[] FreeX; // самая верхняя строчка в таблице, которая содержит свободные иксы.
        private string[] DependX; // самый левый столбец содержащий зависимые иксы.
        private string minormax; // поле содержащее строку min или max, в зависимости от того, какое значение мы будем искать у функции F.

        public SMX_MD(double[] c, double[,] A, double[] b, string minormax)
        {
            if (A.GetLength(0) != b.Length || A.GetLength(1) != c.Length) // проверка чтобы размеры массивов c и b, и матрицы A соответствовали друг другу.
            {
                throw new Exception("Неверное соотношение размеров матриц!");
            }

            if (minormax == "min") this.c = c;
            else
            {
                double[] c_temp = new double[c.Length]; // создается временный массив c_temp, который будет заполнен коэфициентами из масива c, знак коэфициентов будет зависить от того ищем мы max или min функции F.
                for (int i = 0; i < c.Length; i++) { c_temp[i] = -1 * c[i]; }
                this.c = c_temp;
            }
            this.A = A;
            this.b = b;
            this.minormax = minormax;
            FillTable();

            FreeX = new string[c.Length];
            int k = 0; // заполнение свободных иксов. Счетчик k понадобится для того, чтобы знать сколько было зависимых иксов. Это нужно чтобы правильно именовать зависимые иксы.
            for (int i = 0; i < c.Length; i++)
            {
                FreeX[i] = $"X{i + 1}";
                k++;
            }

            DependX = new string[b.Length]; // заполнение зависимых иксов
            for (int i = 0; i < b.Length; i++)
            {
                k++;
                DependX[i] = $"X{k}";
            }
        }

        public bool Solution()
        {
            Console.WriteLine("Начальная задача: ");
            Console.WriteLine();
            Print();
            Console.WriteLine();
            Console.WriteLine();

            if (FindOptSolve()) { if (minormax == "max") { table[0, b.Length] = -1 * table[0, b.Length]; Print(); Console.WriteLine(); Console.WriteLine(); } Check(); return true; }
            return false;
        }

        public bool FindOptSolve()
        {
            if (FindOprSolve())
            {
                int l = 0; // счетчик отрицательных элементов в строке F
                for (int t = 1; t < c.Length + 1; t++) if (table[t, b.Length] < 0) { l++; }
                if (l == c.Length) { return true; } // если все элементы в строке F отрицательные и найдено опорное решение, то мы нашли оптимальное решение

                for (int i = 1; i < c.Length + 1; i++)
                {
                    if (table[i, b.Length] > 0)
                    {
                        int razr_stolb = i;

                        double min = double.MaxValue; // поиск разрешающей строки. Переменная min будет содержать минимальное положительное число. Для начального значение возьмем очень большое положительное число.

                        int flag2 = 0; // флаг показывает, меняли ли мы переменную min, если flag2 > 0, значит меняли.
                        int razr_str = -1;

                        for (int z = 0; z < b.Length; z++)
                        {
                            double k = table[0, z] / table[razr_stolb, z];
                            if (k < min && k > 0)
                            {
                                min = k;
                                razr_str = z;
                                flag2++;
                            }
                        }

                        if (flag2 == 0) { continue; }
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
            table1[0, b.Length] = 0;

            for (int i = 0; i < b.Length; i++)
            {
                for (int j = 1; j < c.Length + 1; j++)
                {
                    table1[j, i] = A[i, j - 1];
                }
            }

            for (int i = 1; i < c.Length + 1; i++) { table1[i, b.Length] = -c[i - 1]; } // на этом моменте заполнили всю начальную таблицу
            table = table1;
        }

        public bool FindOprSolve() // поиск опорного решения
        {
            int l = 0; // счетчик положительных элементов в столбце свободных членов
            for (int t = 0; t < b.Length; t++) if (table[0, t] >= 0) { l++; }
            if (l == b.Length) { return true; }

            for (int i = 0; i < b.Length; i++)
            {
                if (table[0, i] < 0)
                {
                    for (int j = 1; j < c.Length + 1; j++)
                    {
                        if (table[j, i] < 0)
                        {
                            int razr_stolb = j;

                            double min = double.MaxValue; // поиск разрешающей строки. Переменная min будет содержать минимальное положительное число. Для начального значение возьмем очень большое положительное число.

                            int flag1 = 0; // флаг показывает, меняли ли мы переменную min, если flag1 > 0, значит меняли.
                            int razr_str = -1;

                            for (int z = 0; z < b.Length; z++)
                            {
                                double k = table[0, z] / table[razr_stolb, z];
                                if (k < min && k > 0)
                                {
                                    min = k;
                                    razr_str = z;
                                    flag1++;
                                }
                            }

                            if (flag1 == 0) { continue; }

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

            string _x = FreeX[razr_stolb - 1];
            FreeX[razr_stolb - 1] = DependX[razr_str];
            DependX[razr_str] = _x;

            table1[razr_stolb, razr_str] = 1 / r_e;

            for (int i = 0; i < c.Length + 1; i++)
            {
                if (i != razr_stolb) table1[i, razr_str] = table[i, razr_str] / r_e;
            }

            for (int i = 0; i < b.Length + 1; i++)
            {
                if (i != razr_str) table1[razr_stolb, i] = -table[razr_stolb, i] / r_e;
            }

            for (int i = 0; i < b.Length + 1; i++)
            {
                for (int j = 0; j < c.Length + 1; j++)
                {
                    if (i != razr_str && j != razr_stolb) table1[j, i] = table[j, i] - (table[razr_stolb, i] * table[j, razr_str]) / r_e;
                }
            }

            table = table1;

            Print();
            Console.WriteLine();
            Console.WriteLine();
        }

        public void Print() // выводит таблицу
        {
            Console.Write("\t");
            Console.Write(" S" + "\t");
            foreach (object obj in FreeX) { Console.Write(" " + obj + "\t"); }
            Console.WriteLine();
            Console.WriteLine();

            for (int i = 0; i < b.Length + 1; i++)
            {
                if (i != b.Length) Console.Write(DependX[i] + "\t"); else Console.Write("F" + "\t");

                for (int j = 0; j < c.Length + 1; j++)
                {
                    if (table[j, i] >= 0) Console.Write(" " + Math.Round(table[j, i], 2) + "\t"); // этот if нужен для выравнивания столбцов.
                    else Console.Write(Math.Round(table[j, i], 2) + "\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine("______________________________________________");
        }

        public void Check() // проверка найденного решение методом подстановки в начальные условия
        {
            Console.WriteLine("Проверка решения: ");
            Console.WriteLine();
            Console.WriteLine("Функция: ");
            double[] solve_x = new double[table.GetLength(0) + table.GetLength(1) - 2]; // создаем массив иксов, которые решают поставленную задачу, берем их из последней таблицы.
            string[] str_X = new string[solve_x.Length]; // вспомогательный массив для определения последовательности иксов в массиве solve_x, взятых из последней таблицы table;
            for (int i = 0; i < solve_x.Length; i++) str_X[i] = $"X{i + 1}";

            for (int i = 0; i < DependX.Length; i++)
            {
                int k = Array.IndexOf(str_X, DependX[i]); // переменная для определения номера икса в массиве solve_x
                solve_x[k] = table[0, i];
            }

            for (int i = 0; i < solve_x.Length / 2; i++)
            {
                if (minormax == "min" && i != solve_x.Length / 2 - 1) Console.Write($"{c[i]}*{solve_x[i]} + ");
                if (minormax == "min" && i == solve_x.Length / 2 - 1) Console.Write($"{c[i]}*{solve_x[i]} = {table[0, table.GetLength(1) - 1]}");
                if (minormax == "max" && i != solve_x.Length / 2 - 1) Console.Write($"{-c[i]}*{solve_x[i]} + ");
                if (minormax == "max" && i == solve_x.Length / 2 - 1) Console.Write($"{-c[i]}*{solve_x[i]} = {table[0, table.GetLength(1) - 1]}");
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Неравенства: ");

            for (int i = 0; i < b.Length; i++)
            {
                for (int j = 0; j < solve_x.Length / 2; j++)
                {
                    if (j != solve_x.Length / 2 - 1) Console.Write($"{A[i, j]}*{solve_x[j]} + ");
                    if (j == solve_x.Length / 2 - 1) Console.Write($"{A[i, j]}*{solve_x[j]} <= {b[i]}");
                }
                Console.WriteLine();
            }
            Console.WriteLine("______________________________________________");
            Console.WriteLine();
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            SMX_MD a = new SMX_MD(new double[] { 7, 8, 3 }, new double[,] { { 3, 1, 1 }, { 1, 4, 0 }, { 0, 0.5, 2 } }, new double[] { 4, 7, 8 }, "max"); // условие задачи принимаются в каноническом виде
            if(!a.Solution()) { Console.WriteLine("Решение не найдено"); };
            Console.WriteLine();
        }
    }
} 
// последние 2 таблицы отличаются только значение функции
