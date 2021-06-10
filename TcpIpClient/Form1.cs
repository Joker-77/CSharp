using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpIpClient
{
    public partial class Form1 : Form
    {
        private TcpClient _tcpClient;
        private Stream _stream;
        private byte[] _receivedMsg;
        private string _encodedMsg;
        string clientName = "Client";
        private bool _connected;
        Thread getMessageThread;
        public Form1()
        {
            InitializeComponent();
            _receivedMsg = new byte[2048];
            button1.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                _tcpClient = new TcpClient();
                statusLbl.Text = "Connecting...";
                _tcpClient.Connect("127.0.0.1", 8888);
                textBox1.Text = $"Connnected to server: 127.0.0.1:8888";
                MessageBox.Show("Connected Successfuly", "Status", MessageBoxButtons.OKCancel);
                statusLbl.Text = "Connected...";
                button2.Enabled = false;
                _connected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel);
            }
            getMessageThread = new Thread(() =>
            {
                this.GetMessage();
            });
            getMessageThread.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string message = clientName + '%' + inputMsg.Text;
                if (_tcpClient != null)
                {
                    _stream = _tcpClient.GetStream();
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] encodedMsg = encoding.GetBytes(message);
                    statusLbl.Text = "Sending...";
                    _stream.Write(encodedMsg, 0, encodedMsg.Length);
                    messageReadtxt.Text += $"{DateTime.Now.ToShortTimeString()}: {inputMsg.Text} {Environment.NewLine}";
                    inputMsg.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Server is offline now", "Server Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
        }

        private void GetMessage()
        {
            while (_connected)
            {
                if (_tcpClient != null)
                {
                    _stream = _tcpClient.GetStream();
                    int bytesCount = _stream.Read(_receivedMsg, 0, 2048);
                    _encodedMsg = Encoding.ASCII.GetString(_receivedMsg, 0, bytesCount);
                    if (bytesCount >= 1)
                    {
                        Msg();
                    }
                }

            }
        }

        private void Msg()
        {
            if (messageReadtxt.InvokeRequired)
            {
                messageReadtxt.BeginInvoke((Action)delegate ()
                {
                    messageReadtxt.Text += $"{_encodedMsg.Split('%')[0].TrimStart()}-{DateTime.Now.ToShortTimeString()}: {_encodedMsg.Split('%')[1].TrimStart()} {Environment.NewLine}";
                });
            }
            else
            {
                messageReadtxt.Text += $"{_encodedMsg.Split('%')[0].TrimStart()}-{DateTime.Now.ToShortTimeString()}: {_encodedMsg.Split('%')[1].TrimStart()} {Environment.NewLine}";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void inputMsg_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(inputMsg.Text) || string.IsNullOrWhiteSpace(inputMsg.Text))
                button1.Enabled = false;
            else
                button1.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _connected = false;
            textBox1.Text = $"";
            button2.Text = "Connect";
            button2.Enabled = true;
            statusLbl.Text = "Disconnected...";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CreateGroup createGroup = new CreateGroup();
            createGroup.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            GroupInfo info = new GroupInfo();
            info.Show();
        }
    }
}
