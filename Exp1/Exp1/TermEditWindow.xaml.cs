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
    public partial class TermEditWindow : Window
    {
        private const string ResultParamCombo = "resultparamcombo";
        private const string ParameterPanel = "parameterpanel";
        private const string ParamCombo = "paramcombo";
        private const string CompareCombo = "comparecombo";
        private const string ValueControl = "valuecontrol";
        private const string ResultPanel = "resultpanel";

        private const string TermPanel = "termpanel";
        private const string TermText = "TermNameTexBox";
        private const string TermFunc = "TermFunctionTexBox";
        private const string TermTitle = "TermTitle";
        private const string TermRightRange = "TermRightRange";
        private const string TermLeftRange = "TermLeftRange";
        private const string TermSymbols = "TermSymbols";
        private const string btnDelete = "btnDelete";
        private const string ComparableNumText = "ComparableNumText";

        private const int TermCountRowsAlwaysExists = 4;

        readonly List<Term> _terms;
        //readonly Parameter _Param;
        int _group_id;
        //readonly Rule _rule;

        public TermEditWindow()
        {
            Title = "Новая группа термов";
            _terms = null;
            _group_id = 0;
            InitializeComponent();
            InitializeForm();
       }

        public TermEditWindow(int group_id, string group_name)
        {
            Title = "Редактирование группы термов";
            _terms = Helpers.ReadTermList(group_id);
            _group_id = group_id;
            InitializeComponent();
            
            InitializeForm(group_id, group_name);

            foreach (Term trm in _terms)
                AddTerm(trm.TermId, trm.TermName, trm.TermFunction, trm.TermUsed, trm.LeftRange, trm.RightRange, trm.ComparableNum);
        }

        private void InitializeForm(int? groupId = null, string groupName = null)
        {
            if (groupId != null)
            {
                GroupNameBox.Text = (groupName == null) ? "" : groupName;
                GroupNameBox.Tag = groupId;
            }
            else
                AddTerm();
            /*ParamTypeCombo.Items.Add(new ComboBoxItem() { Content = ParamType.PString.GetStringValue() });
            ParamTypeCombo.Items.Add(new ComboBoxItem() { Content = ParamType.PBool.GetStringValue() });
            ParamTypeCombo.Items.Add(new ComboBoxItem() { Content = ParamType.PDouble.GetStringValue() });
            ParamTypeCombo.Items.Add(new ComboBoxItem() { Content = ParamType.PFuzzy.GetStringValue() });
            
            ParamTypeCombo.SelectedIndex = 0;
            //wp.Children.Add(TermTypeCombo);
           
            /*var wp = new WrapPanel { Name = ResultPanel };
            Grid.SetRow(wp, 0);
            editParam.Children.Add(wp);

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
                ((ComboBox)wp.Children.FindByName(ValueControl)).Text = resultValue;*/
        }


        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            AddTerm();
        }

        private void AddTerm(int? termId = null, string termName = null, string termFunction = null, bool termUsed = false, int left_range =0, int right_range = 0, int ? comparable_num = 0)
        {
            editTermGroup.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            //var tb = new WrapPanel { Name = ParameterPanel };
            var term_title = new TextBlock { Name = TermTitle, MinWidth = 50 };
            Run run = new Run();
            run.Text = "Терм: ";
            term_title.Foreground = new SolidColorBrush(Colors.Blue);
            term_title.Inlines.Add(run);
            Grid.SetRow(term_title, editTermGroup.RowDefinitions.Count - 1);
            Grid.SetColumn(term_title, 0);
            editTermGroup.Children.Add(term_title);

            var wp = new WrapPanel { Name = TermPanel };
            Grid.SetRow(wp, editTermGroup.RowDefinitions.Count - 1);
            Grid.SetColumn(wp, 1);
            editTermGroup.Children.Add(wp);

            var term_comparable = new TextBox { Name = ComparableNumText, MinWidth = 50, Text = comparable_num.ToString() };
            wp.Children.Add(term_comparable);
            
            var term_name = new TextBox { Name = TermText, MinWidth = 100, Text = termName };
            term_name.Tag = ((termId != null) ? termId : 0);
            wp.Children.Add(term_name);

            var term_func = new TextBox { Name = TermFunc, MinWidth = 200, Text = termFunction };
            wp.Children.Add(term_func);

            var term_left_quot = new TextBlock { Name = TermSymbols, VerticalAlignment = VerticalAlignment.Center, Text = "  [  " };
            wp.Children.Add(term_left_quot);

            var term_left_range = new TextBox { Name = TermLeftRange, MinWidth = 30, Text = left_range.ToString() };
            wp.Children.Add(term_left_range);

            var term_comma = new TextBlock { Name = TermSymbols, VerticalAlignment = VerticalAlignment.Center, Text = " ,  " };
            wp.Children.Add(term_comma);

            var term_rigth_range = new TextBox { Name = TermRightRange, MinWidth = 30, Text = right_range.ToString() };
            wp.Children.Add(term_rigth_range);

            var term_rigth_quot = new TextBlock { Name = TermSymbols, VerticalAlignment = VerticalAlignment.Center, Text = "  ]  " };
            wp.Children.Add(term_rigth_quot);

            
            
            var deleteBox = new Button { Height = 20, Width = 20, Margin = new Thickness(10, 0, 0, 0) , Name=btnDelete};
            deleteBox.Click += deleteBox_Click;
            deleteBox.Content = new Image { Source = Helpers.BitmapSourceFromBitmap(Properties.Resources.delete) };
            wp.Children.Add(deleteBox);

            var CheckBoxUsed = new CheckBox { IsChecked = termUsed, VerticalAlignment = VerticalAlignment.Center};
            CheckBoxUsed.IsEnabled = false;
            wp.Children.Add(CheckBoxUsed);
          
        }

        /*private void FillValueCombo(int? paramId, ComboBox value)
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
        }*/

        /*private void FillParamCombo(ComboBox parameter)
        {
            foreach (Parameter p in _parameters)
                parameter.Items.Add(new ComboBoxItem { Tag = p.ParamId, Content = p.ParamName + " (" + p.ParamType.GetStringValue() + ")" });
            parameter.SelectionChanged += ParameterSelectionChanged;
        }*/

        void deleteBox_Click(object sender, RoutedEventArgs e)
        {
            var index = Grid.GetRow((UIElement)((Button)sender).Parent);
            //if (editTermGroup.Children.FindByName(btnDelete).Count))
            
            for (int i = 0; i < editTermGroup.Children.Count; i++)
            {
                // Запрещаем удалять все термы для группы
                if (Grid.GetRow(editTermGroup.Children[i]) == index)
                    if (editTermGroup.Children.Count != TermCountRowsAlwaysExists + 2)
                        editTermGroup.Children.RemoveAt(i);
                    else
                        MessageBox.Show("Запрещено удалять все термы из группы.");
                else
                    i++;
            }
        }

        private string _previousText = String.Empty;

        /*void ValuesTextChanged(object sender, TextChangedEventArgs e)
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
        }*/

        private void ValueParamTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (((ComboBox)sender).SelectedItem as ComboBoxItem).Content.ToString();
            if (text == ParamType.PFuzzy.GetStringValue())
            {
                ButtonTermAdd.IsEnabled = true;
            }
            else
            {
                ButtonTermAdd.IsEnabled = false;
                for (int i = 0; i < editTermGroup.Children.Count; )
                {
                    if (Grid.GetRow(editTermGroup.Children[i]) > TermCountRowsAlwaysExists)
                        editTermGroup.Children.RemoveAt(i);
                    else
                        i++;
                }
           
            }
            /*if (_creditParameters.Exists(a => a.ParamName == text))
            {
                _previousText = text;
                return;
            }
            bool success = double.TryParse(text, out num);
            if (success & num >= 0)
                _previousText = text;
            else
                ((TextBox)sender).Text = _previousText;*/
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (_terms != null)
            {
                ConnectionManager.ExecuteNonQuery(String.Format(@"delete from term_group where group_id={0}", _group_id));
            }
            // Обязательность термов в нечетких  переменных
           /* if (((ParamTypeCombo.SelectedItem as ComboBoxItem).Content.ToString() == ParamType.PFuzzy.GetStringValue())
                && (editTermGroup.Children.FindByName(TermText) == null))
            { 
                //MessageBox.Show("!!!");
                //return;
            }*/

            /*    using (var command = new SQLiteCommand(ConnectionManager.Connection))
                {
                    if (groupId != 0)
                    {
                        command.CommandText = "insert into term (term_id, term_name, term_function, left_range, right_range, group_name, group_id) values (@term_id, @term_name, @term_function, @left_range, @right_range, @group_name, @group_id)";
                        command.Parameters.Add(new SQLiteParameter("@group_id", groupId));
                        command.Parameters.Add(new SQLiteParameter("@group_name", ParamNameBox.Text));
                        //var _valuecontrol = wp.Children.FindByName(ValueControl);
                        //((wp.Children.FindByName(ResultParamCombo) as ComboBox).SelectedItem as ComboBoxItem).Tag)
                        command.Parameters.Add(new SQLiteParameter("@param_type", (ParamTypeCombo.SelectedItem as ComboBoxItem).Content.ToString()));
                        command.Parameters.Add(new SQLiteParameter("@question", ParamQuestionBox.Text));
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        command.CommandText = "insert into param (param_name, param_type,question) values (@param_name, @param_type,@question); SELECT last_insert_rowid();";
                        command.Parameters.Add(new SQLiteParameter("@param_name", ParamNameBox.Text));
                        command.Parameters.Add(new SQLiteParameter("@param_type", (ParamTypeCombo.SelectedItem as ComboBoxItem).Content.ToString()));
                        command.Parameters.Add(new SQLiteParameter("@question", ParamQuestionBox.Text));
                        paramId = int.Parse(command.ExecuteScalar().ToString());
                        
                    }
                }
           */

            if (_group_id != 0)
            {
                // Редактируем группу
                using (var command = new SQLiteCommand(ConnectionManager.Connection))
                {
                    command.CommandText = "insert into term_group (group_id, group_name) values (@group_id, @group_name)";
                    command.Parameters.Add(new SQLiteParameter("@group_name", GroupNameBox.Text));
                    command.Parameters.Add(new SQLiteParameter("@group_id", int.Parse(GroupNameBox.Tag.ToString())));
                    command.ExecuteNonQuery();
                }

                foreach (UIElement uie in editTermGroup.Children)
                {
                    if (!(uie is WrapPanel)) continue;
                    var wp = uie as WrapPanel;
                    //Фильтрация по WrapPanel, в которых содержутся термы
                    if (wp.Name != TermPanel) continue;

                    using (var command = new SQLiteCommand(ConnectionManager.Connection))
                    {
                        int term_id = int.Parse(((TextBox)wp.Children.FindByName(TermText)).Tag.ToString());
                        if (term_id == 0)
                        {
                            command.CommandText = "insert into term (term_name, term_function, left_range, right_range, group_id, comparable_num) values (@term_name, @term_function, @left_range, @right_range, @group_id, @comparable_num)";
                            command.Parameters.Add(new SQLiteParameter("@group_id", _group_id));
                            command.Parameters.Add(new SQLiteParameter("@group_name", GroupNameBox.Text));
                            command.Parameters.Add(new SQLiteParameter("@term_name", ((TextBox)wp.Children.FindByName(TermText)).Text));
                            command.Parameters.Add(new SQLiteParameter("@term_function", ((TextBox)wp.Children.FindByName(TermFunc)).Text));
                            command.Parameters.Add(new SQLiteParameter("@left_range", int.Parse(((TextBox)wp.Children.FindByName(TermLeftRange)).Text)));
                            command.Parameters.Add(new SQLiteParameter("@right_range", int.Parse(((TextBox)wp.Children.FindByName(TermRightRange)).Text)));
                            command.Parameters.Add(new SQLiteParameter("@comparable_num", int.Parse(((TextBox)wp.Children.FindByName(ComparableNumText)).Text)));
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            command.CommandText = "insert into term (term_id, term_name, term_function, left_range, right_range, group_id, comparable_num) values (@term_id, @term_name, @term_function, @left_range, @right_range, @group_id, @comparable_num)";
                            command.Parameters.Add(new SQLiteParameter("@group_id", _group_id));
                            command.Parameters.Add(new SQLiteParameter("@term_id", term_id));
                            command.Parameters.Add(new SQLiteParameter("@group_name", GroupNameBox.Text));
                            command.Parameters.Add(new SQLiteParameter("@term_name", ((TextBox)wp.Children.FindByName(TermText)).Text));
                            command.Parameters.Add(new SQLiteParameter("@term_function", ((TextBox)wp.Children.FindByName(TermFunc)).Text));
                            command.Parameters.Add(new SQLiteParameter("@left_range", int.Parse(((TextBox)wp.Children.FindByName(TermLeftRange)).Text)));
                            command.Parameters.Add(new SQLiteParameter("@right_range", int.Parse(((TextBox)wp.Children.FindByName(TermRightRange)).Text)));
                            command.Parameters.Add(new SQLiteParameter("@comparable_num", int.Parse(((TextBox)wp.Children.FindByName(ComparableNumText)).Text)));
                            command.ExecuteNonQuery();
                        }
                    }
                }
              
            }
            else
            {
                // Добавляем группу 
                using (var command = new SQLiteCommand(ConnectionManager.Connection))
                {
                    command.CommandText = "insert into term_group (group_name) values (@group_name); SELECT last_insert_rowid();";
                    command.Parameters.Add(new SQLiteParameter("@group_name", GroupNameBox.Text));
                    _group_id = int.Parse(command.ExecuteScalar().ToString());
                }

                foreach (UIElement uie in editTermGroup.Children)
                {
                    if (!(uie is WrapPanel)) continue;
                    var wp = uie as WrapPanel;
                    //Фильтрация по WrapPanel, в которых содержутся термы
                    if (wp.Name != TermPanel) continue;

                    using (var command = new SQLiteCommand(ConnectionManager.Connection))
                    {
                        command.CommandText = "insert into term (term_name, term_function, left_range, right_range, group_id) values (@term_name, @term_function, @left_range, @right_range, @group_id)";
                        command.Parameters.Add(new SQLiteParameter("@group_id", _group_id));
                        command.Parameters.Add(new SQLiteParameter("@group_name", GroupNameBox.Text));
                        command.Parameters.Add(new SQLiteParameter("@term_name", ((TextBox)wp.Children.FindByName(TermText)).Text));
                        command.Parameters.Add(new SQLiteParameter("@term_function", ((TextBox)wp.Children.FindByName(TermFunc)).Text));
                        command.Parameters.Add(new SQLiteParameter("@left_range", int.Parse(((TextBox)wp.Children.FindByName(TermLeftRange)).Text)));
                        command.Parameters.Add(new SQLiteParameter("@right_range", int.Parse(((TextBox)wp.Children.FindByName(TermRightRange)).Text)));
                        command.ExecuteNonQuery();
                    }
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