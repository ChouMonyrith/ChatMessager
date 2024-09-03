using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Xml.Schema;
using System.Threading;

namespace Task2_Exam
{
    public partial class Form1 : Form
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _receiveThread;
        private bool _connected;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_connected)
            {
                MessageBox.Show("Already connected.");
                return;
            }

            try
            {
                _client = new TcpClient("127.0.0.1", 5000);
                _stream = _client.GetStream();
                _connected = true;
                btnConnect.Enabled = false;
                txtUsername.Enabled = false;
                string username = txtUsername.Text;

                byte[] usernameBytes = Encoding.ASCII.GetBytes(username);
                _stream.Write(usernameBytes, 0, usernameBytes.Length);
                Invoke((MethodInvoker)delegate
                {
                    textBox2.AppendText("Connected to server: ");
                });
                _receiveThread = new Thread(ReceiveMessages);
                _receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
        }
        private void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while (_connected)
            {
                try
                {
                    bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Invoke((MethodInvoker)delegate
                        {
                            textBox2.AppendText(message + Environment.NewLine);
                        });
                    }
                }
                catch
                {
                    _connected = false;
                    Invoke((MethodInvoker)delegate
                    {
                        btnConnect.Enabled = true;
                        txtUsername.Enabled = true;
                    });
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!_connected)
            {
                MessageBox.Show("Not connected to the server.");
                return;
            }

            try
            {
                string message = txtMessage.Text;
                string formattedMessage = $"{DateTime.Now.ToString("HH:mm:ss")} {txtUsername.Text}: {message}";
                byte[] messageBytes = Encoding.ASCII.GetBytes(formattedMessage);
                _stream.Write(messageBytes, 0, messageBytes.Length);
                txtMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}");
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if(!_connected)
            {
                MessageBox.Show("Not Connected To Sever.");
                return;
            }
            try
            {
                
                string DCMessage = $"{DateTime.Now.ToString("HH:mm:ss")} {txtUsername.Text}: Disconnected";
                byte[] messageBytes = Encoding.ASCII.GetBytes(DCMessage);
                _stream.Write(messageBytes, 0, messageBytes.Length);
                Invoke((MethodInvoker)delegate
                {
                    textBox2.AppendText(DCMessage + Environment.NewLine);
                });

                _connected = false;
                _client.Close();
                _receiveThread.Join();
                btnConnect.Enabled = true;
                txtUsername.Enabled = true;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error disconnecting from server: {ex.Message}");
            }
        }
    }
}
