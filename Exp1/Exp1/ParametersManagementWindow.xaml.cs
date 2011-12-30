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
    /// Interaction logic for ManageParametersWindow.xaml
    /// </summary>
    public partial class ParametersManagementWindow : Window
    {
        List<Parameter> parameters;
        private const string paramname = "paramname";
        private const string paramtype = "paramtype";
        private const string paramedit = "paramedit";
        private const string paramdelete = "paramdelete";
        private const string paramquestion = "paramquestion";
        
        public ParametersManagementWindow()
        {
            InitializeComponent();
            UpdateTable();
        }

        private void UpdateTable()
        {
            paramEdit.RowDefinitions.Clear();
            paramEdit.Children.Clear();

            parameters = Helpers.ReadParametersList();
            foreach (Parameter parameter in parameters)
            {
                RowDefinition row = new RowDefinition();
                row.Height = GridLength.Auto;
                paramEdit.RowDefinitions.Add(row);

                WrapPanel wp = new WrapPanel() { MinHeight = 20 };
                wp.Tag = parameter.ParamId;
                Grid.SetRow(wp, paramEdit.RowDefinitions.Count - 1);
                paramEdit.Children.Add(wp);

                TextBlock nameBox = new TextBlock();
                nameBox.MinWidth = 200;
                nameBox.Name = paramname;
                nameBox.Text = parameter.ParamName;
                wp.Children.Add(nameBox);

                TextBlock typeBox = new TextBlock();
                typeBox.Name = paramtype;
                typeBox.MinWidth = 50;
                typeBox.Text = parameter.ParamType.GetStringValue();
                typeBox.Margin = new Thickness(5, 0, 0, 0);
                wp.Children.Add(typeBox);

                TextBlock questionBox = new TextBlock();
                questionBox.Name = paramquestion;
                questionBox.MinWidth = 300;
                questionBox.Text = parameter.Question;
                questionBox.Margin = new Thickness(5, 0, 0, 0);
                wp.Children.Add(questionBox);

                Button editButton = new Button();
                editButton.Name = paramedit;
                editButton.Height = 20;
                editButton.Width = 20;
                editButton.Margin = new Thickness(5, 0, 0, 0);
                editButton.Tag = parameter.ParamId;
                editButton.Click += new RoutedEventHandler(editButton_Click);
                Image editImage = new Image();
                editImage.Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.edit);
                editButton.Content = editImage;
                wp.Children.Add(editButton);

                if (parameter.ParamUsed == false)
                {
                    Button deleteButton = new Button();
                    deleteButton.Name = paramdelete;
                    deleteButton.Height = 20;
                    deleteButton.Width = 20;
                    deleteButton.Margin = new Thickness(5, 0, 0, 0);
                    deleteButton.Tag = parameter.ParamId;
                    deleteButton.Click += new RoutedEventHandler(deleteButton_Click);
                    Image deleteImage = new Image();
                    deleteImage.Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.delete);
                    deleteButton.Content = deleteImage;
                    wp.Children.Add(deleteButton);
                }
            }
        }

        void editButton_Click(object sender, RoutedEventArgs e)
        {
            Button editButton = sender as Button;
            int tag = (int)editButton.Tag;
            Parameter p = parameters.First(a => a.ParamId == tag);

            WrapPanel wp = editButton.Parent as WrapPanel;
            TextBlock parname = wp.Children.FindByName(paramname) as TextBlock;
            string name = parname.Text;
            
            wp.Children.Remove(parname);
            TextBox nameBox = new TextBox();
            nameBox.Name = paramname;
            nameBox.Text = name;
            nameBox.Width = 200;
            wp.Children.Insert(0, nameBox);

            if (!p.ParamUsed)
            {
                TextBlock partype = wp.Children.FindByName(paramtype) as TextBlock;
                string type = partype.Text;

                wp.Children.Remove(partype);

                ComboBox typeBox = new ComboBox();
                typeBox.Items.Add(new ComboBoxItem() { Content = ParamType.PString.GetStringValue() });
                typeBox.Items.Add(new ComboBoxItem() { Content = ParamType.PBool.GetStringValue() });
                typeBox.Items.Add(new ComboBoxItem() { Content = ParamType.PDouble.GetStringValue() });
                typeBox.SelectedIndex = 0;
                typeBox.Name = paramtype;
                typeBox.Text = type;
                typeBox.Width = 100;
                wp.Children.Insert(1, typeBox);
            }

            TextBlock parquestion = wp.Children.FindByName(paramquestion) as TextBlock;
            string quest = parquestion.Text;

            wp.Children.Remove(parquestion);
            TextBox questionBox = new TextBox();
            questionBox.Name = paramquestion;
            questionBox.Text = quest;
            questionBox.MinWidth = 300;
            wp.Children.Insert(2, questionBox);

            Button paredit = wp.Children.FindByName(paramedit) as Button;
            wp.Children.Remove(parname);

            Button pardelete = wp.Children.FindByName(paramdelete) as Button;
            if (pardelete != null)
                wp.Children.Remove(parname);

            foreach (UIElement uie in paramEdit.Children)
                if (uie is WrapPanel)
                {
                    WrapPanel _wp = uie as WrapPanel;
                    _wp.Children.Remove(_wp.Children.FindByName(paramedit));
                    if (_wp.Children.FindByName(paramdelete) != null)
                        _wp.Children.Remove(_wp.Children.FindByName(paramdelete));
                }

            Button okButton = new Button();
            okButton.Name = paramedit;
            okButton.Height = 20;
            okButton.Width = 20;
            okButton.Margin = new Thickness(5, 0, 0, 0);
            okButton.Tag = p.ParamId;
            okButton.Click += new RoutedEventHandler(okButton_Click);
            Image okImage = new Image();
            okImage.Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.ok);
            okButton.Content = okImage;
            wp.Children.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.Name = paramedit;
            cancelButton.Height = 20;
            cancelButton.Width = 20;
            cancelButton.Margin = new Thickness(5, 0, 0, 0);
            cancelButton.Tag = p.ParamId;
            cancelButton.Click += new RoutedEventHandler(cancelButton_Click);
            Image cancelImage = new Image();
            cancelImage.Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.cancel);
            cancelButton.Content = cancelImage;
            wp.Children.Add(cancelButton);
        }

        void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateTable();
        }

        void okButton_Click(object sender, RoutedEventArgs e)
        {
            Button okButton = sender as Button;
            int paramId = (int)okButton.Tag;
            WrapPanel wp = okButton.Parent as WrapPanel;

            Parameter p = parameters.First(a => a.ParamId == paramId);
            string query = String.Format(@"update param set param_name='{0}', question='{1}'",(wp.Children.FindByName(paramname) as TextBox).Text,(wp.Children.FindByName(paramquestion) as TextBox).Text);
            if (!p.ParamUsed)
                query += String.Format(@",param_type='{0}' ",((wp.Children.FindByName(paramtype) as ComboBox).SelectedItem as ComboBoxItem).Content.ToString());
            query += String.Format("where param_id={0}", paramId);
            ConnectionManager.ExecuteNonQuery(query);
            UpdateTable();
        }

        void addButton_Click(object sender, RoutedEventArgs e)
        {
            Button okButton = sender as Button;
            WrapPanel wp = okButton.Parent as WrapPanel;

            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.Connection))
            {
                command.CommandText = "insert into param (param_name,param_type,question) values (@param_name,@param_type,@question)";
                command.Parameters.Add(new SQLiteParameter("@param_name",(wp.Children.FindByName(paramname) as TextBox).Text));
                command.Parameters.Add(new SQLiteParameter("@param_type",((wp.Children.FindByName(paramtype) as ComboBox).SelectedItem as ComboBoxItem).Content.ToString()));
                command.Parameters.Add(new SQLiteParameter("@question", (wp.Children.FindByName(paramquestion) as TextBox).Text));
                command.ExecuteNonQuery();
            }
            UpdateTable();
        }

        void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены что хотите удалить данный параметр?", "Удалить?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                ConnectionManager.ExecuteNonQuery(String.Format("delete from param where param_id={0}",(sender as Button).Tag));
                UpdateTable();
            }
        }

        private void addParameterButton_Click(object sender, RoutedEventArgs e)
        {
            RowDefinition row = new RowDefinition();
            row.Height = GridLength.Auto;
            paramEdit.RowDefinitions.Add(row);

            WrapPanel wp = new WrapPanel();
            Grid.SetRow(wp, paramEdit.RowDefinitions.Count - 1);
            paramEdit.Children.Add(wp);

            TextBox nameBox = new TextBox();
            nameBox.Name = paramname;
            nameBox.MinWidth = 200;
            wp.Children.Add(nameBox);

            ComboBox typeBox = new ComboBox();
            typeBox.Items.Add(new ComboBoxItem() { Content = ParamType.PString.GetStringValue() });
            typeBox.Items.Add(new ComboBoxItem() { Content = ParamType.PBool.GetStringValue() });
            typeBox.Items.Add(new ComboBoxItem() { Content = ParamType.PDouble.GetStringValue() });
            typeBox.Name = paramtype;
            typeBox.Width = 100;
            typeBox.SelectedIndex = 0;
            wp.Children.Add(typeBox);

            TextBox questionBox = new TextBox();
            questionBox.Name = paramquestion;
            questionBox.MinWidth = 300;
            questionBox.Margin = new Thickness(5, 0, 0, 0);
            wp.Children.Add(questionBox);

            foreach (UIElement uie in paramEdit.Children)
                if (uie is WrapPanel)
                {
                    WrapPanel _wp = uie as WrapPanel;
                    _wp.Children.Remove(_wp.Children.FindByName(paramedit));
                    if (_wp.Children.FindByName(paramdelete) != null)
                        _wp.Children.Remove(_wp.Children.FindByName(paramdelete));
                }

            Button okButton = new Button();
            okButton.Name = paramedit;
            okButton.Height = 20;
            okButton.Width = 20;
            okButton.Margin = new Thickness(10, 0, 0, 0);
            okButton.Click +=new RoutedEventHandler(addButton_Click);
            Image okImage = new Image();
            okImage.Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.ok);
            okButton.Content = okImage;
            wp.Children.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.Name = paramedit;
            cancelButton.Height = 20;
            cancelButton.Width = 20;
            cancelButton.Margin = new Thickness(10, 0, 0, 0);
            cancelButton.Click += new RoutedEventHandler(cancelButton_Click);
            Image cancelImage = new Image();
            cancelImage.Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.cancel);
            cancelButton.Content = cancelImage;
            wp.Children.Add(cancelButton);
        }
    }
}
