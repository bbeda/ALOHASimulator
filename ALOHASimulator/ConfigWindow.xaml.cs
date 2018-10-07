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
using ALOHASimulator.Model;

namespace ALOHASimulator
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {        
        public ConfigWindow(Configuration config)
        {
            InitializeComponent();
            this.DataContext = config;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }        
    }
}
