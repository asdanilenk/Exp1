using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Exp1
{
    public class Parser
    {
        static string WRONG_BRACKETS_ORDER = "Wrong brackets order: ";
        static string UNKNOWN_FUNCTION = "Unknown function or variable: ";
        static string DIVISION_BY_ZERO = "Division by zero";
        static string LOGARITHM_DOMAIN = "Argument outside the domain of logarithm";
        static string ARCCOSINE_DOMAIN = "Argument outside the domain of arccosine";
        static string ARCSINE_DOMAIN = "Argument outside the domain of arcsine";
        static string ROOT_DOMAIN = "Argument outside the domain of square root";
        static string EMPTY_ARGUMENT = "Empty argument";
        static string ar = "+-*/^";

        Node root;
        double EPS = 0.0001;
        double PI = Math.PI;
        double E = Math.E;

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

            if ((i != -1) && (ar.LastIndexOf(str[i - 1]) == -1) && (ar.LastIndexOf(str[i + 1]) == -1))
            {
                string str1, str2, str3;
                str1 = str.Substring(0, i);
                str2 = str.Substring(i, 1);
                str3 = str.Substring(i + 1);
                root = new Node(str2);
                Parse(str1, ref root.left, variables);
                Parse(str3, ref root.right, variables);
                return true;
            }
            return false;
        }
        private bool SearchF(string str, string s1, ref Node root, List<string> variables)
        {
            string s = s1;
            if (str.StartsWith(s) && str.EndsWith(")"))
            {
                string str1;
                s = s.Substring(0, s.Length - 1);
                str1 = str.Substring(s.Length + 1, str.Length - s.Length - 2);
                root = new Node(s);
                Parse(str1, ref root.left, variables);
                return true;
            }
            return false;
        }
        public bool Parse(string str, List<string> variables)
        {
            return Parse(str, ref root, variables);
        }
        private bool Parse(string str, ref Node root, List<string> variables)
        {
            str = str.Trim();
            int i = 0;
            int c = 0;
            // Проверяет на пустую строку
            if (str.Equals(""))
            {
                throw new Exception(EMPTY_ARGUMENT);
            }
            // Проверяет что кол-во открытых скобок равно кол-ву закрытых

            for (i = 0; i < str.Length; i++)
            {
                if (str[i] == ')') c--;
                else if (str[i] == '(') c++;
                if (c < 0) break;
            }
            if (c != 0)
            {
                throw new Exception(WRONG_BRACKETS_ORDER);
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
            foreach (string variable in variables)
                if (str.Equals(variable))
                {
                    root = new Node(str);
                    return true;
                }
            //Проверяет на унарный минус

            if (str[0] == '-')
            {
                root = new Node("-");
                str = str.Substring(1);
                Parse(str, ref root.right, variables);
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
            SearchF(str, "sqrt(", ref root, variables)) return true;

            throw new Exception(UNKNOWN_FUNCTION);
        }

        private bool Check(double a, string MSG)
        {
            if (a < EPS)
            {
                throw new Exception(MSG);
            }
            return true;
        }
        public double Calculate(Dictionary<string, Double> values)
        {
            return Calculate(values, ref root);
        }

        private double Calculate(Dictionary<string, Double> values, ref Node root)
        {
            if (root.key.Equals("+")) return (Calculate(values, ref root.left) + Calculate(values, ref root.right));
            if (root.key.Equals("-") && (root.left == null)) return (-Calculate(values, ref root.right));
            if (root.key.Equals("-")) return (Calculate(values, ref root.left) - Calculate(values, ref root.right));
            if (root.key.Equals("*")) return (Calculate(values, ref root.left) * Calculate(values, ref root.right));
            if (root.key.Equals("/"))
            {
                if (Check(Math.Abs(Calculate(values, ref root.right)), DIVISION_BY_ZERO)) return (Calculate(values, ref root.left) / Calculate(values, ref root.right));
                else return 0;
            }

            if (root.key.Equals("^")) return (Math.Pow(Calculate(values, ref root.left), Calculate(values, ref root.right)));
            if (root.key.Equals("cosh")) return Math.Cosh(Calculate(values, ref root.left));
            if (root.key.Equals("sinh")) return Math.Sinh(Calculate(values, ref root.left));
            if (root.key.Equals("sin")) return Math.Sin(Calculate(values, ref root.left));
            if (root.key.Equals("cos")) return Math.Cos(Calculate(values, ref root.left));
            if (root.key.Equals("exp")) return Math.Exp(Calculate(values, ref root.left));

            if (root.key.Equals("ln"))
            {
                if (Check(Calculate(values, ref root.left), LOGARITHM_DOMAIN)) return Math.Log(Calculate(values, ref root.left));
                else return 0;
            }
            if (root.key.Equals("log"))
            {
                if (Check(Calculate(values, ref root.left), LOGARITHM_DOMAIN)) return Math.Log10(Calculate(values, ref root.left));
                else return 0;
            }
            double a = 0;
            if (root.key.Equals("tan"))
            {
                if (Check(Math.Abs(Math.Cos(Calculate(values, ref root.left))), DIVISION_BY_ZERO)) return Math.Tan(Calculate(values, ref root.left));
                else return 0;
            }
            if (root.key.Equals("ctan"))
            {
                if (Check(Math.Abs(Math.Sin(Calculate(values, ref root.left))), DIVISION_BY_ZERO)) return 1 / Math.Tan(Calculate(values, ref root.left));
                else return 0;
            }
            if (root.key.Equals("acos"))
            {
                a = Calculate(values, ref root.left);
                if ((a < -1) || (a > 1))
                {
                    throw new Exception(ARCCOSINE_DOMAIN);
                }
                else return Math.Acos(a);
            }

            if (root.key.Equals("asin"))
            {
                a = Calculate(values, ref root.left);
                if ((a < -1) || (a > 1))
                {
                    throw new Exception(ARCSINE_DOMAIN);
                }
                else return Math.Asin(a);
            }

            if (root.key.Equals("atan")) return Math.Atan(Calculate(values, ref root.left));
            if (root.key.Equals("actan")) return PI / 2 - Math.Atan(Calculate(values, ref root.left));
            if (root.key.Equals("tanh")) return Math.Tanh(Calculate(values, ref root.left));
            if (root.key.Equals("ctanh"))
            {
                if (Check(Math.Abs(Math.Sinh(Calculate(values, ref root.left))), DIVISION_BY_ZERO)) return 1 / Math.Tanh(a);
                else return 0;
            }

            if (root.key.Equals("sqrt"))
            {
                a = Calculate(values, ref root.left);
                if (a < 0)
                {
                    throw new Exception(ROOT_DOMAIN);
                }
                else return Math.Sqrt(a);
            }
            if (root.key.Equals("pi"))
                return PI;
            if (root.key.Equals("e"))
                return E;
            foreach (string variable in values.Keys)
                if (root.key.Equals((string)variable))
                    return Double.Parse(values[variable].ToString());
            return Double.Parse(root.key);
        }
    }

    public class Node
    {
        public Node left;
        public Node right;
        public string key;
        public Node(string k)
        {
            left=null;
            right=null;
            key=k;
        }
    }
}
