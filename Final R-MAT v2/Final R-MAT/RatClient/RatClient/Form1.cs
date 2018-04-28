using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using RDPCOMAPILib;

namespace RatClient
{
    public partial class Form1 : Form
    {
        public static string sendString = "";
        public static RDPSession currentSession = null;
        //Lock
        [DllImport("user32")]
        public static extern void LockWorkStation();

        //LogOFF
        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        //Hybernate or Sleep
        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        //TaskManager
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string className, string windowText);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int command);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;

        private bool IsTaskBarShown = true;

        public static IntPtr TaskBarHWnd = FindWindow("Shell_TrayWnd", "");

        //Desktop Icons

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        public static IntPtr hWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null);

        public static int port = 4444;
        private static TcpClient client = new TcpClient();
        public static Thread getMessages = new Thread(readMessage);
        public static NetworkStream dataStream;

        public Form1()
        {
            InitializeComponent();
        }

        static int getIndex(byte[] buffer)
        {
            int i = 0;
            for (; i < buffer.Length; i++)
            {
                if (buffer[i] == 0x00) { break; }

            }
            return i;
        }

        private void exit(object sender, FormClosedEventArgs e)
        {
            getMessages.Abort();
            Environment.Exit(0);
        }


        static void analyzeMesage(string msg)
        {
            if (msg == "WarningMsg")
            {
                MessageBox.Show("Warning by an Administrator", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            else if (msg == "ShutdownPC")
            {
                System.Diagnostics.Process.Start("shutdown", "/s /t 60");
            }

            else if (msg == "RebootPC")
            {
                System.Diagnostics.Process.Start("shutdown", "/r /t 60");
            }

            else if (msg == "LogoffPC")
            {
                ExitWindowsEx(0, 0);
            }

            else if (msg == "HybernatePC")
            {
                SetSuspendState(true, true, true);
            }

            else if (msg == "LockPC")
            {
                LockWorkStation();
            }

            else if (msg == "SleepPC")
            {
                SetSuspendState(false, true, true);
            }

            else if (msg == "CancelShutdownPC")
            {
                System.Diagnostics.Process.Start("shutdown", "/a");
            }

            else if (msg == "HideTaskbar")
            {
                ShowWindow(TaskBarHWnd, SW_HIDE);
            }
            else if (msg == "UnhideTaskbar")
            {
                ShowWindow(TaskBarHWnd, SW_SHOW);
            }
            else if (msg == "Hide Desktop Icons")
            {
                ShowWindow(hWnd, 0);
            }
            else if (msg == "Show Desktop Icons")
            {
                ShowWindow(hWnd, 5);
            }

            else
            {
                MessageBox.Show(msg, "Message");
            }

        }


        static void readMessage()
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
                    analyzeMesage(temp);
                    temp = string.Empty;
                    dataStream.Flush();
                    dataStream.Flush();

                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }

        private void btnConnect_Click(object sender, EventArgs e)
        {

            try
            {
                client.Connect(IPAddress.Parse(txtIP.Text), port);
                dataStream = client.GetStream();
                getMessages.Start();
                btnConnect.Text = "Connected";
                MessageBox.Show("Connected to Listener");
                label2.Text = "Connected to Listener " + txtIP.Text;
                
                //RDP
                createSession();
                Connect(currentSession);
                sendString = getConnectionString(currentSession, "Text", "Group", "", 5);
                sendMessage(sendString);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to Connect...");
            }
        }
        //
        //RDP
        //
        public static void createSession()
        {
            currentSession = new RDPSession();
        }

        public static void Connect(RDPSession session)
        {
            session.OnAttendeeConnected += Incoming;
            session.Open();
        }

        public void disconnect(RDPSession session)
        {
            session.Close();
        }

        public static string getConnectionString(RDPSession session, string authstring, string group, string password, int clientlimit)
        {
            IRDPSRAPIInvitation invitation = session.Invitations.CreateInvitation(authstring, group, password, clientlimit);
            return invitation.ConnectionString;
        }

        public static void Incoming(object guest)
        {
            IRDPSRAPIAttendee MyGuest = (IRDPSRAPIAttendee)guest;
            MyGuest.ControlLevel = CTRL_LEVEL.CTRL_LEVEL_INTERACTIVE;
        }


        //
        //Send RDP String
        //
        public void sendMessage(string msg)
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

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

    }
}
