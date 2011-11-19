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
            foreach (UIElement uie in col)
            {
                if (uie is FrameworkElement && (uie as FrameworkElement).Name == name)
                    return uie;
            }
            return null;
        }

        public static List<param> ReadParametersList()
        {
            List<param> parameters = new List<param>();
            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
            {
                command.CommandText = @"select * from vparam";
                SQLiteDataReader DataReader = command.ExecuteReader();
                while (DataReader.Read())
                {
                    parameters.Add(new param(int.Parse(DataReader["param_id"].ToString()),
                        DataReader["param_name"].ToString(),
                        DataReader["param_type"].ToString(),
                        int.Parse(DataReader["used"].ToString()),
                        DataReader["question"].ToString()));
                }
            }
            return parameters;
        }
        public static List<creditparam> ReadCreditParametersList()
        {
            List<creditparam> parameters = new List<creditparam>();
            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
            {
                command.CommandText = @"select * from vcredit_param";
                SQLiteDataReader DataReader = command.ExecuteReader();
                while (DataReader.Read())
                {
                    parameters.Add(new creditparam(int.Parse(DataReader["param_id"].ToString()),
                        DataReader["param_name"].ToString(),
                        DataReader["param_type"].ToString(),
                        int.Parse(DataReader["used"].ToString())));
                }
            }
            return parameters;
        }
        public static List<credit> ReadCreditsList()
        {
            List<credit> credits = new List<credit>();
            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
            {
                command.CommandText = @"select * from credit";
                SQLiteDataReader DataReader = command.ExecuteReader();
                while (DataReader.Read())
                {
                    credits.Add(new credit(int.Parse(DataReader["credit_id"].ToString()),
                        DataReader["credit_name"].ToString()));
                }
            }
            return credits;
        }
        public static Dictionary<creditparam, string> ReadCreditParams(int id)
        {
            Dictionary<creditparam, string> values = new Dictionary<creditparam, string>();
            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
            {
                command.CommandText = @"select * from vcredit_param_value where credit_id="+id;
                SQLiteDataReader DataReader = command.ExecuteReader();
                while (DataReader.Read())
                {
                    values.Add(new creditparam(int.Parse(DataReader["param_id"].ToString()),
                        DataReader["param_name"].ToString(),
                        DataReader["param_type"].ToString() ,
                        0), DataReader["value"].ToString());
                }
            }
            return values;
        }


        public static List<Rule> ReadRulesList()
        {
            List<param> pars = Helpers.ReadParametersList();
            List<Rule> rules = new List<Rule>();
            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
            {
                command.CommandText = @"select * from vrule_right";
                SQLiteDataReader DataReader = command.ExecuteReader();
                while (DataReader.Read())
                {
                    rules.Add(new Rule(int.Parse(DataReader["rule_id"].ToString()), pars.Find(a => a.param_id == int.Parse(DataReader["param_id"].ToString())),
                        DataReader["rule_result_param_value"].ToString(), int.Parse(DataReader["rule_priority"].ToString())));
                }
            }

            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
            {
                command.CommandText = @"select * from vrule_left";
                SQLiteDataReader DataReader = command.ExecuteReader();
                while (DataReader.Read())
                {
                    rules.Find(r => r.rule_id == int.Parse(DataReader["rule_id"].ToString())).conditions.Add(new Condition(
                        pars.Find(a => a.param_id == int.Parse(DataReader["param_id"].ToString())), DataReader["compare_type"].ToString(), DataReader["value"].ToString()));
                }
            }
            return rules;
        }


    }
}
