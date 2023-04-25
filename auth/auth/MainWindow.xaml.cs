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
using System.Net.Sockets;

namespace auth
{
    // <summary>
    // Interaction logic for MainWindow.xaml
    // <\\summary>
    public partial class MainWindow : Window
    {

        
        
        static List<User> users = new List<User>();
        public MainWindow()
        {
            InitializeComponent();
            if (!File.Exists("..\\..\\..\\..\\..\\infa.json"))
            {
                File.WriteAllText("..\\..\\..\\..\\..\\infa.json", "");
            }
            users = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText("..\\..\\..\\..\\..\\infa.json")) ?? new List<User>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (tb.Text.Length == 0)
            {
                MessageBox.Show("Пустое поле имени пользователя неприемлено");
                return;
            }
            
            if (users.Count != 0)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].name == tb.Text)
                    {
                        MessageBox.Show("Такой пользователь уже есть на сервере");
                        return;
                    }

                }
            }

            WriteData();
            Open("C:\\Users\\mansu\\prak6\\WpfApp1\\WpfApp1\\bin\\Debug\\net6.0-windows\\WpfApp1.exe");

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (tb.Text.Length == 0)
            {
                MessageBox.Show("Пустое поле имени пользователя неприемлено");
                return;
            }

            if (IP.Text.Length == 0)
            {
                MessageBox.Show("Адрес не может быть пустым");
                return;
            }

            if (users.Count != 0)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].name == tb.Text)
                    {
                        MessageBox.Show("Такой пользователь уже есть на сервере");
                        return;
                    }

                }
            }


            Regex reg = new Regex(@"^[1-9]{3}.[0-9].[0-9].[0-9]");

            if (!reg.IsMatch(IP.Text))
            {
                MessageBox.Show("Некорректный IP адрес");
                return;
            }

            else
            {
                WriteData();
                Open("..\\..\\..\\..\\..\\client\\client\\bin\\Debug\\net6.0-windows\\client.exe");
            }
            

            
        }
        void Open(string path)
        {
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            Close();
        }
        void WriteData()
        {
            users.Add(new User(true, tb.Text));
            File.WriteAllText("..\\..\\..\\..\\..\\infa.json", JsonConvert.SerializeObject(users));
        }
    }
    class User
    {
        public bool Is_On;
        public string name;

        public User(bool on, string name)
        {
            this.Is_On = on;
            this.name = name;
        }
    }
}
