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

    public class Message
    {
        public string name;
        public DateTime time;
        public string content;
        public int type;

        public Message(string nm, DateTime dt, string content, int type)
        {
            name = nm;
            time = dt;
            this.content = content;
            this.type = type;
        }

    }

    public static class Serl
    {
        public static readonly string PATH = $"{Environment.GetEnvironmentVariable("USERPROFILE")}\\log.txt";
        public static void write(List<string> list)
        {
            File.WriteAllLines(PATH, list);
        }
    }

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

        public static List<string> logs = new List<string>();

        private readonly string ignore_msg = "$@-@$";

        private List<string> ign = new List<string>();
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
            {
                if (File.Exists(Serl.PATH))
                {
                    string line;
                    var reader = new StreamReader(Serl.PATH);
                    while ((line = reader.ReadLine()) != null)
                    {
                        logs.Add(line);
                    }
                    reader.Close();
                }
                listen();

            }
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

                    if (msg.Text == "/disconnect")
                    {
                        messages.Items.Add("Мы не можем вас отключить!");
                        return;
                    }

                    string info = $"[{DateTime.Now}] {name}: {msg.Text}";



                    messages.Items.Add(info);
                    foreach (var i in users_.Keys.ToList())
                    {
                        Sending(i, info);
                    }
                    logs.Add($"[{DateTime.Now}] сообщение от {name}: {msg.Text}");
                    Serl.write(logs);
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
                    {
                        Client.SendAsync(Encoding.UTF8.GetBytes(msg.Text), SocketFlags.None);
                    }

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
                    string message = Encoding.UTF8.GetString(bt);

                    string final = GetString(bt);


                    if (final == ignore_msg)
                    {
                        ign.Add(final);
                    }
                    else
                    {
                        messages.Items.Add(final);
                    }

                    ign.Clear();


                }

                catch (SocketException)
                {
                    if (users.ItemsSource != null)
                    {
                        messages.Items.Add($"Клиент {users.Items[0]} покинул сервер!");
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
        private async void disconnect_(object sender, EventArgs e) // событие закрытия
        {
            if (CommType == 1)
            {
                if (users_.Count == 0)
                {
                    Client.Dispose();
                    Client.Close();
                    new MainWindow().Show();
                }
                else
                {
                    foreach (var sock in users_.Keys.ToList())
                    {
                        await Sending(sock, $"Сервер {name} покинул нас!");
                    }

                    logs.Add($"[{DateTime.Now}]: {name} отключился");
                    Serl.write(logs);

                    Client.Dispose();
                    Client.Close();
                    users_.Clear();
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
                    string ms = $"Пользователь {users_[socket]} подключился!";
                    foreach (var i in users_.Keys.ToList())
                    {
                        await i.SendAsync(Encoding.UTF8.GetBytes(ignore_msg), SocketFlags.None);
                        Thread.Sleep(500);
                        await i.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(users_.Values.ToList(), SerializerSettings)), SocketFlags.None);
                    }
                    users.ItemsSource = users_.Values.ToList();
                }

                logs.Add($"[{DateTime.Now}]: {users_[socket]} подключился");
                Serl.write(logs);

                messages.Items.Add($"Пользователь {users_[socket]} подключился!");
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
                    messages.Items.Add($"[{DateTime.Now}]: Пользователь {users_[client_]} отключился!");
                    logs.Add($"[{DateTime.Now}]: {users_[client_]} отключился");
                    Serl.write(logs);

                    string tmp_name = users_[client_];
                    await client_.DisconnectAsync(true);
                    users_.Remove(client_);

                    foreach (var sock in users_.Keys.ToList())
                    {
                        await sock.SendAsync(Encoding.UTF8.GetBytes(ignore_msg), SocketFlags.None);
                        Thread.Sleep(500);
                        await sock.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(users_.Values.ToList(), SerializerSettings)), SocketFlags.None);
                    }


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
                    messages.Items.Add($"[{DateTime.Now}]: Пользователь {users_[client_]} отключился!");
                    logs.Add($"[{DateTime.Now}]: {users_[client_]} отключился");
                    Serl.write(logs);

                    string tmp_name = users_[client_];
                    await client_.DisconnectAsync(true);
                    users_.Remove(client_);

                    foreach (var sock in users_.Keys.ToList())
                    {
                        await sock.SendAsync(Encoding.UTF8.GetBytes(ignore_msg), SocketFlags.None);
                        Thread.Sleep(500);
                        await sock.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(users_.Values.ToList(), SerializerSettings)), SocketFlags.None);
                    }


                    users.ItemsSource = users_.Values.ToList();
                    break;
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

                    logs.Add($"[{DateTime.Now}] {users_[client_]}: {msg}");
                    Serl.write(logs);



                }
                if (!IsConnected(client_))
                {
                    messages.Items.Add($"[{DateTime.Now}]: Пользователь {users_[client_]} отключился!");
                    logs.Add($"[{DateTime.Now}]: {users_[client_]} отключился");
                    Serl.write(logs);

                    string tmp_name = users_[client_];
                    await client_.DisconnectAsync(true);
                    users_.Remove(client_);

                    foreach (var sock in users_.Keys.ToList())
                    {
                        await sock.SendAsync(Encoding.UTF8.GetBytes(ignore_msg), SocketFlags.None);
                        Thread.Sleep(500);
                        await sock.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(users_.Values.ToList(), SerializerSettings)), SocketFlags.None);
                    }


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
        async void close()
        {
            if (CommType == 1)
            {
                foreach (var sock in users_.Keys.ToList())
                {
                    await Sending(sock, $"Сервер {name} покинул нас!");
                }


                users_.Clear();
                Client.Dispose();
                Client.Close();
                users.ItemsSource = null;
                Thread.Sleep(500);
                Close();
            }
            else
            {
                await Client.SendAsync(Encoding.UTF8.GetBytes("/disconnect"), SocketFlags.None);
                await Client.DisconnectAsync(true);
                users.ItemsSource = null;
                isConnected = false;
                Client.Dispose();
                Client.Close();
                Thread.Sleep(500);
                Close();
            }
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            log_window log = new log_window();
            log.Show();
        }
        /// <summary>
        /// Gets string without garbage
        /// </summary>
        /// <returns></returns>
        private string GetString(byte[] dirty)
        {
            int i = 0;
            string str = null;
            char[] some = Encoding.UTF8.GetChars(dirty);

            while (some[i] != '\0')
            {
                str += some[i];
                i++;
            }

            return str;
        }
    }
}