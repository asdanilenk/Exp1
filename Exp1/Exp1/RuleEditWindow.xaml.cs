using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Windows.Interop;

namespace Exp1
{
    /// <summary>
    /// Interaction logic for EditRule.xaml
    /// </summary>
    public partial class RuleEditWindow : Window
    {
        private const string resultparamcombo = "resultparamcombo";
        private const string parameterpanel = "parameterpanel";
        private const string paramcombo = "paramcombo";
        private const string comparecombo = "comparecombo";
        private const string valuecontrol = "valuecontrol";
        private const string resultpanel = "resultpanel";

        List<param> parameters;
        List<creditparam> credit_parameters;
        Rule rule;

        public RuleEditWindow()
        {
            this.Title = "Новое правило";
            parameters = Helpers.ReadParametersList();
            credit_parameters = Helpers.ReadCreditParametersList();
            credit_parameters.Sort();
            parameters.Sort();
            
            InitializeComponent();
            InitializeResult();
        }

        public RuleEditWindow(Rule rule)
        {
            this.Title = "Редактирование правила";
            this.rule = rule;
            parameters = Helpers.ReadParametersList();
            credit_parameters = Helpers.ReadCreditParametersList();
            credit_parameters.Sort();
            parameters.Sort();

            InitializeComponent();
            this.PriorityBox.Text = rule.rule_priority.ToString();

            foreach (Condition con in rule.conditions)
                    AddParameter(con.par.param_id, con.comparision.GetStringValue(), con.value.ToString());
            InitializeResult(rule.result.param_id, rule.resultvalue.ToString());
        }

        private void InitializeResult(int? result_id = null, string result_value = null)
        {
            WrapPanel wp = new WrapPanel() { Name = resultpanel };
            Grid.SetRow(wp, 0);
            editRule.Children.Add(wp);

            TextBlock textBox = new TextBlock() {Text= "Вывод:"};
            wp.Children.Add(textBox);

            ComboBox result = new ComboBox() { Name = resultparamcombo, Margin = new Thickness(10, 0, 0, 0), MinWidth = 200};
            FillParamCombo(result);
            wp.Children.Add(result);

            textBox = new TextBlock() { Text = " = " };
            wp.Children.Add(textBox);

            ComboBox value = new ComboBox() { Name = valuecontrol, Width = 200 };
            wp.Children.Add(value);

            if (result_id != null)
                result.SelectedIndex = parameters.IndexOf(parameters.First(a => a.param_id == result_id));
            else
                result.SelectedIndex = 0;

            if (result_value != null)
                (wp.Children.FindByName(valuecontrol) as ComboBox).Text = result_value;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddParameter();
        }

        private void AddParameter(int? param_id = null, string compare_type = null, string param_value = null)
        {
            editRule.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            WrapPanel wp = new WrapPanel() { Name = parameterpanel};
            Grid.SetRow(wp, editRule.RowDefinitions.Count - 1);
            editRule.Children.Add(wp);

            ComboBox parameter = new ComboBox() { Name = paramcombo, MinWidth = 200 };
            FillParamCombo(parameter);
            wp.Children.Add(parameter);

            ComboBox comparison = new ComboBox() { Name = comparecombo, Width = 50, Margin = new Thickness(5, 0, 0, 0) };
            wp.Children.Add(comparison);

            ComboBox value = new ComboBox() { IsEditable = true, MinWidth = 200, Name = valuecontrol, Margin = new Thickness(5, 0, 0, 0)};
            FillValueCombo(param_id, value);
            wp.Children.Add(value);

            Button deleteBox = new Button() { Height = 20, Width = 20, Margin = new Thickness(10, 0, 0, 0) };
            deleteBox.Click += new RoutedEventHandler(deleteBox_Click);
            deleteBox.Content = new Image() { Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.delete) }; 
            wp.Children.Add(deleteBox);

            if (param_id != null)
                parameter.SelectedIndex = parameters.IndexOf(parameters.First(a => a.param_id == param_id));
            else
                parameter.SelectedIndex = 0;
            if (compare_type != null)
                comparison.SelectedValue = compare_type;
            if (param_value != null)
               (wp.Children.FindByName(valuecontrol) as ComboBox).Text = param_value;
        }

