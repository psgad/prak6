using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;

namespace auth
{
    // <summary>
    // Interaction logic for MainWindow.xaml
    // <\\summary>
    public partial class MainWindow : Window
    {
        static List<user> users = new List<user>();
        public MainWindow()
        {
            InitializeComponent();
            if (!File.Exists("..\\..\\..\\..\\..\\infa.json"))
            {
                File.WriteAllText("..\\..\\..\\..\\..\\infa.json", "");
            }
            users = JsonConvert.DeserializeObject<List<user>>(File.ReadAllText("..\\..\\..\\..\\..\\infa.json")) ?? new List<user>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (tb.Text == "")
            {
                MessageBox.Show("Кто ты, воин?");
                return;
            }
            if (users.Count != 0)
            {

                if (users[0].server_power == "on")
                {
                    MessageBox.Show("Крч, ты опоздал, сервак уже создан ¯\\_(ツ)_/¯");
                    return;
                }
            }
            let_s_go();
            open("..\\..\\..\\..\\..\\WpfApp1\\WpfApp1\\bin\\Debug\\net6.0-windows\\WpfApp1.exe");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            users = JsonConvert.DeserializeObject<List<user>>(File.ReadAllText("..\\..\\..\\..\\..\\infa.json")) ?? new List<user>();
            if (IP.Text == "")
            {
                MessageBox.Show("А где суету, собственно, будем наводить???");
                return;
            }
            if (tb.Text == "")
            {
                MessageBox.Show("Кто ты, воин?");
                return;
            }
            if (JsonConvert.DeserializeObject<List<user>>(File.ReadAllText("..\\..\\..\\..\\..\\infa.json")) != null)
            {
                if (users.Count == 2)
                {
                    MessageBox.Show("Ну вот где ты раньше то был?? Сервак забит, сорян, попроси там кентов выйти, чтобы ты зашел ¯\\_(ツ)_/¯");  
                    return;
                }
            }
            else
            {
                MessageBox.Show("Сорян кнчн, но сервак вырублен ¯\\_(ツ)_/¯");
                return;
            }

            Regex reg = new Regex(@"^[1-9]{3}.[0-9].[0-9].[0-9]");
            if (!reg.IsMatch(IP.Text))
            {
                MessageBox.Show("Куда мы лезим... ¯\\_(ツ)_/¯ (Введи норм IP)");
                return;
            }
            let_s_go();
            open("..\\..\\..\\..\\..\\client\\client\\bin\\Debug\\net6.0-windows\\client.exe");
        }
        void open(string path)
        {
            File.WriteAllText("..\\..\\..\\..\\..\\mogu.txt", "1");
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            Close();
        }
        void let_s_go()
        {
            users.Add(new user("on", tb.Text));
            File.WriteAllText("..\\..\\..\\..\\..\\infa.json", JsonConvert.SerializeObject(users));
        }
    }
    class user
    {
        public string server_power;
        public string name;

        public user(string server_power, string name)
        {
            this.server_power = server_power;
            this.name = name;
        }
    }
}
