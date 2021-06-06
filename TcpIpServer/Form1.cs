using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpIpServer
{
    public partial class Form1 : Form
    {
        private IPAddress _iPAddress;
        private TcpListener tcpListener;
        private List<Socket> _socket;
        byte[] receivedMessage = new byte[2048];
        string encodedMessage = "";
        string serverName = "Server";
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                button2.Text = "Starting...";
                _iPAddress = IPAddress.Parse("127.0.0.1");
                tcpListener = new TcpListener(_iPAddress, 8888);
                tcpListener.Start();
                statusLbl.Text = "The server is running at port 8888...";
                button2.Enabled = false;
                _socket = new List<Socket>();
                new Thread(() =>
                {
                    while (true)
                    {
                        var client = tcpListener.AcceptSocket();
                        _socket.Add(client);
                        new Thread(() =>
                        {
                            this.GetMessage(client);
                        }).Start();
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                var dialogResult = MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel);
            }
        }

        private void GetMessage(Socket _client)
        {
            while (true)
            {
                var count = _client.Receive(receivedMessage);
                if (count >= 1)
                {
                    encodedMessage = Encoding.ASCII.GetString(receivedMessage, 0, count);
                    var arr = encodedMessage.Split('%');
                    arr[0] = $"Client {_socket.IndexOf(_client)}";
                    encodedMessage = arr[0] + '%' + arr[1];
                    foreach (var item in _socket.Where(e => e.Connected).ToList())
                    {
                        ASCIIEncoding asen = new ASCIIEncoding();
                        if (item != _client)
                             item.Send(asen.GetBytes($"Client {_socket.IndexOf(item)}" + '%' + encodedMessage.Split('%')[1]));
                    }
                    Msg();
                }
            }
        }

        private void Msg()
        {
            if (messageReadtxt.InvokeRequired)
            {
                messageReadtxt.BeginInvoke((Action)delegate ()
                {
                    messageReadtxt.Text += $"{encodedMessage.Split('%')[0]}-{DateTime.Now.ToShortTimeString()}: {encodedMessage.Split('%')[1].TrimStart()} {Environment.NewLine}";
                });
            }
            else
            {
                messageReadtxt.Text += $"{encodedMessage.Split('%')[0]}-{DateTime.Now.ToShortTimeString()}: {encodedMessage.Split('%')[1].TrimStart()} {Environment.NewLine}";
            }
            //ASCIIEncoding asen = new ASCIIEncoding();
            //foreach (var item in _socket.Where(e => e.Connected).ToList())
            //{
            //    item.Send(asen.GetBytes(serverName + '%' + msgTxt.Text));
            //    messageReadtxt.Text += $"{serverName}-{DateTime.Now.ToShortTimeString()}: {msgTxt.Text} {Environment.NewLine}";
            //    msgTxt.Text = "";
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ASCIIEncoding asen = new ASCIIEncoding();
            foreach(var item in _socket.Where(e => e.Connected).ToList())
            {
                item.Send(asen.GetBytes(serverName + '%' + msgTxt.Text));
            }
            messageReadtxt.Text += $"{serverName}-{DateTime.Now.ToShortTimeString()}: {msgTxt.Text} {Environment.NewLine}";
            msgTxt.Text = "";
        }
    }
}