        private void FillValueCombo(int? param_id, ComboBox value)
        {
            if (param_id != null)
            {
                param par = parameters.First(a => a.param_id == (int)param_id);
                foreach (creditparam p in credit_parameters)
                    if (par.param_type == p.param_type)
                        value.Items.Add(new ComboBoxItem() { Content = p.param_name, Tag = p.param_id });
            }
            else
                foreach (creditparam p in credit_parameters)
                    value.Items.Add(new ComboBoxItem() { Content = p.param_name, Tag = p.param_id });
        }

        private void FillParamCombo(ComboBox parameter)
        {
            foreach (param p in parameters)
                parameter.Items.Add(new ComboBoxItem() { Tag = p.param_id, Content = p.param_name + " (" + p.param_type.GetStringValue() + ")" });
            parameter.SelectionChanged += new SelectionChangedEventHandler(parameter_SelectionChanged);
        }

        void deleteBox_Click(object sender, RoutedEventArgs e)
        {
            WrapPanel wp = (sender as Button).Parent as WrapPanel;
            (wp.Parent as Grid).Children.Remove(wp);
        }

        void parameter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox changed = (ComboBox)sender;
            WrapPanel wp = (WrapPanel)changed.Parent;
            int par_id = (int)((ComboBoxItem)changed.SelectedItem).Tag;
            param par = parameters.First(a => a.param_id == par_id);
            int valueIndex = wp.Children.IndexOf(wp.Children.FindByName(valuecontrol));

            if (changed.Name != resultparamcombo)
            {
                ComboBox comparision = wp.Children.FindByName(comparecombo) as ComboBox;
                comparision.Items.Clear();
                switch (par.param_type)
                {
                    case Param_type.p_string:
                        comparision.Items.Add(Comparision.Equals.GetStringValue());
                        comparision.Items.Add(Comparision.NotEquals.GetStringValue());
                        break;
                    case Param_type.p_bool:
                    case Param_type.p_double:
                        comparision.Items.Add(Comparision.Equals.GetStringValue());
                        comparision.Items.Add(Comparision.NotEquals.GetStringValue());
                        comparision.Items.Add(Comparision.Less.GetStringValue());
                        comparision.Items.Add(Comparision.LessOrEquals.GetStringValue());
                        comparision.Items.Add(Comparision.Greater.GetStringValue());
                        comparision.Items.Add(Comparision.GreaterOrEquals.GetStringValue());
                        break;
                }
                comparision.SelectedIndex = 0;
            }

