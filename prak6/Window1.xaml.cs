using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace prak6
{

    public partial class Window1 : Window
    {

        enum MSG_TYPE
        {
            INFO = 1,
            MSG
        }



        int counter = 0;
        int msg_counter = 0;
        private readonly int MAX_CONN = 100;
        private readonly int MAX_MSG = 2000;
        private Socket sock;

        private readonly long MAX_LENGTH = 0x777;

        Dictionary<Socket, string> socket_user = new Dictionary<Socket, string>();
        List<Socket> users_sockets = new List<Socket>();

        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();



        static readonly string[] commands = { "/disconnect", "/close", "/list", "/hello" };
        public Window1()
        {
            InitializeComponent();
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 13000);
            sock.Bind(ip);

            sock.Listen(MAX_CONN);
            Listen();


        }

        private async Task Listen()
        {
            while (counter < MAX_CONN)
            {
                var Client = await sock.AcceptAsync();
                var client_name = new byte[100];
                string hello = "Hello from server!";
                var Name = new byte[20];
                Client.SendAsync(stob(hello), SocketFlags.None);
                await Client.ReceiveAsync(Name, SocketFlags.None);
                socket_user.Add(Client, btos(Name));
                messages.Items.Add($"[ {DateTime.Now} ] {MSG_TYPE.INFO} :: Client {socket_user[Client]} connected to server!");
                counter++;
                Update();
                Receive_From(Client);
            }


        }
        private void Update()
        {
            users.ItemsSource = socket_user.Values.ToList();
        }
        private void Broadcast_Send(object sender, RoutedEventArgs e)
        {
            users_sockets = socket_user.Keys.ToList();
            foreach (var i in users_sockets)
            {
                Sending(i, message_text.Text);
            }
            messages.Items.Add($"{DateTime.Now} {MSG_TYPE.INFO}: You: {message_text.Text}");
        }



        private async Task Receive_From(Socket cl)
        {
            while (true)
            {
                var bt = new byte[100];
                await cl.ReceiveAsync(bt, SocketFlags.None);
                var tb = Encoding.UTF8.GetBytes(socket_user[cl] + ' ');
                foreach (var i in socket_user.Keys.ToList())
                {
                    string msg = btos(bt);
                    messages.Items.Add($"[{DateTime.Now}] [{socket_user[cl]}] {msg}");
                    i.SendAsync(bt, SocketFlags.None);
                }
            }
        }


        private byte[] stob(string Taken_string) // from String to Bytes
        {
            return Encoding.UTF8.GetBytes(Taken_string);
        }

        private string btos(byte[] arr) // from Bytes to String
        {
            return Encoding.UTF8.GetString(arr);
        }

        private List<string> btol(byte[] arr)
        {
            return arr.Select(b_value => b_value.ToString()).ToList();
        }


        private async Task Sending(Socket s, string msg)
        {
            var bt = Encoding.UTF8.GetBytes(msg);
            await s.SendAsync(bt, SocketFlags.None);

        }

    }
}
