using Newtonsoft.Json;
using prak6;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace prak6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (name.Text == "")
            {
                MessageBox.Show("Ошибка ввода имени");
                return;
            }
            Hide();
            new client(name.Text).Show();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (IP.Text == "")
            {
                MessageBox.Show("Не введен IP");
                return;
            }
            if (name.Text == "")
            {
                MessageBox.Show("Ошибка ввода имени");
                return;
            }
            Hide();
            new client(name.Text, IP.Text).Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
