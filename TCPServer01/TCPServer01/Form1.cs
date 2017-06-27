using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace TCPServer01
{
    public partial class Form1 : Form
    {
        TcpListener aTcpListener;
        TcpClient aTcpClient;
        byte[] mRx;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStartListening_Click(object sender, EventArgs e)
        {
            IPAddress ipaddr;
            int nPort;

            if (!int.TryParse(tbPort.Text, out nPort))
            {
                nPort = 23000;
            }
            if (!IPAddress.TryParse(tbIPAddress.Text, out ipaddr))
            {
                MessageBox.Show("Invalid IP Address supplied");
                return;
            }

            aTcpListener = new TcpListener(ipaddr,nPort);

            aTcpListener.Start();

            aTcpListener.BeginAcceptTcpClient(onCompleteTcpClient,aTcpListener);


        }

         void onCompleteTcpClient(IAsyncResult iar)
        {
            TcpListener tcpl = (TcpListener)iar.AsyncState;
            try
            {

                aTcpClient = tcpl.EndAcceptTcpClient(iar);

                printLine("Client Conneted...");
                
                mRx = new byte[512];

                aTcpClient.GetStream().BeginRead(mRx,0,mRx.Length,OnCompleteFromTCPClientStream, aTcpClient);

            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OnCompleteFromTCPClientStream(IAsyncResult iar)
        {

            TcpClient tcpc;
            int nCountReadBytes = 0;
            string strRecv;

            try
            {
                tcpc = (TcpClient)iar.AsyncState;

                nCountReadBytes = tcpc.GetStream().EndRead(iar);

                if (nCountReadBytes == 0)
                {
                    MessageBox.Show("client disconnected. ");
                    return;
                }

                strRecv = Encoding.ASCII.GetString(mRx,0,nCountReadBytes);

                printLine(strRecv);

                mRx = new byte[512];

                tcpc.GetStream().BeginRead(mRx, 0, mRx.Length, OnCompleteFromTCPClientStream, tcpc);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        public void printLine(string _strPrint)
        {
            TheConsoleOutput.Invoke(new Action<string>(doInvoke), _strPrint);
        }

        public void doInvoke(string _strPrint)
        {
            TheConsoleOutput.Text = _strPrint + Environment.NewLine + TheConsoleOutput.Text;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            byte[] tx = new byte[512];

            if (string.IsNullOrEmpty(Payload.Text)) return;

            try
            {
                if(aTcpClient != null)
                {
                    if (aTcpClient.Client.Connected)
                    {
                        tx = Encoding.ASCII.GetBytes(Payload.Text);
                        aTcpClient.GetStream().BeginWrite(tx, 0, tx.Length, OnCompleteWriteToClientStream, aTcpClient);
                    }
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnCompleteWriteToClientStream(IAsyncResult iar)
        {

            try
            {
                TcpClient tcpc = (TcpClient)iar.AsyncState;
                tcpc.GetStream().EndWrite(iar);
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
    }
}
