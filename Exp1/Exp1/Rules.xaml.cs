using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SQLite;

namespace Exp1
{
    /// <summary>
    /// Interaction logic for Rules.xaml
    /// </summary>
    public partial class Rules : Window
    {
        public Rules()
        {
            InitializeComponent();
            ConnectionManager.filename = @"C:\Users\Артем\Desktop\Exp1\Exp1\data.db";
            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
            {
                RowDefinition gridRow;
                TextBlock txtBlock = new TextBlock();

                int rules_count = 1;
                int rule_id = 1;

                command.CommandText = @"select * from vrule_left order by rule_id";
                SQLiteDataReader DataReader = command.ExecuteReader();
                while (DataReader.Read())
                {
                    if (rule_id != int.Parse(DataReader["rule_id"].ToString()))
                    {
                        AppendRightPart(txtBlock, rule_id);
                        rule_id = int.Parse(DataReader["rule_id"].ToString());
                        rules_count++;
                    }

                    if (rules_count > this.rules.RowDefinitions.Count)
                    {
                        gridRow = new RowDefinition();
                        gridRow.Height = GridLength.Auto;
                        this.rules.RowDefinitions.Add(gridRow);
                        txtBlock = new TextBlock();
                        Grid.SetRow(txtBlock, rules_count - 1);
                        Grid.SetColumn(txtBlock, 0);

                        AddInline(txtBlock, "IF ", Colors.Blue);
                    }
                    else
                    {
                        AddInline(txtBlock, " AND ", Colors.Blue);
                    }
                    AddInline(txtBlock, "( ", Colors.Blue);
                    AddInline(txtBlock, DataReader["param_name"].ToString() + " ", Colors.Black);
                    AddInline(txtBlock, DataReader["compare_type"].ToString() + " ", Colors.Blue);
                    AddInline(txtBlock, DataReader["value"].ToString(), Colors.Black);
                    AddInline(txtBlock, " )", Colors.Blue);
                }
                AppendRightPart(txtBlock, rule_id);
            }
        }

        private void AddInline(TextBlock txtBlock, string text, Color color)
        {
            Run run = new Run();
            run.Text = text;
            run.Foreground = new SolidColorBrush(color);
            txtBlock.Inlines.Add(run);
        }

        private void AppendRightPart(TextBlock txtBlock, int rule_id)
        {
            using (SQLiteCommand command_inner = new SQLiteCommand(ConnectionManager.connection))
            {
                command_inner.CommandText = @"select * from vrule_right where rule_id=" + rule_id;
                SQLiteDataReader DataReader_inner = command_inner.ExecuteReader();
                DataReader_inner.Read();
                AddInline(txtBlock, " THEN ", Colors.Blue);
                AddInline(txtBlock, DataReader_inner["param_name"].ToString(), Colors.Black);
                AddInline(txtBlock, " = ", Colors.Blue);
                AddInline(txtBlock, DataReader_inner["rule_result_param_value"].ToString(), Colors.Black);
                rules.Children.Add(txtBlock);
            }
        }
    }
}
