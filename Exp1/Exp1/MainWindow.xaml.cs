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
        public MainWindow()
        {
            InitializeComponent();
            //FIXME
            ConnectionManager.filename = @"C:\Users\Артем\Desktop\Exp1\Exp1\data.db";
        }

        private void rulesButton_Click(object sender, RoutedEventArgs e)
        {
            (new Rules()).ShowDialog();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            (new ManageCreditParametersWindow()).ShowDialog();
        }
    }
}
