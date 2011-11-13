using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
            BuildRulesTable();
        }

        private void BuildRulesTable()
        {
            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
            {
                RowDefinition gridRow;
                TextBlock textBlock = new TextBlock();

                int rules_count = 1;
                int rule_id = 1;

                command.CommandText = @"select * from vrule_left order by rule_id";
                SQLiteDataReader DataReader = command.ExecuteReader();
                while (DataReader.Read())
                {
                    if (rule_id != int.Parse(DataReader["rule_id"].ToString()))
                    {
                        AppendRightPart(textBlock, rule_id);
                        rule_id = int.Parse(DataReader["rule_id"].ToString());
                        rules_count++;
                    }

                    if (rules_count > this.rules.RowDefinitions.Count)
                    {
                        gridRow = new RowDefinition();
                        gridRow.Height = GridLength.Auto;
                        this.rules.RowDefinitions.Add(gridRow);

                        WrapPanel wp = new WrapPanel();
                        Grid.SetColumn(wp, 0);
                        Grid.SetRow(wp, rules_count - 1);
                        rules.Children.Add(wp);

                        Button editBox = new Button();
                        editBox.Height = 20;
                        editBox.Width = 20;
                        editBox.Margin = new Thickness(10, 0, 0, 0);
                        editBox.Tag = rule_id;
                        editBox.Click += new RoutedEventHandler(edit_Click);
                        Image editImage = new Image();
                        BitmapSource editsource = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.edit);
                        editImage.Source = editsource;
                        editBox.Content = editImage;

                        Button deleteBox = new Button();
                        deleteBox.Height = 20;
                        deleteBox.Width = 20;
                        deleteBox.Margin = new Thickness(5, 0, 0, 0);
                        deleteBox.Tag = rule_id;
                        deleteBox.Click += new RoutedEventHandler(deleteBox_Click);
                        Image deleteImage = new Image();
                        BitmapSource deletesource = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.delete);
                        deleteImage.Source = deletesource;
                        deleteBox.Content = deleteImage;

                        textBlock = new TextBlock();
                        Grid.SetRow(textBlock, rules_count - 1);
                        Grid.SetColumn(textBlock, 0);
                        AddInline(textBlock, "IF ", Colors.Blue);

                        wp.Children.Add(textBlock);
                        wp.Children.Add(editBox);
                        wp.Children.Add(deleteBox);
                    }
                    else
                    {
                        AddInline(textBlock, " AND ", Colors.Blue);
                    }
                    AddInline(textBlock, "( ", Colors.Blue);
                    AddInline(textBlock, DataReader["param_name"].ToString() + " ", Colors.Black);
                    AddInline(textBlock, DataReader["compare_type"].ToString() + " ", Colors.Blue);
                    AddInline(textBlock, DataReader["value"].ToString(), Colors.Black);
                    AddInline(textBlock, " )", Colors.Blue);
                }
                AppendRightPart(textBlock, rule_id);
            }
        }

        void deleteBox_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure that you want to delete selected rule?", "Delete?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                ConnectionManager.ExecuteNonQuery("delete from rule where rule_id=" + (sender as Button).Tag);
                RefreshTable();
            }
        }

        void edit_Click(object sender, RoutedEventArgs e)
        {
            int rule_id = (int)(sender as Button).Tag;
            (new EditRuleWindow(rule_id)).ShowDialog();
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
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            RefreshTable();
        }

        private void RefreshTable()
        {
            this.rules.RowDefinitions.Clear();
            this.rules.Children.Clear();
            BuildRulesTable();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            (new EditRuleWindow()).ShowDialog();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            (new ManageParametersWindow()).ShowDialog();
        }
    }
}
