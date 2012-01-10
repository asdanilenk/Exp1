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
    public partial class TermManagementWindow : Window
    {
        List<TermGroup> groups;
        //private const string termname = "termname";
        private const string groupname = "groupname";
        //private const string paramedit = "paramedit";
        private const string groupdelete = "groupdelete";
        private const string groupedit = "groupedit";
        private const string numbox = "numbox";

        public TermManagementWindow()
        {
            InitializeComponent();
            UpdateTable();
        }

        private void UpdateTable()
        {
            termEdit.RowDefinitions.Clear();
            termEdit.Children.Clear();

            groups = Helpers.ReadTermGroupList();
            int counter = 0;
            foreach (TermGroup group in groups)
            {
                RowDefinition row = new RowDefinition();
                row.Height = GridLength.Auto;
                termEdit.RowDefinitions.Add(row);

                WrapPanel wp = new WrapPanel() { MinHeight = 20 };
                wp.Tag = group.TermGroupId;
                Grid.SetRow(wp, termEdit.RowDefinitions.Count - 1);
                termEdit.Children.Add(wp);

                TextBlock numBox = new TextBlock();
                numBox.Name = numbox;
                numBox.MinWidth = 20;
                //numBox.Text = (++counter).ToString();

                Run run = new Run();
                run.Text = (++counter).ToString();
                run.Foreground = new SolidColorBrush(Colors.Blue);
                numBox.Inlines.Add(run);

                wp.Children.Add(numBox);


                TextBlock nameBox = new TextBlock();
                nameBox.Name = groupname;
                
                nameBox.MinWidth = 200;
                nameBox.Text = group.TermGroupName;
                wp.Children.Add(nameBox);

                //AddInline(nameBox, (++counter).ToString() + ": IF", Colors.Blue);

                /*TextBlock typeBox = new TextBlock();
                typeBox.Name = paramtype;
                typeBox.MinWidth = 10;
                typeBox.Text = parameter.ParamType.GetStringValue();
                typeBox.Margin = new Thickness(5, 0, 0, 0);
                wp.Children.Add(typeBox);
                */
                Button editButton = new Button();
                editButton.Name = groupedit;
                editButton.Height = 20;
                editButton.Width = 20;
                editButton.Margin = new Thickness(5, 0, 0, 0);
                editButton.Tag = group.TermGroupId;
                editButton.Click += new RoutedEventHandler(editButton_Click);
                Image editImage = new Image();
                editImage.Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.edit);
                editButton.Content = editImage;
                wp.Children.Add(editButton);

                if (group.TermGroupUsed == false)
                {
                    Button deleteButton = new Button();
                    deleteButton.Name = groupdelete;
                    deleteButton.Height = 20;
                    deleteButton.Width = 20;
                    deleteButton.Margin = new Thickness(5, 0, 0, 0);
                    deleteButton.Tag = group.TermGroupId;
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
            TermGroup tg = groups.First(a => a.TermGroupId == tag);
            (new TermEditWindow(tg.TermGroupId,tg.TermGroupName)).ShowDialog();
            UpdateTable();

            /*WrapPanel wp = editButton.Parent as WrapPanel;
            TextBlock parname = wp.Children.FindByName(paramname) as TextBlock;
            string name = parname.Text;
            
            wp.Children.Remove(parname);
            TextBox nameBox = new TextBox();
            nameBox.Name = paramname;
            nameBox.Text = name;
            nameBox.MinWidth = 200;
            wp.Children.Insert(0, nameBox);

            if (!p.ParamUsed)
            {
                TextBlock partype = wp.Children.FindByName(paramtype) as TextBlock;
                string type = partype.Text;

                wp.Children.Remove(partype);

                ComboBox typeBox = new ComboBox();
                typeBox.Items.Add(new ComboBoxItem() { Content= ParamType.PString.GetStringValue() });
                typeBox.Items.Add(new ComboBoxItem() { Content = ParamType.PDouble.GetStringValue() });
                typeBox.Items.Add(new ComboBoxItem() { Content = ParamType.PBool.GetStringValue() });
                typeBox.SelectedIndex = 0;
                typeBox.Name = paramtype;
                typeBox.Text = type;
                typeBox.Width = 100;
                wp.Children.Insert(1, typeBox);
            }
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
             */
        }

        void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateTable();
        }

        /*void okButton_Click(object sender, RoutedEventArgs e)
        {
            Button okButton = sender as Button;
            int paramId = (int)okButton.Tag;
            WrapPanel wp = okButton.Parent as WrapPanel;

            CreditParameter p = parameters.First(a => a.ParamId == paramId);
            string query = String.Format(@"update credit_param set param_name='{0}'",(wp.Children.FindByName(paramname) as TextBox).Text);
            if (!p.ParamUsed)
                query += String.Format(@",param_type='{0}' ",((wp.Children.FindByName(paramtype) as ComboBox).SelectedItem as ComboBoxItem).Content.ToString());
            query += String.Format("where param_id={0}", paramId);
            ConnectionManager.ExecuteNonQuery(query);
            UpdateTable();
        }*/

       void addButton_Click(object sender, RoutedEventArgs e)
        {
           /* Button okButton = sender as Button;
            WrapPanel wp = okButton.Parent as WrapPanel;

            using (SQLiteCommand command = new SQLiteCommand(ConnectionManager.Connection))
            {
                command.CommandText = "insert into credit_param (param_name,param_type) values (@param_name,@param_type)";
                command.Parameters.Add(new SQLiteParameter("@param_name",(wp.Children.FindByName(paramname) as TextBox).Text));
                command.Parameters.Add(new SQLiteParameter("@param_type",((wp.Children.FindByName(paramtype) as ComboBox).SelectedItem as ComboBoxItem).Content.ToString()));
                command.ExecuteNonQuery();
            }
            UpdateTable();*/
            (new TermEditWindow()).ShowDialog();
            UpdateTable();
        }
    

        void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить данную группу термов?", "Удалить?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                ConnectionManager.ExecuteNonQuery(String.Format("delete from term_group where group_id={0}",(sender as Button).Tag));
                UpdateTable();
            }
        }

        private void addTermGroupButton_Click(object sender, RoutedEventArgs e)
        {
            (new TermEditWindow()).ShowDialog();
            UpdateTable();
            /*RowDefinition row = new RowDefinition();
            row.Height = GridLength.Auto;
            paramEdit.RowDefinitions.Add(row);

            WrapPanel wp = new WrapPanel();
            Grid.SetRow(wp, paramEdit.RowDefinitions.Count - 1);
            paramEdit.Children.Add(wp);

            TextBox nameBox = new TextBox();
            nameBox.Name = paramname;
            nameBox.Width = 200;
            wp.Children.Add(nameBox);

            ComboBox typeBox = new ComboBox();
            typeBox.Items.Add(new ComboBoxItem() { Content = ParamType.PString.GetStringValue() });
            typeBox.Items.Add(new ComboBoxItem() { Content = ParamType.PBool.GetStringValue() });
            typeBox.Items.Add(new ComboBoxItem() { Content = ParamType.PDouble.GetStringValue() });
            typeBox.Name = paramtype;
            typeBox.Width = 100;
            typeBox.SelectedIndex = 0;
            wp.Children.Add(typeBox);

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
            wp.Children.Add(cancelButton);*/
        }
    }
}