            wp.Children.Remove(wp.Children.FindByName(valuecontrol));
            switch (par.param_type)
            {
                case Param_type.p_bool:
                    ComboBox valueb = new ComboBox() { Name = valuecontrol, MinWidth = 200, Margin = new Thickness(5, 0, 0, 0) };
                    valueb.Items.Add(Boolean.TrueString);
                    valueb.Items.Add(Boolean.FalseString);
                    valueb.SelectedIndex = 0;
                    wp.Children.Insert(valueIndex, valueb);
                    FillValueCombo(par.param_id, valueb);
                    break;
                case Param_type.p_string:
                    ComboBox value = new ComboBox(){IsEditable = true, MinWidth = 200,Name = valuecontrol,Margin = new Thickness(5, 0, 0, 0)};
                    FillValueCombo(par.param_id, value);
                    wp.Children.Insert(valueIndex, value);
                    break;
                case Param_type.p_double:
                    ComboBox values = new ComboBox() { IsEditable = true, Name = valuecontrol, MinWidth = 200, Margin = new Thickness(5, 0, 0, 0), Text = "0" };
                    FillValueCombo(par.param_id, values);
                    
                    System.Windows.Style st = new System.Windows.Style();
                    st.Setters.Add(new EventSetter() { Event= TextBox.TextChangedEvent, Handler= new TextChangedEventHandler(values_TextChanged)});
                    values.Style = st;

                    wp.Children.Insert(valueIndex, values);
                    break;
            }
        }


        private string previousText = String.Empty;
        void values_TextChanged(object sender, TextChangedEventArgs e)
        {
            double num = 0;
            string text = ((ComboBox)sender).Text;
            if (credit_parameters.Exists(a => a.param_name == text))
            {
                previousText = text;
                return;
            }
            bool success = double.TryParse(text, out num);
            if (success & num >= 0)
                previousText = text;
            else
                ((ComboBox)sender).Text = previousText;
        }

        void value_TextChanged(object sender, TextChangedEventArgs e)
        {
            double num = 0;
            string text = ((TextBox)sender).Text;
            if (credit_parameters.Exists(a => a.param_name == text))
            {
                previousText = text;
                return;
            }
            bool success = double.TryParse(text, out num);
            if (success & num >= 0)
                previousText = text;
            else
                ((TextBox)sender).Text = previousText;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            int rule_id =0;
            if (rule != null)
            {
                ConnectionManager.ExecuteNonQuery(@"delete from rule where rule_id=" + rule.rule_id);
                rule_id = rule.rule_id;
            }
            foreach (UIElement uie in editRule.Children)
            {
                if (uie is WrapPanel)
                {
                    WrapPanel wp = uie as WrapPanel;
                    if (wp.Name == resultpanel)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
                        {
                            if (rule_id != 0)
                            {
                                command.CommandText = "insert into rule (rule_id, rule_result_param_id, rule_result_param_value,rule_priority) values (@rule_id, @rule_result_param_id, @rule_result_param_value,@rule_priority)";
                                command.Parameters.Add(new SQLiteParameter("@rule_id", rule_id));
                                command.Parameters.Add(new SQLiteParameter("@rule_result_param_id", ((wp.Children.FindByName(resultparamcombo) as ComboBox).SelectedItem as ComboBoxItem).Tag));
                                var _valuecontrol = wp.Children.FindByName(valuecontrol);
                                command.Parameters.Add(new SQLiteParameter("@rule_result_param_value", (_valuecontrol is ComboBox ? (_valuecontrol as ComboBox).Text : (_valuecontrol as TextBox).Text)));
                                command.Parameters.Add(new SQLiteParameter("@rule_priority", PriorityBox.Text));
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                command.CommandText = "insert into rule (rule_result_param_id, rule_result_param_value,rule_priority) values (@rule_result_param_id, @rule_result_param_value, @rule_priority);SELECT last_insert_rowid() AS [ID]";
                                command.Parameters.Add(new SQLiteParameter("@rule_result_param_id", ((wp.Children.FindByName(resultparamcombo) as ComboBox).SelectedItem as ComboBoxItem).Tag));
                                var _valuecontrol = wp.Children.FindByName(valuecontrol);
                                command.Parameters.Add(new SQLiteParameter("@rule_result_param_value", (_valuecontrol is ComboBox ? (_valuecontrol as ComboBox).Text : (_valuecontrol as TextBox).Text)));
                                command.Parameters.Add(new SQLiteParameter("@rule_priority", PriorityBox.Text));
                                rule_id = int.Parse(command.ExecuteScalar().ToString());
                            }
                        }
                    }
                }
            }
            
            foreach (UIElement uie in editRule.Children)
            {
                if (uie is WrapPanel)
                {
                    WrapPanel wp = uie as WrapPanel;
                    if (wp.Name == parameterpanel)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
                        {
                            command.CommandText = @"insert into rule_left (rule_id,param_id,compare_type,value) values (@rule_id,@param_id,@compare_type,@value)";
                            command.Parameters.Add(new SQLiteParameter("@rule_id", rule_id));
                            command.Parameters.Add(new SQLiteParameter("@param_id", ((wp.Children.FindByName(paramcombo) as ComboBox).SelectedItem as ComboBoxItem).Tag));
                            command.Parameters.Add(new SQLiteParameter("@compare_type", (wp.Children.FindByName(comparecombo) as ComboBox).SelectedValue.ToString()));
                            var _valuecontrol = wp.Children.FindByName(valuecontrol);
                            command.Parameters.Add(new SQLiteParameter("@value", (_valuecontrol is ComboBox ? (_valuecontrol as ComboBox).Text : (_valuecontrol as TextBox).Text)));
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
    }
}
