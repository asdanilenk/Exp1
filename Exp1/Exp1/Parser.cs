using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exp1
{
    public class Parser
    {
        private const string WrongBracketsOrder = "Wrong brackets order: ";
        private const string UnknownFunction = "Unknown function or variable: ";
        private const string DivisionByZero = "Division by zero";
        private const string LogarithmDomain = "Argument outside the domain of logarithm";
        private const string ArccosineDomain = "Argument outside the domain of arccosine";
        private const string ArcsineDomain = "Argument outside the domain of arcsine";
        private const string RootDomain = "Argument outside the domain of square root";
        private const string EmptyArgument = "Empty argument";
        private const string Ar = "+-*/^";

        Node _root;
        private const double EPS = 0.0001;
        private const double PI = Math.PI;
        private const double E = Math.E;

        private bool Search(string str, string s1, ref Node root, List<string> variables)
        {
            int i = str.Length - 1;
            int c = 0;
            string s = s1;

            while (((i >= 0) && (!((c == 0) && (i != str.Length - 1) && ((s.LastIndexOf(str[i])) != -1)))))
            {
                if (str[i] == '(')
                    c++;
                if (str[i] == ')')
                    c--;
                i--;
            }

            if ((i != -1) && (Ar.LastIndexOf(str[i - 1]) == -1) && (Ar.LastIndexOf(str[i + 1]) == -1))
            {
                string str1 = str.Substring(0, i);
                string str2 = str.Substring(i, 1);
                string str3 = str.Substring(i + 1);
                root = new Node(str2);
                Parse(str1, ref root.Left, variables);
                Parse(str3, ref root.Right, variables);
                return true;
            }
            return false;
        }
        private bool SearchF(string str, string s1, ref Node root, List<string> variables)
        {
            string s = s1;
            if (str.StartsWith(s) && str.EndsWith(")"))
            {
                s = s.Substring(0, s.Length - 1);
                string str1 = str.Substring(s.Length + 1, str.Length - s.Length - 2);
                root = new Node(s);
                Parse(str1, ref root.Left, variables);
                return true;
            }
            return false;
        }

        private bool SearchF2(string str, string s1, ref Node root, List<string> variables)
        {
            string s = s1;
            if (str.StartsWith(s) && str.EndsWith(")"))
            {
                str = str.Substring(s.Length, str.Length - s.Length - 1);
                int c = 0, i = str.Length - 1;
                while (((i >= 0) && (!((c == 0) && (i != str.Length - 1) && (str[i]== ';')))))
                {
                    if (str[i] == '(')
                        c++;
                    if (str[i] == ')')
                        c--;
                    i--;
                }

                if ((i != -1))
                {
                    string str1 = str.Substring(0, i);
                    string str3 = str.Substring(i + 1);
                    root = new Node(s.Substring(0,s.Length-1));
                    Parse(str1, ref root.Left, variables);
                    Parse(str3, ref root.Right, variables);
                    return true;
                }
                return false;
            }
            return false;
        }
        public bool Parse(string str, List<string> variables)
        {
            return Parse(str, ref _root, variables);
        }
        private bool Parse(string str, ref Node root, List<string> variables)
        {
            str = str.Trim();
            int i;
            int c = 0;
            // Проверяет на пустую строку
            if (str.Equals(""))
            {
                throw new Exception(EmptyArgument);
            }
            // Проверяет что кол-во открытых скобок равно кол-ву закрытых

            for (i = 0; i < str.Length; i++)
            {
                switch (str[i])
                {
                    case ')':
                        c--;
                        break;
                    case '(':
                        c++;
                        break;
                }
                if (c < 0) break;
            }
            if (c != 0)
            {
                throw new Exception(WrongBracketsOrder);
            }
            // Переводит строку типа (***) в *** и запускает парсер от него
            i = 1;
            if (str[0] == '(')
            {
                while ((i < str.Length) && !((str[i] == ')') && (c == 0)))
                {
                    if ((str[i] == ')') && (i != str.Length - 1) && (c == 0)) break;
                    if (str[i] == ')') c--;
                    if (str[i] == '(') c++;
                    i++;
                }
                if (i == str.Length - 1)
                {
                    str = str.Substring(1, str.Length - 2);
                    return Parse(str, ref root, variables);
                }
            }
            //Проверяет не явлеятся ли строка числом

            bool catched = false;
            try { Double.Parse(str); }
            catch { catched = true; }
            if (!catched)
            {
                root = new Node(str);
                return true;
            }
            //Проверяет не является ли строка Pi или е
            if (str.ToLower().Equals("pi") || str.ToLower().Equals("e"))
            {
                root = new Node(str.ToLower());
                return true;
            }
            //Проверяет не является ли строка переменной
            if (variables.Any(variable => str.Equals(variable)))
            {
                root = new Node(str);
                return true;
            }
            //Проверяет на унарный минус

            if (str[0] == '-')
            {
                root = new Node("-");
                str = str.Substring(1);
                Parse(str, ref root.Right, variables);
                return true;
            }

            if (Search(str, "+-", ref root, variables) || Search(str, "*/", ref root, variables) ||
            Search(str, "^", ref root, variables)      || SearchF(str, "sin(", ref root, variables) ||
            SearchF(str, "cos(", ref root, variables)  || SearchF(str, "exp(", ref root, variables) ||
            SearchF(str, "ln(", ref root, variables)   || SearchF(str, "log(", ref root, variables) ||
            SearchF(str, "tan(", ref root, variables)  || SearchF(str, "ctan(", ref root, variables) ||
            SearchF(str, "acos(", ref root, variables) || SearchF(str, "asin(", ref root, variables) ||
            SearchF(str, "atan(", ref root, variables) || SearchF(str, "actan(", ref root, variables) ||
            SearchF(str, "cosh(", ref root, variables) || SearchF(str, "sinh(", ref root, variables) ||
            SearchF(str, "tanh(", ref root, variables) || SearchF(str, "ctanh(", ref root, variables) ||
            SearchF2(str, "min(", ref root, variables) || SearchF2(str, "max(", ref root, variables) ||
            SearchF(str, "sqrt(", ref root, variables) || SearchF(str, "abs(", ref root, variables)) return true;

            throw new Exception(UnknownFunction + str);
        }

        private static bool Check(double a, string msg)
        {
            if (a < EPS)
            {
                throw new Exception(msg);
            }
            return true;
        }
        public double Calculate(Dictionary<string, Double> values)
        {
            return Calculate(values, ref _root);
        }

        private static double Calculate(Dictionary<string, Double> values, ref Node root)
        {
            if (root.Key.Equals("+")) return (Calculate(values, ref root.Left) + Calculate(values, ref root.Right));
            if (root.Key.Equals("-") && (root.Left == null)) return (-Calculate(values, ref root.Right));
            if (root.Key.Equals("-")) return (Calculate(values, ref root.Left) - Calculate(values, ref root.Right));
            if (root.Key.Equals("*")) return (Calculate(values, ref root.Left) * Calculate(values, ref root.Right));
            if (root.Key.Equals("/"))
            {
                if (Check(Math.Abs(Calculate(values, ref root.Right)), DivisionByZero)) return (Calculate(values, ref root.Left) / Calculate(values, ref root.Right));
                return 0;
            }

            if (root.Key.Equals("^")) return (Math.Pow(Calculate(values, ref root.Left), Calculate(values, ref root.Right)));
            if (root.Key.Equals("cosh")) return Math.Cosh(Calculate(values, ref root.Left));
            if (root.Key.Equals("sinh")) return Math.Sinh(Calculate(values, ref root.Left));
            if (root.Key.Equals("sin")) return Math.Sin(Calculate(values, ref root.Left));
            if (root.Key.Equals("cos")) return Math.Cos(Calculate(values, ref root.Left));
            if (root.Key.Equals("exp")) return Math.Exp(Calculate(values, ref root.Left));

            if (root.Key.Equals("ln"))
            {
                return Check(Calculate(values, ref root.Left), LogarithmDomain) ? Math.Log(Calculate(values, ref root.Left)) : 0;
            }
            if (root.Key.Equals("log"))
            {
                return Check(Calculate(values, ref root.Left), LogarithmDomain) ? Math.Log10(Calculate(values, ref root.Left)) : 0;
            }
            double a = 0;
            if (root.Key.Equals("tan"))
            {
                return Check(Math.Abs(Math.Cos(Calculate(values, ref root.Left))), DivisionByZero) ? Math.Tan(Calculate(values, ref root.Left)) : 0;
            }
            if (root.Key.Equals("ctan"))
            {
                return Check(Math.Abs(Math.Sin(Calculate(values, ref root.Left))), DivisionByZero)
                           ? 1/Math.Tan(Calculate(values, ref root.Left))
                           : 0;
            }
            if (root.Key.Equals("acos"))
            {
                a = Calculate(values, ref root.Left);
                if ((a < -1) || (a > 1))
                {
                    throw new Exception(ArccosineDomain);
                }
                return Math.Acos(a);
            }

            if (root.Key.Equals("asin"))
            {
                a = Calculate(values, ref root.Left);
                if ((a < -1) || (a > 1))
                {
                    throw new Exception(ArcsineDomain);
                }
                return Math.Asin(a);
            }

            if (root.Key.Equals("atan")) return Math.Atan(Calculate(values, ref root.Left));
            if (root.Key.Equals("actan")) return PI / 2 - Math.Atan(Calculate(values, ref root.Left));
            if (root.Key.Equals("min")) return Math.Min(Calculate(values, ref root.Left), Calculate(values, ref root.Right));
            if (root.Key.Equals("max")) return Math.Max(Calculate(values, ref root.Left), Calculate(values, ref root.Right));
            if (root.Key.Equals("tanh")) return Math.Tanh(Calculate(values, ref root.Left));
            if (root.Key.Equals("abs")) return Math.Abs(Calculate(values, ref root.Left));
            
            if (root.Key.Equals("ctanh"))
            {
                if (Check(Math.Abs(Math.Sinh(Calculate(values, ref root.Left))), DivisionByZero)) return 1 / Math.Tanh(a);
                return 0;
            }

            if (root.Key.Equals("sqrt"))
            {
                a = Calculate(values, ref root.Left);
                if (a < 0)
                {
                    throw new Exception(RootDomain);
                }
                return Math.Sqrt(a);
            }
            if (root.Key.Equals("pi"))
                return PI;
            if (root.Key.Equals("e"))
                return E;
            foreach (string variable in values.Keys)
                if (root.Key.Equals(variable))
                    return Double.Parse(values[variable].ToString());
            return Double.Parse(root.Key);
        }
    }

    public class Node
    {
        public Node Left;
        public Node Right;
        public string Key;
        public Node(string k)
        {
            Left=null;
            Right=null;
            Key=k;
        }
    }
}
