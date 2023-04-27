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
        Socket client_;
        public client()
        {
            InitializeComponent();
            conect();
        }
        async Task conect()
        {
            Socket client_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client_.ConnectAsync(user.ip, 13000);
            MessageBox.Show(client_.RemoteEndPoint.ToString());
            byte[] name = Encoding.UTF8.GetBytes(user.name);
            client_.SendAsync(name, SocketFlags.None);
            byte[] bytes = new byte[100];
            client_.ReceiveAsync(bytes, SocketFlags.None);
            messages.Items.Add(return_string(bytes));

        }
        string return_string(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
        List<string> return_list(byte[] bytes)
        {
            return bytes.Select(byteValue => byteValue.ToString()).ToList();
        }
        public async Task ListenClient()
        {

        }
        async Task Resave()
        {
            byte[] message = new byte[100];
            await client_.ReceiveAsync(message, SocketFlags.None);
            string message_ = Encoding.UTF8.GetString(message);
            string clear_message = "";
            foreach (char ch in message_)
            {
                if (ch != '\0')
                    clear_message += ch;
            }
            messages.Items.Add(clear_message);
        }
        void SendMessage(Socket client_socket, string message)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
    class input
    {
        public string name;
        public string IP;
        public string message;
        public DateTime date;
        public input(string name, string iP, string message, DateTime date)
        {
            this.name = name;
            IP = iP;
            this.message = message;
            this.date = date;
        }
    }
    class user
    {
        public static string name;
        public static string ip;
        public static Socket client_sock;
        public static List<string> Names = new List<string>();
        public static List<Socket> Sockets = new List<Socket>();
    }

}
