using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static List<input> us = new List<input>();
        public static List<string> userk = new List<string>();
        private Socket server;
        private Socket admin;
        private List<Socket> clients = new List<Socket>();
        public MainWindow()
        {
            InitializeComponent();
            if (!File.Exists(pyt("mogu.txt")))
            {
                File.WriteAllText(pyt("mogu.txt"), "0");
            }
            if (File.ReadAllText(pyt("mogu.txt")) == "0")
            {
                Process.Start(new ProcessStartInfo { FileName = pyt("auth\\auth\\bin\\Debug\\net6.0-windows\\auth.exe"), UseShellExecute = true });
                Environment.Exit(0);
            }
            userk.Clear();
            foreach (des r in JsonConvert.DeserializeObject<List<des>>(File.ReadAllText(pyt("infa.json"))))
            {
                userk.Add(r.name);
            }
            users.ItemsSource = userk;
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, 8888));
            server.Listen(2);
            ListenToClient();
            admins();
            //           MessageBox.Show($"IP, к которому будут подключаться люди, для общения: {Dns.GetHostEntry(Dns.GetHostName()).AddressList[2]}");
        }
        async Task admins()
        {
            admin = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            admin.ConnectAsync(Dns.GetHostEntry(Dns.GetHostName()).AddressList[2], 8888);
            var client = await server.AcceptAsync();
            clients.Add(client);
        }
        async Task ListenToClient()
        {
            while (true)
            {
                var client = await server.AcceptAsync();
                clients.Add(client);
                users.ItemsSource = null;
                userk.Clear();
                foreach (des r in JsonConvert.DeserializeObject<List<des>>(File.ReadAllText(pyt("infa.json"))))
                {
                    userk.Add(r.name);
                }
                users.ItemsSource = userk;

                Resave(client);
            }
        }
        async Task Resave(Socket client)
        {
            while (true)
            {
                byte[] bytes = new byte[1024];
                await client.ReceiveAsync(bytes, SocketFlags.None);
                string mess = Encoding.UTF8.GetString(bytes);
                for (int i = 0; i < clients.Count; i++)
                {
                    if (client.RemoteEndPoint == clients[i].RemoteEndPoint)
                        messages.Items.Add($"[{DateTime.Now}] [{userk[i]/*client.RemoteEndPoint*/}]: {mess}");
                }
                foreach (var item in clients)
                {
                    SendMessage(item, mess);
                }

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(admin, message_text.Text);
        }
        async Task SendMessage(Socket clientik, string message)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clientik.RemoteEndPoint == clients[i].RemoteEndPoint)
                {
                    string clean = "";
                    for (int j = 0; j < message.Length; j++)
                    {
                        clean += message[j];
                        if (message[j + 1] == '\0')
                        {
                            break;
                        }
                    }
                    us.Add(new input(userk[i], clientik.RemoteEndPoint.ToString(), clean, DateTime.Now));
                    File.WriteAllText(pyt("logi.json"), JsonConvert.SerializeObject(us));
                }
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await clientik.SendAsync(bytes, SocketFlags.None);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            close();
        }
        void close()
        {
            write(pyt("infa.json"));
            write(pyt("logi.json"));
            Close();
            Process.Start(new ProcessStartInfo { FileName = pyt("\\auth\\auth\\bin\\Debug\\net6.0-windows\\auth.exe"), UseShellExecute = true });
        }
        string pyt(string paths)
        {
            return "..\\..\\..\\..\\..\\" + paths;
        }
        void write(string paths)
        {
            File.WriteAllText(paths, "");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = pyt("logi.json"), UseShellExecute = true });
        }
    }
    class des
    {
        public string server_power;
        public string name;
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
}
