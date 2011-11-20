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
        
        List<credit> credits;
        private const string creditname = "creditname";
        public MainWindow()
        {
            InitializeComponent();
            
            ConnectionManager.filename = "data.db";
            credits = Helpers.ReadCreditsList();
            foreach (credit c in credits)
            {
                Credits.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                WrapPanel wp = new WrapPanel();
                Grid.SetRow(wp, Credits.RowDefinitions.Count - 1);
                Credits.Children.Add(wp);
                TextBlock creditName = new TextBlock() { Name = creditname, Text = c.name, Tag= c.id };
                creditName.MouseLeftButtonUp += new MouseButtonEventHandler(creditName_MouseLeftButtonUp);
                wp.Children.Add(creditName);
                Image editImage = new Image() { Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.edit)};
                Button editBox = new Button() { Height = 20, Width = 20, Margin = new Thickness(5, 0, 0, 0), Tag = c.id, Content = editImage };
                Button tryBox = new Button() { Height = 20, Content= "Запустить", Margin = new Thickness(5, 0, 0, 0), Tag = c.id};
                tryBox.Click += new RoutedEventHandler(tryBox_Click);
                editBox.Click += new RoutedEventHandler(editBox_Click);
                wp.Children.Add(editBox);
                wp.Children.Add(tryBox);
            }
        }

        void creditName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock crN = sender as TextBlock;
            string text  = crN.Text;

            WrapPanel wp = crN.Parent as WrapPanel;
            TextBox credit = new TextBox() { Name = creditname, Text = text, Tag = crN.Tag };
            credit.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(credit_LostKeyboardFocus);
            
            wp.Children.Remove(crN);
            wp.Children.Insert(0, credit);
        }

        void credit_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox crN = sender as TextBox;
            string text = crN.Text;

            WrapPanel wp = crN.Parent as WrapPanel;
            TextBlock credit = new TextBlock() { Name = creditname, Text = text, Tag = crN.Tag};
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

        }
    }
}
