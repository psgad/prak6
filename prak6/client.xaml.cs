using Microsoft.VisualBasic;
using Newtonsoft.Json;
using prak6;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace prak6
{
    public partial class client : Window
    {
        private Socket Client;
        bool isConnected = false;
        int CommType;
        bool frst = false;

        static string address;
        static string name;

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        Dictionary<Socket, string> users_ = new Dictionary<Socket, string>();

        public client(string Name, string ip_addr)
        {

            InitializeComponent();
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            CommType = 2;
            name = Name;
            address = ip_addr;
            Title = $"{name} - пользователь";
            open_logs.Visibility = Visibility.Hidden;

            cheked();
        }

        public client(string Name)
        {
            InitializeComponent();
            try
            {
                Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Client.Bind(new IPEndPoint(IPAddress.Any, 8000));
                Client.Listen(10);
            }
            catch { }
            CommType = 1;
            name = Name;
            Title = $"{name} - сервер";
            cheked();
        }


        private async void cheked()
        {
            if (CommType == 1)
                listen();
            else
                await recv();

        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (msg.Text.Length == 0)
                return;
            switch (CommType)
            {
                case 1:
                    if (users_.Count == 0)
                    {
                        messages.Items.Add("Здесь пусто");
                        return;
                    }
                    string info = $"[{DateTime.Now}] {name}: {msg.Text}";
                    messages.Items.Add(info);
                    foreach (var i in users_.Keys.ToList())
                    {
                        Sending(i, info);
                    }
                    break;
                case 2:
                    if (isConnected == false)
                    {
                        messages.Items.Add("Ошибка отправки, вы не подключены!");
                        return;
                    }
                    if (msg.Text == "/disconnect")
                        close();
                    else
                        Client.SendAsync(Encoding.UTF8.GetBytes(msg.Text), SocketFlags.None);
                    break;
            }
        }



        private async Task recv() // main function of client
        {
            try
            {
                await Client.ConnectAsync(address, 8000);
                isConnected = true;
                await Client.SendAsync(Encoding.UTF8.GetBytes(name), SocketFlags.None);
            }
            catch (SocketException)
            {
                MessageBox.Show("Сервер выключен!");
            }

            while (isConnected)
            {
                try
                {
                    var bt = new byte[100];
                    await Client.ReceiveAsync(bt, SocketFlags.None);
                    var list = new byte[100];
                    await Client.ReceiveAsync(list, SocketFlags.None);
                    users.ItemsSource = Deserialize(list);
                    messages.Items.Add($"{Encoding.UTF8.GetString(bt)}");
                }

                catch (SocketException)
                {
                    if (users.ItemsSource != null)
                    {
                        messages.Items.Add($"Клиент {users.Items[0]} Покинул сервер!");
                        await Client.DisconnectAsync(true);
                        users.ItemsSource = null;
                        users.Items.Clear();
                        isConnected = false;
                    }
                    else
                    {
                        messages.Items.Add("Сервер выключен");
                        users.Items.Clear();
                    }
                }
            }
        }
        private static List<string> Deserialize(byte[] source)
        {
            string t = "";
            foreach (var i in Encoding.UTF8.GetString(source))
            {
                if (i == '\0')
                    break;
                t += i;
            }
            return JsonConvert.DeserializeObject<List<string>>(t);

        }
        private async void disconnect_(object sender, EventArgs e)
        {
            if (CommType == 1)
            {
                if (users_.Count == 0)
                {
                    Hide();
                    new MainWindow().Show();
                }
                else
                {
                    foreach (var sock in users_.Keys.ToList())
                    {
                        await Sending(sock, $"Сервер {name} покинул нас!");
                    }
                    Client.Close();
                    users_.Clear();
                    Hide();
                    new MainWindow().Show();
                }
            }
            else
            {
                if (!Client.Connected)
                    return;
                close();
            }
        }
        private async Task listen()
        {
            while (true)
            {
                var socket = await Client.AcceptAsync();
                var client_name = new byte[100];
                await socket.ReceiveAsync(client_name, SocketFlags.None);
                if (frst == false)
                {
                    users_.Add(socket, GetName(client_name));
                    await socket.SendAsync(Encoding.UTF8.GetBytes($"Соединение установлено, приятного общения!"), SocketFlags.None);
                    Thread.Sleep(500);
                    await socket.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(users_.Values.ToList(), SerializerSettings)), SocketFlags.None);
                    frst = true;
                }
                else
                {
                    users_.Add(socket, GetName(client_name));
                    string ms = $"пользователь {users_[socket]} подключился!";
                    foreach (var i in users_.Keys.ToList())
                    {
                        await i.SendAsync(Encoding.UTF8.GetBytes(ms), SocketFlags.None);
                        Thread.Sleep(500);
                        await i.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(users_.Values.ToList(), SerializerSettings)), SocketFlags.None);
                    }
                    users.ItemsSource = users_.Values.ToList();
                }

                messages.Items.Add($"пользователь {users_[socket]} подключился!");
                users.ItemsSource = users_.Values.ToList();
                receive(socket);
            }
        }
        private async Task receive(Socket client_)
        {
            while (true)
            {
                if (!IsConnected(client_))
                {
                    messages.Items.Add($"[{DateTime.Now}] Пользователь {users_[client_]} отлючился!");
                    await client_.DisconnectAsync(true);
                    users_.Remove(client_);
                    users.ItemsSource = users_.Values.ToList();
                    break;
                }
                var bt = new byte[100];
                await client_.ReceiveAsync(bt, SocketFlags.None);
                string msg = "";
                foreach (var i in Encoding.UTF8.GetString(bt))
                {
                    if (i == '\0')
                        break;
                    msg += i;
                }
                if (Equals(msg, "/disconnect"))
                {
                    messages.Items.Add($"[{DateTime.Now}]: пользователь {users_[client_]} отключился!");
                    users_.Remove(client_);
                    users.ItemsSource = users_.Values.ToList();
                    foreach (var sock in users_.Keys.ToList())
                    {
                        await sock.SendAsync(Encoding.UTF8.GetBytes($"[{DateTime.Now}]: пользователь {users_[client_]} отключился!"), SocketFlags.None);
                        Thread.Sleep(500);
                        await sock.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(users_.Values.ToList(), SerializerSettings)), SocketFlags.None);
                    }
                }
                else
                {
                    foreach (var Iter in users_.Keys.ToList())
                    {
                        await Iter.SendAsync(Encoding.UTF8.GetBytes($"[{DateTime.Now}] {users_[client_]} : {msg}"), SocketFlags.None);
                        Thread.Sleep(500);
                        await Iter.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(users_.Values.ToList(), SerializerSettings)), SocketFlags.None);
                    }
                    messages.Items.Add($"[{DateTime.Now}] {users_[client_]} : {msg}");
                }
                if (!IsConnected(client_))
                {
                    users_.Remove(client_);
                    foreach (var Iter in users_.Keys.ToList())
                    {
                        await Iter.SendAsync(Encoding.UTF8.GetBytes($"[{DateTime.Now}] пользователь {users_[client_]} отключился!"), SocketFlags.None);
                        Thread.Sleep(500);
                        await Iter.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(users_.Values.ToList(), SerializerSettings)), SocketFlags.None);
                    }
                    await client_.DisconnectAsync(true);
                    messages.Items.Add($"[{DateTime.Now}] пользователь {users_[client_]} отключился!");
                    users.ItemsSource = users_.Values.ToList();
                    break;
                }
            }
        }

        private string GetName(byte[] dirty)
        {
            string msg = "";
            foreach (var a in Encoding.UTF8.GetChars(dirty))
            {
                if (a == '\0')
                    break;
                msg += a;
            }
            return msg;
        }
        private async Task Sending(Socket s, string msg)
        {
            var bt = Encoding.UTF8.GetBytes(msg);
            await s.SendAsync(bt, SocketFlags.None);
            Thread.Sleep(500);
            await s.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(users_.Values.ToList(), SerializerSettings)), SocketFlags.None);
        }

        private bool IsConnected(Socket i)
        {
            bool part1 = i.Poll(1000, SelectMode.SelectRead);
            bool part2 = (i.Available == 0);
            if (part1 && part2 && !i.Connected)
                return false;
            else
                return true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            close();
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            close();
        }
        async void close()
        {
            await Client.SendAsync(Encoding.UTF8.GetBytes("/disconnect"), SocketFlags.None);
            await Client.DisconnectAsync(true);
            users.ItemsSource = null;
            isConnected = false;
            Close();
            new MainWindow().Show();
        }
    }
}