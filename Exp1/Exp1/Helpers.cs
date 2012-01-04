using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Data.SQLite;

namespace Exp1
{
    static class Helpers
    {
        public static BitmapSource BitmapSourceFromBitmap(Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
        }

        public static UIElement FindByName(this UIElementCollection col, string name)
        {
            return col.Cast<UIElement>().FirstOrDefault(uie => uie is FrameworkElement && (uie as FrameworkElement).Name == name);
        }

        public static List<Parameter> ReadParametersList()
        {
            var parameters = new List<Parameter>();
            var terms = new List<Term>();
            using (var command = new SQLiteCommand(ConnectionManager.Connection))
            {
                command.CommandText = @"select * from vparam";
                SQLiteDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    parameters.Add(new Parameter(int.Parse(dataReader["param_id"].ToString()),
                        dataReader["param_name"].ToString(),
                        dataReader["param_type"].ToString(),
                        int.Parse(dataReader["used"].ToString()),
                        dataReader["question"].ToString(), new List<Term>()));
                }
            }


            using (var command = new SQLiteCommand(ConnectionManager.Connection))
            {
                command.CommandText = @"select * from v_term_list";
                SQLiteDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    parameters.Find(p => p.ParamId == int.Parse(dataReader["param_id"].ToString())).Term.Add(new Term(int.Parse(dataReader["term_id"].ToString()), dataReader["term_name"].ToString(), dataReader["term_function"].ToString(), int.Parse(dataReader["used"].ToString()), int.Parse(dataReader["left_range"].ToString()), int.Parse(dataReader["right_range"].ToString())));
                }
            }

            return parameters;
        }

        public static List<Term> ReadTermsList( int param_id)
        {
            var terms = new List<Term>();
            using (var command = new SQLiteCommand(ConnectionManager.Connection))
            {
                command.CommandText = @"select * from v_term_list";
                SQLiteDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    terms.Add(new Term(int.Parse(dataReader["term_id"].ToString()), 
                        dataReader["term_name"].ToString(),
                        dataReader["term_function"].ToString(),
                        int.Parse(dataReader["used"].ToString()), int.Parse(dataReader["left_range"].ToString()), int.Parse(dataReader["right_range"].ToString())));
                }
            }

            return terms;
        }

        public static List<CreditParameter> ReadCreditParametersList()
        {
            var parameters = new List<CreditParameter>();
            using (var command = new SQLiteCommand(ConnectionManager.Connection))
            {
                command.CommandText = @"select * from vcredit_param";
                SQLiteDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    parameters.Add(new CreditParameter(int.Parse(dataReader["param_id"].ToString()),
                        dataReader["param_name"].ToString(),
                        dataReader["param_type"].ToString(),
                        int.Parse(dataReader["used"].ToString())));
                }
            }
            return parameters;
        }
        public static List<Credit> ReadCreditsList()
        {
            var credits = new List<Credit>();
            using (var command = new SQLiteCommand(ConnectionManager.Connection))
            {
                command.CommandText = @"select * from credit";
                //MessageBox.Show(command.CommandText);
                SQLiteDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    credits.Add(new Credit(int.Parse(dataReader["credit_id"].ToString()),
                        dataReader["credit_name"].ToString()));
                }
            }
            return credits;
        }
        public static Dictionary<CreditParameter, string> ReadCreditParams(int id)
        {
            var values = new Dictionary<CreditParameter, string>();
            using (var command = new SQLiteCommand(ConnectionManager.Connection))
            {
                command.CommandText = @"select * from vcredit_param_value where credit_id="+id;
                SQLiteDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    values.Add(new CreditParameter(int.Parse(dataReader["param_id"].ToString()),
                        dataReader["param_name"].ToString(),
                        dataReader["param_type"].ToString() ,
                        0), dataReader["value"].ToString());
                }
            }
            return values;
        }


        public static List<Rule> ReadRulesList()
        {
            var pars = ReadParametersList();
            var rules = new List<Rule>();
            using (var command = new SQLiteCommand(ConnectionManager.Connection))
            {
                command.CommandText = @"select * from vrule_right";
                SQLiteDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    rules.Add(new Rule(int.Parse(dataReader["rule_id"].ToString()), pars.Find(a => a.ParamId == int.Parse(dataReader["param_id"].ToString())),
                        dataReader["rule_result_param_value"].ToString(), int.Parse(dataReader["rule_priority"].ToString())));
                }
            }

            using (var command = new SQLiteCommand(ConnectionManager.Connection))
            {
                command.CommandText = @"select * from vrule_left";
                SQLiteDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    rules.Find(r => r.RuleId == int.Parse(dataReader["rule_id"].ToString())).Conditions.Add(new Condition(
                        pars.Find(a => a.ParamId == int.Parse(dataReader["param_id"].ToString())), dataReader["compare_type"].ToString(), dataReader["value"].ToString()));
                }
            }
            string aq = String.Empty;
            foreach (Rule b in rules)
                aq += b + "\r\n";
            return rules;
        }

        public static Dictionary<string, double> ToValueList(this Dictionary<Parameter, object> param)
        {
            return param.Where(par => par.Key.ParamType == ParamType.PDouble && par.Value != null).ToDictionary(par => par.Key.ParamName, par => (double) par.Value);
        }
    }
}
