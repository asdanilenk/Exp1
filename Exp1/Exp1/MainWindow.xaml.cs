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
            //FIXME
            ConnectionManager.filename = @"C:\Users\Артем\Desktop\Exp1\Exp1\data.db";
            credits = Helpers.ReadCreditsList();
            foreach (credit c in credits)
            {
                Credits.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                WrapPanel wp = new WrapPanel();
                Grid.SetRow(wp, Credits.RowDefinitions.Count - 1);
                Credits.Children.Add(wp);

                wp.Children.Add(new TextBlock() { Name = creditname, Text = c.name });
                Image editImage = new Image() { Source = Helpers.BitmapSourceFromBitmap(Exp1.Properties.Resources.edit)};
                Button editBox = new Button() { Height = 20, Width = 20, Margin = new Thickness(5, 0, 0, 0), Tag = c.id, Content = editImage };
                editBox.Click += new RoutedEventHandler(editBox_Click);
                wp.Children.Add(editBox);
            }
        }

        void editBox_Click(object sender, RoutedEventArgs e)
        {
            (new CreditEditWindow((int)(sender as Button).Tag)).ShowDialog();
        }

        private void rulesButton_Click(object sender, RoutedEventArgs e)
        {
            (new RulesManagementWindow()).ShowDialog();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            (new CreditParametersManagementWindow()).ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
