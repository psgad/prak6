using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
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
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        Socket server;
        public Window1()
        {
            InitializeComponent();
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, 8080));
            server.Listen(10);
            ListenClient();
        }
        async Task ListenClient()
        {
            while (true)
            {
                byte[] b = new byte[100];
                var client = await server.AcceptAsync();
                user.Sockets.Add(client);
                Resave(client);
            }
        }
        async Task Resave(Socket client)
        {

        }
    }
}
