﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<string> userk = new List<string>();
        private Socket client;
        public MainWindow()
        {
            InitializeComponent();
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.ConnectAsync(Dns.GetHostEntry(Dns.GetHostName()).AddressList[2], 8888);
            Resave();
        }
        async Task Resave()
        {
            while (true)
            {
                users.ItemsSource = null;
                userk.Clear();
                foreach (des r in JsonConvert.DeserializeObject<List<des>>(File.ReadAllText("..\\..\\..\\..\\..\\infa.json")))
                {
                    userk.Add(r.name);
                }
                users.ItemsSource = userk;
                byte[] bytes = new byte[1024];
                await client.ReceiveAsync(bytes, SocketFlags.None);
                string mess = Encoding.UTF8.GetString(bytes);
                messages.Items.Add(mess);
            }
        }
        async Task SendMessage(string message) // отправка сообщений
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(bytes, SocketFlags.None);
        }

        private void Button_Click(object sender, RoutedEventArgs e) // нажатие на кнопку
        {
            SendMessage(message_text.Text);
        }
    }
    class des
    {
        public string server_power;
        public string name;
    }
}
