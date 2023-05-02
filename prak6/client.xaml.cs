using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace prak6
{
    /// <summary>
    /// Логика взаимодействия для client.xaml
    /// </summary>
    public partial class client : Window
    {
        public static Socket Client;
        public client(string Name_client, string IP_client)
        {
            InitializeComponent();
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Client.ConnectAsync(IP_client, 13000);
            Resave(Name_client);/*
            update_list_users();*/
        }
        async Task Resave(string b)
        {
            Client.SendAsync(Encoding.UTF8.GetBytes(b), SocketFlags.None);
            while (true)
            {
                byte[] message = new byte[200];
                await Client.ReceiveAsync(message, SocketFlags.None);
                string msg = clear_string(Encoding.UTF8.GetString(message));
                if (msg == "/disconnect")
                {
                    exit();
                    return;
                }
                messages.Items.Add($"[{DateTime.Now}] : {msg}");
            }
            MessageBox.Show("Server closed!");
            exit();
        }
        string clear_string(string str)
        {
            string a = "";
            foreach (char b in str)
            {
                if (b == '\0')
                    break;
                a += b;
            }
            return a;
        }
        async Task update_list_users()
        {
            while (true)
            {
                Client.SendAsync(Encoding.UTF8.GetBytes("/list"), SocketFlags.None);
                byte[] count = new byte[3];
                byte[] user_list = new byte[100];
                await Client.ReceiveAsync(count, SocketFlags.None);
                try
                {
                    int counts = Convert.ToInt32(clear_string(Encoding.UTF8.GetString(count)));
                    users.Items.Clear();
                    for (int i = 0; i < counts * 2; i++)
                    {
                        await Client.ReceiveAsync(user_list, SocketFlags.None);
                        string name = clear_string(Encoding.UTF8.GetString(user_list));
                        users.Items.Add(name);
                    }
                }
                catch
                {
                    update_list_users();
                }
            }
        }
        async Task SendMessage(string message)
        {
            await Client.SendAsync(Encoding.UTF8.GetBytes(message), SocketFlags.None);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (message_text.Text == "")
                return;
            if (message_text.Text.Contains("/disconnect"))
            {
                exit();
                return;
            }
            SendMessage(message_text.Text);
        }
        async Task exit()
        {
            await Client.SendAsync(Encoding.UTF8.GetBytes("/disconnect"), SocketFlags.None);
            Close();
            new MainWindow().Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            exit();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            exit();
        }
    }
}
