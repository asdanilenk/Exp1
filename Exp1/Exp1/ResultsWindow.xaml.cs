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
    /// Interaction logic for ResultsWindow.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        public ResultsWindow(string result, Logger logger)
        {
            InitializeComponent();
            this.ResultBlock.Text = result;
            foreach (string logentry in logger.entries)
            {
                logGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                TextBlock textBlock = new TextBlock() { Text = logentry };
                Grid.SetRow(textBlock, logGrid.RowDefinitions.Count - 1);
                logGrid.Children.Add(textBlock);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
