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
    /// Interaction logic for CreditEditWindow.xaml
    /// </summary>
    public partial class CreditEditWindow : Window
    {
        private const string parameterpanel = "parameterpanel";
        private const string parambox = "parambox";
        private const string valuecontrol = "valuecontrol";

        Dictionary<creditparam, string> paramvalues;
        List<creditparam> pars;
        int credit_id;

        public CreditEditWindow(int credit_id)
        {
            InitializeComponent();
            this.credit_id = credit_id;
            this.Title = "Редактирование кредита";
            paramvalues = Helpers.ReadCreditParams(credit_id);
            pars = Helpers.ReadCreditParametersList();

//KeyValuePair<param, string> par in paramvalues
            foreach (creditparam p in pars)
            {
                RowDefinition row = new RowDefinition();
                row.Height = GridLength.Auto;
                Params.RowDefinitions.Add(row);

                WrapPanel wp = new WrapPanel();
                wp.Name = parameterpanel;
                Grid.SetRow(wp, Params.RowDefinitions.Count - 1);
                Grid.SetColumn(wp, 0);
                Params.Children.Add(wp);

                TextBlock parameter = new TextBlock();
                parameter.Name = parambox;
                parameter.Width = 200;
                parameter.Text = p.param_name + " (" + p.param_type + ") =";
                parameter.Tag = p.param_id;
                wp.Children.Add(parameter);

                if (p.param_type == Param_type.p_bool)
                {
                    ComboBox value = new ComboBox();
                    value.Width = 200;
                    value.Name = valuecontrol;
                    value.Margin = new Thickness(5, 0, 0, 0);
                    wp.Children.Add(value);
                    value.Items.Add("");
                    value.Items.Add("true");
                    value.Items.Add("false");
                    if (paramvalues.Keys.ToList().Find(pp => pp.param_id == p.param_id) != null)
                        value.Text = paramvalues.First(pp => pp.Key.param_id == p.param_id).Value;
                }
                else
                {
                    TextBox value = new TextBox();
                    value.Width = 200;
                    value.Name = valuecontrol;
                    value.Margin = new Thickness(5, 0, 0, 0);
                    wp.Children.Add(value);
                    if (paramvalues.Keys.ToList().Find(pp => pp.param_id == p.param_id) !=null)
                        value.Text = paramvalues.First(pp => pp.Key.param_id == p.param_id).Value;
                    if (p.param_type == Param_type.p_int)
                        value.TextChanged += new TextChangedEventHandler(value_TextChanged);
                }
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
            this.Close();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionManager.ExecuteNonQuery("delete from credit_param_value where credit_id=" + credit_id);
            foreach (UIElement uie in Params.Children)
                if (uie is WrapPanel)
                {
                    WrapPanel wp = uie as WrapPanel;
                    int paramid = (int)(wp.Children.FindByName(parambox) as TextBlock).Tag;
                    UIElement vcontrol = wp.Children.FindByName(valuecontrol);
                    string value = String.Empty;
                    if (vcontrol is ComboBox)
                        value = (vcontrol as ComboBox).Text;
                    else if (vcontrol is TextBox)
                        value = (vcontrol as TextBox).Text;
                    ConnectionManager.ExecuteNonQuery("insert into credit_param_value (credit_id,param_id,value) values (" + credit_id + "," + paramid + ",\'" + value + "\')");
                }
            this.Close();
        }
    }
}
