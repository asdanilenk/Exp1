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
    public partial class EditRuleWindow : Window
    {
        private const string resultparamcombo = "resultparamcombo";
        private const string parameterpanel = "parameterpanel";
        private const string paramcombo = "paramcombo";
        private const string comparecombo = "comparecombo";
        private const string valuecontrol = "valuecontrol";
        private const string resultpanel = "resultpanel";

        List<param> parameters;
        List<param> credit_parameters;
        int rule_id = 0;

        public EditRuleWindow()
        {
            this.Title = "New Rule";
            InitializeComponent();
            parameters = Helpers.ReadParametersList(Helpers.ParameterScope.user);
            credit_parameters = Helpers.ReadParametersList(Helpers.ParameterScope.credit);
            InitializeResult();
        }

        public EditRuleWindow(int rule_id)
        {
            this.Title = "Edit Rule";
            this.rule_id = rule_id;

            InitializeComponent();
            parameters = Helpers.ReadParametersList(Helpers.ParameterScope.user);
            credit_parameters = Helpers.ReadParametersList(Helpers.ParameterScope.credit);

            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
            {
                command.CommandText = @"select * from vrule_left where rule_id=" + rule_id;
                SQLiteDataReader DataReader = command.ExecuteReader();
                while (DataReader.Read())
                {
                    AddParameter(int.Parse(DataReader["param_id"].ToString()),
                        DataReader["compare_type"].ToString(),
                        DataReader["value"].ToString());
                }
            }

            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.connection))
            {
                command.CommandText = @"select * from vrule_right where rule_id=" + rule_id;
                SQLiteDataReader DataReader = command.ExecuteReader();
                DataReader.Read();
                InitializeResult(int.Parse(DataReader["param_id"].ToString()),
                        DataReader["rule_result_param_value"].ToString());
                
            }
        }

        private void InitializeResult(int? result_id = null, string result_value = null)
        {
            WrapPanel wp = new WrapPanel();
            wp.Name = resultpanel;
            Grid.SetRow(wp, 0);
            editRule.Children.Add(wp);

            TextBlock textBox = new TextBlock();
            textBox.Text = "Result:";
            wp.Children.Add(textBox);

            ComboBox result = new ComboBox();
            result.Name = resultparamcombo;
            result.Margin = new Thickness(10, 0, 0, 0);
            result.Width = 200;
            FillParamCombo(result);
            wp.Children.Add(result);

            textBox = new TextBlock();
            textBox.Text = " = ";
            wp.Children.Add(textBox);

            ComboBox value = new ComboBox();
            value.Name = valuecontrol;
            result.Width = 200;
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
            RowDefinition row = new RowDefinition();
            row.Height = GridLength.Auto;
            editRule.RowDefinitions.Add(row);

            WrapPanel wp = new WrapPanel();
            wp.Name = parameterpanel;
            Grid.SetRow(wp, editRule.RowDefinitions.Count - 1);
            Grid.SetColumn(wp, 0);
            editRule.Children.Add(wp);

            ComboBox parameter = new ComboBox();
            parameter.Name = paramcombo;
            parameter.Width = 200;
            FillParamCombo(parameter);
            wp.Children.Add(parameter);

            ComboBox comparison = new ComboBox();
            comparison.Name = comparecombo;
            comparison.Width = 50;
            comparison.Margin = new Thickness(5, 0, 0, 0);
            wp.Children.Add(comparison);

            ComboBox value = new ComboBox();
            FillValueCombo(param_id, value);
            
            value.IsEditable = true;
            value.Width = 200;
            value.Name = valuecontrol;
            value.Margin = new Thickness(5, 0, 0, 0);
            wp.Children.Add(value);

            Button deleteBox = new Button();
            deleteBox.Height = 20;
            deleteBox.Width = 20;
            deleteBox.Margin = new Thickness(10, 0, 0, 0);
            deleteBox.Click += new RoutedEventHandler(deleteBox_Click);
            Image finalImage = new Image();
            BitmapSource b = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.delete);
            finalImage.Source = b;
            deleteBox.Content = finalImage;
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
                foreach (param p in credit_parameters)
                    if (par.param_type == p.param_type)
                        value.Items.Add(new ComboBoxItem() { Content = p.param_name, Tag = p.param_id });
            }
            else
                foreach (param p in credit_parameters)
                    value.Items.Add(new ComboBoxItem() { Content = p.param_name, Tag = p.param_id });
        }

        private void FillParamCombo(ComboBox parameter)
        {
            foreach (param p in parameters)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Tag = p.param_id;
                cbi.Content = p.param_name + " (" + p.param_type.GetStringValue() + ")";
                parameter.Items.Add(cbi);
            }
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
            int valueIndex = 3;

            if (changed.Name != resultparamcombo)
            {
                ComboBox comparision = wp.Children.FindByName(comparecombo) as ComboBox;
                comparision.Items.Clear();
                switch (par.param_type)
                {
                    case Param_type.p_bool:
                    case Param_type.p_string:
                        comparision.Items.Add("=");
                        comparision.Items.Add("!=");
                        break;
                    case Param_type.p_int:
                        comparision.Items.Add("=");
                        comparision.Items.Add("!=");
                        comparision.Items.Add(">");
                        comparision.Items.Add("<");
                        break;
                }
                comparision.SelectedIndex = 0;
                valueIndex = 2;
            }

            wp.Children.Remove(wp.Children.FindByName(valuecontrol));
            switch (par.param_type)
            {
                case Param_type.p_bool:
                    ComboBox valueb = new ComboBox();
                    valueb.Name = valuecontrol;
                    valueb.Width = 200;
                    valueb.Margin = new Thickness(5, 0, 0, 0);
                    valueb.Items.Add("true");
                    valueb.Items.Add("false");
                    valueb.SelectedIndex = 0;
                    wp.Children.Insert(valueIndex, valueb);
                    break;
                case Param_type.p_string:
                    ComboBox value = new ComboBox();
                    value.IsEditable = true;
                    FillValueCombo(par.param_id, value);
                    value.Width = 200;
                    value.Name = valuecontrol;
                    value.Margin = new Thickness(5, 0, 0, 0);
                    wp.Children.Insert(valueIndex, value);
                    break;
                case Param_type.p_int:
                    ComboBox values = new ComboBox();
                    values.IsEditable = true;
                    FillValueCombo(par.param_id, values);
                    values.Name = valuecontrol;
                    values.Width = 200;
                    values.Margin = new Thickness(5, 0, 0, 0);
                    values.Text = "0";
                    
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
            int num = 0;
            string text = ((ComboBox)sender).Text;
            if (credit_parameters.Exists(a => a.param_name == text))
            {
                previousText = text;
                return;
            }
            bool success = int.TryParse(text, out num);
            if (success & num >= 0)
                previousText = text;
            else
                ((ComboBox)sender).Text = previousText;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (rule_id != 0)
            {
                ConnectionManager.ExecuteNonQuery(@"delete from rule where rule_id=" + rule_id);
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
                                command.CommandText = "insert into rule (rule_id, rule_result_param_id, rule_result_param_value) values (@rule_id, @rule_result_param_id, @rule_result_param_value)";
                                command.Parameters.Add(new SQLiteParameter("@rule_id", rule_id));
                                command.Parameters.Add(new SQLiteParameter("@rule_result_param_id", ((wp.Children.FindByName(resultparamcombo) as ComboBox).SelectedItem as ComboBoxItem).Tag));
                                var _valuecontrol = wp.Children.FindByName(valuecontrol);
                                command.Parameters.Add(new SQLiteParameter("@rule_result_param_value", (_valuecontrol is ComboBox ? (_valuecontrol as ComboBox).Text : (_valuecontrol as TextBox).Text)));
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                command.CommandText = "insert into rule (rule_result_param_id, rule_result_param_value) values (@rule_result_param_id, @rule_result_param_value);SELECT last_insert_rowid() AS [ID]";
                                command.Parameters.Add(new SQLiteParameter("@rule_result_param_id", ((wp.Children.FindByName(resultparamcombo) as ComboBox).SelectedItem as ComboBoxItem).Tag));
                                var _valuecontrol = wp.Children.FindByName(valuecontrol);
                                command.Parameters.Add(new SQLiteParameter("@rule_result_param_value", (_valuecontrol is ComboBox ? (_valuecontrol as ComboBox).Text : (_valuecontrol as TextBox).Text)));
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
