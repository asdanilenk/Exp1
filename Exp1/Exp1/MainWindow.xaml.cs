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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Exp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        List<Credit> credits;
        private const string creditname = "creditname";
        private const string savebutton = "savebutton";
        private const string cancelbutton = "cancelbutton";
        
        public MainWindow()
        {
            InitializeComponent();
            
            ConnectionManager.Filename = "data.db";
            BuildTable();
        }

        private void BuildTable()
        {
            credits = Helpers.ReadCreditsList();
            foreach (Credit c in credits)
            {
                Credits.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                WrapPanel wp = new WrapPanel();
                Grid.SetRow(wp, Credits.RowDefinitions.Count - 1);
                Credits.Children.Add(wp);
                TextBlock creditName = new TextBlock() { Name = creditname, Text = c.Name, Tag = c.Id, MinWidth=100 };
                creditName.MouseLeftButtonUp += new MouseButtonEventHandler(creditName_MouseLeftButtonUp);
                wp.Children.Add(creditName);
                Image editImage = new Image() { Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.edit) };
                Button editBox = new Button() { Height = 20, Width = 20, Margin = new Thickness(5, 0, 0, 0), Tag = c.Id, Content = editImage };
                Image deleteImage = new Image() { Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.delete) };
                Button deleteBox = new Button() { Height = 20, Width = 20, Margin = new Thickness(5, 0, 0, 0), Tag = c.Id, Content = deleteImage };
                deleteBox.Click += new RoutedEventHandler(deleteBox_Click);
                Button tryBox = new Button() { Height = 20, Content = "Запустить", Margin = new Thickness(5, 0, 0, 0), Tag = c.Id };
                tryBox.Click += new RoutedEventHandler(tryBox_Click);
                editBox.Click += new RoutedEventHandler(editBox_Click);
                wp.Children.Add(editBox);
                wp.Children.Add(deleteBox);
                wp.Children.Add(tryBox);
            }
        }

        void deleteBox_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены что хотите удалить данный кредит?", "Удалить?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                ConnectionManager.ExecuteNonQuery(String.Format("delete from credit where credit_id={0}", (sender as Button).Tag));
                UpdateTable();
            }
        }
        

        void creditName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock crN = sender as TextBlock;
            string text  = crN.Text;

            WrapPanel wp = crN.Parent as WrapPanel;
            TextBox credit = new TextBox() { Name = creditname, Text = text, Tag = crN.Tag, MinWidth=100 };
            credit.LostFocus += new RoutedEventHandler(credit_LostFocus);
            credit.Focus();
            
            
            wp.Children.Remove(crN);
            wp.Children.Insert(0, credit);
        }

        void credit_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox crN = sender as TextBox;
            string text = crN.Text;

            WrapPanel wp = crN.Parent as WrapPanel;
            TextBlock credit = new TextBlock() { Name = creditname, Text = text, Tag = crN.Tag, MinWidth = 100 };
            credit.MouseLeftButtonUp += new MouseButtonEventHandler(creditName_MouseLeftButtonUp);

            ConnectionManager.ExecuteNonQuery("update credit set credit_name='" + text + "' where credit_id=" + (int)credit.Tag);

            wp.Children.Remove(crN);
            wp.Children.Insert(0, credit);
        }

        void tryBox_Click(object sender, RoutedEventArgs e)
        {
            int cr_id =(int)(sender as Button).Tag;
            ReverseChain rs = new ReverseChain(cr_id);
        }

        void editBox_Click(object sender, RoutedEventArgs e)
        {
            (new CreditEditWindow((int)(sender as Button).Tag)).ShowDialog();
        }

        private void rulesButton_Click(object sender, RoutedEventArgs e)
        {
            (new RulesManagementWindow()).ShowDialog();
        }

        private void AddCredit_Click(object sender, RoutedEventArgs e)
        {
            Credits.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            WrapPanel wp = new WrapPanel();
            Grid.SetRow(wp, Credits.RowDefinitions.Count - 1);
            Credits.Children.Add(wp);

            TextBox nameBox = new TextBox();
            nameBox.Name = creditname;
            nameBox.MinWidth = 170;
            wp.Children.Add(nameBox);
            
            Button saveButton = new Button();
            saveButton.Name = savebutton;
            saveButton.Height = 20;
            saveButton.Width = 20;
            saveButton.Margin = new Thickness(10, 0, 0, 0);
            saveButton.Click += new RoutedEventHandler(saveButton_Click);
            Image okImage = new Image();
            okImage.Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.ok);
            saveButton.Content = okImage;
            wp.Children.Add(saveButton);

            Button cancelButton = new Button();
            cancelButton.Name = cancelbutton;
            cancelButton.Height = 20;
            cancelButton.Width = 20;
            cancelButton.Margin = new Thickness(10, 0, 0, 0);
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

        private void UpdateTable()
        {
            Credits.RowDefinitions.Clear();
            Credits.Children.Clear();
            BuildTable();
        }
        

        void saveButton_Click(object sender, RoutedEventArgs e)
        {
            WrapPanel wp = (sender as Button).Parent as WrapPanel;
            string credit_name = (wp.Children.FindByName(creditname) as TextBox).Text;
            ConnectionManager.ExecuteNonQuery(String.Format("insert into credit (credit_name) values ('{0}')", credit_name));

            UpdateTable();
        }
    }
}
