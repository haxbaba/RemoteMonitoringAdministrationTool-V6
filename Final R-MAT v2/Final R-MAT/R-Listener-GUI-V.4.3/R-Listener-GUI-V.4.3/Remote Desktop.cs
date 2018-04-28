using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RDPCOMAPILib;
using AxRDPCOMAPILib;

namespace R_Listener_GUI_V._4._3
{
    public partial class Remote_Desktop : Form
    {
        string textConnectionString;
        public Remote_Desktop(string textConnectionString)
        {
            this.textConnectionString = textConnectionString;
            InitializeComponent();
        }

        private void Remote_Desktop_Load(object sender, EventArgs e)
        {
            try
            {
                connect(textConnectionString, this.axRDPViewer1, "", "");
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to Connect to Server");
            }
        }
        public static void connect(string invitation, AxRDPViewer display, string userName, string password)
        {
            display.Connect(invitation, userName, password);
        }

        public static void disconnect(AxRDPViewer display)
        {
            display.Disconnect();
        }
    }
}
