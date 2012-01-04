using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    public partial class ClientParametrEditWindow : Window
    {
        private const string ResultParamCombo = "resultparamcombo";
        private const string ParameterPanel = "parameterpanel";
        private const string ParamCombo = "paramcombo";
        private const string CompareCombo = "comparecombo";
        private const string ValueControl = "valuecontrol";
        private const string ResultPanel = "resultpanel";

        readonly List<Term> _terms;
        //readonly List<CreditParameter> _creditParameters;
        //readonly Rule _rule;

        public ClientParametrEditWindow()
        {
            Title = "Новый параметр клиента";
            _parameters = Helpers.ReadParametersList();
            _creditParameters = Helpers.ReadCreditParametersList();
            _creditParameters.Sort();
            _parameters.Sort();

            InitializeComponent();
            InitializeResult();
        }

        public ClientParametrEditWindow(Parameter param_id)
        {
            Title = "Редактирование параметра клиента";
            _rule = rule;
            _parameters = Helpers.ReadParametersList();
            _terms = Helpers.ReadTermsList(int.Parse(param_id.ParamId.ToString()));
            _creditParameters = Helpers.ReadCreditParametersList();
            _creditParameters.Sort();
            _parameters.Sort();

            InitializeComponent();
            PriorityBox.Text = rule.RulePriority.ToString();

            foreach (Condition con in rule.Conditions)
                AddParameter(con.Parameter.ParamId, con.Comparision.GetStringValue(), con.Value.ToString());
            InitializeResult(rule.Result.ParamId, rule.ResultValue);
        }

        private void InitializeResult(int? resultId = null, string resultValue = null)
        {
            var wp = new WrapPanel { Name = ResultPanel };
            Grid.SetRow(wp, 0);
            editRule.Children.Add(wp);

            var textBox = new TextBlock { Text = "Вывод:" };
            wp.Children.Add(textBox);

            var result = new ComboBox { Name = ResultParamCombo, Margin = new Thickness(10, 0, 0, 0), MinWidth = 200 };
            FillParamCombo(result);
            wp.Children.Add(result);

            textBox = new TextBlock { Text = " = " };
            wp.Children.Add(textBox);

            var value = new ComboBox { Name = ValueControl, Width = 200 };
            wp.Children.Add(value);

            result.SelectedIndex = resultId != null ? _parameters.IndexOf(_parameters.First(a => a.ParamId == resultId)) : 0;

            if (resultValue != null)
                ((ComboBox)wp.Children.FindByName(ValueControl)).Text = resultValue;
        }


        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            AddTerm();
        }

        private void AddTerm(int? termId = null, string compareType = null, string termValue = null)
        {
            editRule.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var wp = new WrapPanel { Name = ParameterPanel };
            Grid.SetRow(wp, editRule.RowDefinitions.Count - 1);
            editRule.Children.Add(wp);

            var parameter = new ComboBox { Name = ParamCombo, MinWidth = 200 };
            FillParamCombo(parameter);
            wp.Children.Add(parameter);

            var comparison = new ComboBox { Name = CompareCombo, Width = 50, Margin = new Thickness(5, 0, 0, 0) };
            wp.Children.Add(comparison);

            var value = new ComboBox { IsEditable = true, MinWidth = 200, Name = ValueControl, Margin = new Thickness(5, 0, 0, 0) };
            FillValueCombo(paramId, value);
            wp.Children.Add(value);

            var deleteBox = new Button { Height = 20, Width = 20, Margin = new Thickness(10, 0, 0, 0) };
            deleteBox.Click += deleteBox_Click;
            deleteBox.Content = new Image { Source = Helpers.BitmapSourceFromBitmap(Properties.Resources.delete) };
            wp.Children.Add(deleteBox);

            parameter.SelectedIndex = paramId != null ? _parameters.IndexOf(_parameters.First(a => a.ParamId == paramId)) : 0;
            if (compareType != null)
                comparison.SelectedValue = compareType;
            if (paramValue != null)
                ((ComboBox)wp.Children.FindByName(ValueControl)).Text = paramValue;
        }

        private void FillValueCombo(int? paramId, ComboBox value)
        {
            if (paramId != null)
            {
                Parameter par = _parameters.First(a => a.ParamId == (int)paramId);
                foreach (CreditParameter p in _creditParameters)
                    if (par.ParamType == p.ParamType)
                        value.Items.Add(new ComboBoxItem { Content = p.ParamName, Tag = p.ParamId });
            }
            else
                foreach (CreditParameter p in _creditParameters)
                    value.Items.Add(new ComboBoxItem { Content = p.ParamName, Tag = p.ParamId });
        }

        private void FillParamCombo(ComboBox parameter)
        {
            foreach (Parameter p in _parameters)
                parameter.Items.Add(new ComboBoxItem { Tag = p.ParamId, Content = p.ParamName + " (" + p.ParamType.GetStringValue() + ")" });
            parameter.SelectionChanged += ParameterSelectionChanged;
        }

        void deleteBox_Click(object sender, RoutedEventArgs e)
        {
            var wp = ((Button)sender).Parent as WrapPanel;
            ((Grid)wp.Parent).Children.Remove(wp);
        }

        void ParameterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var changed = (ComboBox)sender;
            var wp = (WrapPanel)changed.Parent;
            var parId = (int)((ComboBoxItem)changed.SelectedItem).Tag;
            Parameter par = _parameters.First(a => a.ParamId == parId);
            int valueIndex = wp.Children.IndexOf(wp.Children.FindByName(ValueControl));

            if (changed.Name != ResultParamCombo)
            {
                var comparision = wp.Children.FindByName(CompareCombo) as ComboBox;
                comparision.Items.Clear();
                switch (par.ParamType)
                {
                    case ParamType.PString:
                        comparision.Items.Add(Comparision.Equals.GetStringValue());
                        comparision.Items.Add(Comparision.NotEquals.GetStringValue());
                        break;
                    case ParamType.PBool:
                    case ParamType.PDouble:
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

            wp.Children.Remove(wp.Children.FindByName(ValueControl));
            switch (par.ParamType)
            {
                case ParamType.PBool:
                    var valueb = new ComboBox { Name = ValueControl, MinWidth = 200, Margin = new Thickness(5, 0, 0, 0) };
                    valueb.Items.Add(Boolean.TrueString);
                    valueb.Items.Add(Boolean.FalseString);
                    valueb.SelectedIndex = 0;
                    wp.Children.Insert(valueIndex, valueb);
                    FillValueCombo(par.ParamId, valueb);
                    break;
                case ParamType.PString:
                    var value = new ComboBox { IsEditable = true, MinWidth = 200, Name = ValueControl, Margin = new Thickness(5, 0, 0, 0) };
                    FillValueCombo(par.ParamId, value);
                    wp.Children.Insert(valueIndex, value);
                    break;
                case ParamType.PDouble:
                    var values = new ComboBox { IsEditable = true, Name = ValueControl, MinWidth = 200, Margin = new Thickness(5, 0, 0, 0), Text = "0" };
                    FillValueCombo(par.ParamId, values);
                    if (changed.Name != ResultParamCombo)
                    {
                        var st = new Style();
                        st.Setters.Add(new EventSetter { Event = TextBoxBase.TextChangedEvent, Handler = new TextChangedEventHandler(ValuesTextChanged) });
                        values.Style = st;
                    }

                    wp.Children.Insert(valueIndex, values);
                    break;
            }
        }

        private string _previousText = String.Empty;
        void ValuesTextChanged(object sender, TextChangedEventArgs e)
        {
            double num;
            string text = ((ComboBox)sender).Text;
            if (_creditParameters.Exists(a => a.ParamName == text))
            {
                _previousText = text;
                return;
            }
            bool success = double.TryParse(text, out num);
            if (success & num >= 0)
                _previousText = text;
            else
                ((ComboBox)sender).Text = _previousText;
        }

        void ValueTextChanged(object sender, TextChangedEventArgs e)
        {
            double num;
            string text = ((TextBox)sender).Text;
            if (_creditParameters.Exists(a => a.ParamName == text))
            {
                _previousText = text;
                return;
            }
            bool success = double.TryParse(text, out num);
            if (success & num >= 0)
                _previousText = text;
            else
                ((TextBox)sender).Text = _previousText;
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            int ruleId = 0;
            if (_rule != null)
            {
                ConnectionManager.ExecuteNonQuery(String.Format(@"delete from rule where rule_id={0}", _rule.RuleId));
                ruleId = _rule.RuleId;
            }
            foreach (UIElement uie in editRule.Children)
            {
                if (!(uie is WrapPanel)) continue;
                var wp = uie as WrapPanel;
                if (wp.Name != ResultPanel) continue;
                using (var command = new SQLiteCommand(ConnectionManager.Connection))
                {
                    if (ruleId != 0)
                    {
                        command.CommandText = "insert into rule (rule_id, rule_result_param_id, rule_result_param_value,rule_priority) values (@rule_id, @rule_result_param_id, @rule_result_param_value,@rule_priority)";
                        command.Parameters.Add(new SQLiteParameter("@rule_id", ruleId));
                        command.Parameters.Add(new SQLiteParameter("@rule_result_param_id", ((wp.Children.FindByName(ResultParamCombo) as ComboBox).SelectedItem as ComboBoxItem).Tag));
                        var _valuecontrol = wp.Children.FindByName(ValueControl);
                        command.Parameters.Add(new SQLiteParameter("@rule_result_param_value", (_valuecontrol is ComboBox ? (_valuecontrol as ComboBox).Text : (_valuecontrol as TextBox).Text)));
                        command.Parameters.Add(new SQLiteParameter("@rule_priority", PriorityBox.Text));
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        command.CommandText = "insert into rule (rule_result_param_id, rule_result_param_value,rule_priority) values (@rule_result_param_id, @rule_result_param_value, @rule_priority);SELECT last_insert_rowid() AS [ID]";
                        command.Parameters.Add(new SQLiteParameter("@rule_result_param_id", ((wp.Children.FindByName(ResultParamCombo) as ComboBox).SelectedItem as ComboBoxItem).Tag));
                        var _valuecontrol = wp.Children.FindByName(ValueControl);
                        command.Parameters.Add(new SQLiteParameter("@rule_result_param_value", (_valuecontrol is ComboBox ? (_valuecontrol as ComboBox).Text : (_valuecontrol as TextBox).Text)));
                        command.Parameters.Add(new SQLiteParameter("@rule_priority", PriorityBox.Text));
                        ruleId = int.Parse(command.ExecuteScalar().ToString());
                    }
                }
            }

            foreach (UIElement uie in editRule.Children)
            {
                if (!(uie is WrapPanel)) continue;
                var wp = uie as WrapPanel;
                if (wp.Name != ParameterPanel) continue;
                using (var command = new SQLiteCommand(ConnectionManager.Connection))
                {
                    command.CommandText = @"insert into rule_left (rule_id,param_id,compare_type,value) values (@rule_id,@param_id,@compare_type,@value)";
                    command.Parameters.Add(new SQLiteParameter("@rule_id", ruleId));
                    command.Parameters.Add(new SQLiteParameter("@param_id", ((ComboBoxItem)((ComboBox)wp.Children.FindByName(ParamCombo)).SelectedItem).Tag));
                    command.Parameters.Add(new SQLiteParameter("@compare_type", ((ComboBox)wp.Children.FindByName(CompareCombo)).SelectedValue.ToString()));
                    var _valuecontrol = wp.Children.FindByName(ValueControl);
                    command.Parameters.Add(new SQLiteParameter("@value", (_valuecontrol is ComboBox ? (_valuecontrol as ComboBox).Text : ((TextBox)_valuecontrol).Text)));
                    command.ExecuteNonQuery();
                }
            }
            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
