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

namespace Exp1
{
    /// <summary>
    /// Interaction logic for AskWindow.xaml
    /// </summary>
    public partial class AskWindow : Window
    {
        public object Ask(Parameter par)
        {
            var vlc = main.Children.FindByName(ValueControl);
            if (vlc != null)
                main.Children.Remove(vlc);
            Question.Text = "";
            foreach (string s in par.Question.Split(new[] { "\\n" }, StringSplitOptions.None))
            {
                Question.Inlines.Add(new Run { Text = s });
                Question.Inlines.Add(new LineBreak());
            }
            if (par.ParamType == ParamType.PBool)
            {
                var value = new ComboBox { Width = 100, Height = 20, Name = ValueControl, Margin = new Thickness(5, 0, 0, 0) };
                Grid.SetRow(value, 1);
                main.Children.Add(value);
                value.Items.Add(Boolean.TrueString);
                value.Items.Add(Boolean.FalseString);
                value.SelectedIndex = 0;
                value.Focus();
            }
            else
            {
                var value = new TextBox { Width = 400, Name = ValueControl, Margin = new Thickness(5, 0, 0, 0) };
                Grid.SetRow(value, 1);
                main.Children.Add(value);
                if (par.ParamType == ParamType.PDouble || par.ParamType == ParamType.PFuzzy)
                {
                    value.Text = "0";
                    value.TextChanged += ValueTextChanged;
                    value.Tag = par.ParamType;
                }
                else
                    value.Tag = ParamType.PString;
                value.SelectAll();
                value.Focus();
            }
            
            if (ShowDialog() == true)
            {

                UIElement uie = main.Children.FindByName(ValueControl);
                if (uie is TextBox)
                {
                    ParamType pt = (ParamType) (uie as TextBox).Tag;
                    if (pt == ParamType.PDouble || pt == ParamType.PFuzzy)
                        return double.Parse((uie as TextBox).Text);
                    return (uie as TextBox).Text;
                }
                if (uie is ComboBox)
                    return bool.Parse((uie as ComboBox).Text);
            }
            return null;
        }

        private const string ValueControl = "valuecontrol";

        public AskWindow()
        {
            InitializeComponent();
        }


        private string _previousText = String.Empty;
        void ValueTextChanged(object sender, TextChangedEventArgs e)
        {
            double num;
            string text = ((TextBox)sender).Text;
            bool success = double.TryParse(text, out num);
            if (success & num >= 0)
                _previousText = text;
            else
                ((TextBox)sender).Text = _previousText;
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

    }
}
