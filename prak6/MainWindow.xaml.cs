using Newtonsoft.Json;
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

namespace prak6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<List<string>> names = new List<List<string>>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_name.Text == "")
            {
                MessageBox.Show("Кто ты, воин?");
                return;
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (TextBox_IP.Text == "")
            {
                MessageBox.Show("А где суету, собственно, будем наводить???");
                return;
            }
            if (TextBox_name.Text == "")
            {
                MessageBox.Show("Кто ты, воин?");
                return;
            }
            Hide();
            user.name = TextBox_name.Text;
            user.ip = TextBox_IP.Text;
            new client().Show();
        }
    }
}
