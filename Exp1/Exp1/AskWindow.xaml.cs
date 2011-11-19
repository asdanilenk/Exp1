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
        public object Answer 
        {
            get 
            { 
                UIElement uie = main.Children.FindByName(valuecontrol);
                if (uie is TextBox)
                {
                    if ((string)(uie as TextBox).Tag == "int")
                        return int.Parse((uie as TextBox).Text);
                    else
                        return (uie as TextBox).Text;
                }
                else if (uie is ComboBox)
                    return bool.Parse((uie as ComboBox).Text);
                return null;
            }
        }

        public string valuecontrol = "valuecontrol";

        public AskWindow(param par)
        {
            InitializeComponent();
            Question.Text = par.question;
            if (par.param_type == Param_type.p_bool)
            {
                ComboBox value = new ComboBox();
                value.Width = 100;
                value.Height = 20;
                value.Name = valuecontrol;
                value.Margin = new Thickness(5, 0, 0, 0);
                Grid.SetRow(value, 1);
                main.Children.Add(value);
                value.Items.Add("");
                value.Items.Add("true");
                value.Items.Add("false");
            }
            else
            {
                TextBox value = new TextBox();
                value.Width = 400;
                value.Name = valuecontrol;
                value.Margin = new Thickness(5, 0, 0, 0);
                Grid.SetRow(value, 1);
                main.Children.Add(value);
                if (par.param_type == Param_type.p_int)
                {
                    value.TextChanged += new TextChangedEventHandler(value_TextChanged);
                    value.Tag = "int";
                }
                else
                    value.Tag = "string";
            }
            
        }


        private string previousText = String.Empty;
        void value_TextChanged(object sender, TextChangedEventArgs e)
        {
            int num = 0;
            string text = ((TextBox)sender).Text;
            bool success = int.TryParse(text, out num);
            if (success & num >= 0)
                previousText = text;
            else
                ((TextBox)sender).Text = previousText;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

    }
}
