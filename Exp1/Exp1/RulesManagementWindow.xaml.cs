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
    public partial class RulesManagementWindow : Window
    {
        public RulesManagementWindow()
        {
            InitializeComponent();
            BuildRulesTable();
        }
        private List<Rule> ruleslist;

        private void BuildRulesTable()
        {
            ruleslist = Helpers.ReadRulesList();

            foreach (Rule rule in ruleslist)
            {
                this.rules.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                WrapPanel wp = new WrapPanel();
                Grid.SetColumn(wp, 0);
                Grid.SetRow(wp, this.rules.RowDefinitions.Count - 1);
                this.rules.Children.Add(wp);

                TextBlock textBlock = new TextBlock();
                AddInline(textBlock, rule.rule_priority+": IF", Colors.Blue);
                foreach (Condition rl in rule.conditions)
                {
                    AddInline(textBlock, "( ", Colors.Blue);
                    AddInline(textBlock, rl.par.param_name + " ", Colors.Black);
                    AddInline(textBlock, rl.comparision.GetStringValue() + " ", Colors.Blue);
                    AddInline(textBlock, rl.value.ToString(), Colors.Black);
                    AddInline(textBlock, " )", Colors.Blue);
                    if (rule.conditions.IndexOf(rl)!=rule.conditions.Count-1)
                        AddInline(textBlock, " AND ", Colors.Blue);
                }

                AddInline(textBlock, " THEN ", Colors.Blue);
                AddInline(textBlock, rule.result.param_name, Colors.Black);
                AddInline(textBlock, " = ", Colors.Blue);
                AddInline(textBlock, rule.resultvalue.ToString(), Colors.Black);
                wp.Children.Add(textBlock);
                
                Button editBox = new Button() { Height = 20, Width = 20, Margin = new Thickness(10, 0, 0, 0), Tag = rule.rule_id };
                editBox.Click += new RoutedEventHandler(edit_Click);
                editBox.Content = new Image() { Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.edit) };
                wp.Children.Add(editBox);

                Button deleteBox = new Button() { Height = 20, Width = 20, Margin = new Thickness(10, 0, 0, 0), Tag = rule.rule_id };
                deleteBox.Click += new RoutedEventHandler(deleteBox_Click);
                deleteBox.Content = new Image() { Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.delete) };
                wp.Children.Add(deleteBox);
            }
        }

        void deleteBox_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены что хотите удалить данное правило?", "Удалить?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                ConnectionManager.ExecuteNonQuery(String.Format("delete from rule where rule_id={0}",(sender as Button).Tag));
                RefreshTable();
            }
        }

        void edit_Click(object sender, RoutedEventArgs e)
        {
            int rule_id = (int)(sender as Button).Tag;
            Rule r = ruleslist.Find(rr => rr.rule_id == rule_id);
            (new RuleEditWindow(r)).ShowDialog();
        }

        private void AddInline(TextBlock txtBlock, string text, Color color)
        {
            Run run = new Run();
            run.Text = text;
            run.Foreground = new SolidColorBrush(color);
            txtBlock.Inlines.Add(run);
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
            (new RuleEditWindow()).ShowDialog();
        }

        private void clientParamButton_Click(object sender, RoutedEventArgs e)
        {
            (new ParametersManagementWindow()).ShowDialog();
        }

        private void creditParamButton_Click(object sender, RoutedEventArgs e)
        {
            (new CreditParametersManagementWindow()).ShowDialog();
        }
    }
}
