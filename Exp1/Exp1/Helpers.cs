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

        public enum ParameterScope
        {
            [StringValue("vparam")]
            user,
            [StringValue("vcredit_param")]
            credit }
        public static List<param> ReadParametersList(ParameterScope scope)
        {
            List<param> parameters = new List<param>();
            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
            {
                command.CommandText = @"select * from " + scope.GetStringValue();
                SQLiteDataReader DataReader = command.ExecuteReader();
                while (DataReader.Read())
                {
                    parameters.Add(new param(int.Parse(DataReader["param_id"].ToString()),
                        DataReader["param_name"].ToString(),
                        DataReader["param_type"].ToString(),
                        int.Parse(DataReader["used"].ToString())));
                }
            }
            return parameters;
        }
    }
}
