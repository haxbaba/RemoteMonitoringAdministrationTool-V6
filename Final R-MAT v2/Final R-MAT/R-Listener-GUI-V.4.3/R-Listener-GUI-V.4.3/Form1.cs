using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace R_Listener_GUI_V._4._3
{
    public partial class R_MAT : Form
    {
        public static string textString = "";
        public static int port = 4444;
        public static TcpListener listener = new TcpListener(IPAddress.Any, port);
        public static TcpClient client;
        public static Thread conn = new Thread(awaitConnection);
        public static Thread getMessage = new Thread(readMessage1);
        public static NetworkStream dataStream;



        static void sendMessage(string msg)   //this is the function to send
        {
            char[] data = msg.ToCharArray();
            byte[] buffer = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                buffer[i] = (byte)data[i];
            }
            dataStream.Write(buffer, 0, buffer.Length);
            dataStream.Flush();
        }

        static void awaitConnection()
        {

            listener.Start();
            client = listener.AcceptTcpClient();
            dataStream = client.GetStream();
            getMessage.Start();
            MessageBox.Show("Client Connected!");

        }

        private void exit(object sender, FormClosedEventArgs e)
        {
            conn.Abort();
            getMessage.Abort();
            listener.Stop();
            Environment.Exit(0);
        }


        public R_MAT()
        {

            InitializeComponent();
        }

        IPAddress findMyIPV4Address()
        {
            string strThisHostName = string.Empty;
            IPHostEntry thisHostDNSEntry = null;
            IPAddress[] allIPsOfThisHost = null;
            IPAddress ipv4Ret = null;

            try
            {
                strThisHostName = System.Net.Dns.GetHostName();
                thisHostDNSEntry = System.Net.Dns.GetHostEntry(strThisHostName);
                allIPsOfThisHost = thisHostDNSEntry.AddressList;

                for (int idx = allIPsOfThisHost.Length - 1; idx >= 0; idx--)
                {
                    if (allIPsOfThisHost[idx].AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipv4Ret = allIPsOfThisHost[idx];
                        break;
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }

            return ipv4Ret;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            conn.Start();
            IPAddress a = findMyIPV4Address();
            LabelIP.Text = a.ToString();
            
        }

        


        private void button2_Click(object sender, EventArgs e)
        {
            sendMessage("WarningMsg");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sendMessage("ShutdownPC");
            MessageBox.Show("The Client PC will Shutdown in 1 Minute");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            sendMessage("RebootPC");
            MessageBox.Show("The Client PC will Restart in 1 Minute");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sendMessage("LogoffPC");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            sendMessage("HybernatePC");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            sendMessage("LockPC");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            sendMessage("SleepPC");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            sendMessage("CancelShutdownPC");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sendMessage(textBox1.Text);
            textBox1.Text = "";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            sendMessage("UnhideTaskbar");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            sendMessage("HideTaskbar");
        }
        
        private void button13_Click(object sender, EventArgs e)
        {
            sendMessage("Show Desktop Icons");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            sendMessage("Hide Desktop Icons");
        }

        private void RDP_Click(object sender, EventArgs e)
        {
            new Remote_Desktop(textString).Show();
        }
        //
        //Message Receive for RDP String
        //
        static int getIndex(byte[] buffer)
        {
            int i;
            for (i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0x00) { break; }
            }
            return i;
        }

        static void analyzeMessage(string msg)
        {
            textString = msg;
        }

        static void readMessage1()
        {
            string temp = "";
            while (true)
            {
                if (dataStream.DataAvailable == true)
                {
                    byte[] buffer = new byte[1048];
                    dataStream.Read(buffer, 0, buffer.Length);
                    int cutAt = getIndex(buffer);
                    for (int i = 0; i < cutAt; i++)
                    {
                        temp += (char)buffer[i];
                    }
                    analyzeMessage(temp);
                    temp = string.Empty;
                    dataStream.Flush();
                }
            }
        }
    }
}
