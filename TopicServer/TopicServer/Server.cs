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
using System.IO;
using System.Net;

namespace TopicServer
{
    public partial class Server : Form
    {
        List<Socket> socketList = new List<Socket>();
        Socket socket ;
        Server server ;

        public Server()
        {
            InitializeComponent();
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            this.Stop();
            try
            {
               // thread.Abort();
            }
            catch (Exception e1) { richbox_AppendText("Error : " + e1.StackTrace + Environment.NewLine); }
        }


        private void btn_Start_Click(object sender, EventArgs e)
        {
            if (this.btn_Start.InvokeRequired)
            {
               this.btn_Start.Invoke((MethodInvoker)delegate()
                {
                    btn_Start_Click(sender, e);
                });
            }
            else
            {
                Accept();
            }
        }

        private void Accept()
        {
            try
            {
                //1.**** 비동기 수신용 소켓을 생성함 ...
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, Int32.Parse(txt_port.Text)));
                socket.Listen(10); // 수신가능 갯수 10개 

                btn_Start.Enabled = false;
                btn_stop.Enabled = true;

                //2.**** 비동기 수신용 2014.04.22(화)
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Complete);
                socket.AcceptAsync(args);
            }
            catch (Exception e)
            {
                richbox_AppendText("에러메세지: " + e.Message +Environment.NewLine + e.StackTrace+ Environment.NewLine);
            }
        }


        //3. *** 비동기로 클라이언트의 접속을 허가함 

        List<Socket> list_clientSocket=new List<Socket>();
        byte[] szData;
      

        private void Accept_Complete(object sender, SocketAsyncEventArgs e) 
        {
            try
            {
                Socket clientSocket = e.AcceptSocket;
                list_clientSocket.Add(clientSocket);

                if (list_clientSocket != null)
                {
                    richbox_AppendText(Environment.NewLine+"클라이언트접속: " + System.DateTime.Now);
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    szData = new byte[1024];
                    args.SetBuffer(szData, 0, 1024);
                    args.UserToken = list_clientSocket;
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
                    clientSocket.ReceiveAsync(args);
                }

                e.AcceptSocket = null;
                socket.AcceptAsync(e);
            }
            catch (SocketException eeee) { richbox_AppendText(Environment.NewLine+eeee.StackTrace); }
            catch (Exception ea) { richbox_AppendText(Environment.NewLine + ea.StackTrace); }
        }

        // 왜 여러번 수신 하는게 안돼지?

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket clientSocket = (Socket)sender;
            if(clientSocket.Connected && e.BytesTransferred >0){
                byte[] szData = e.Buffer;
                string sData = Encoding.Unicode.GetString(szData);

                string test = sData.Replace("\0", "").Trim();
                richbox_AppendText(Environment.NewLine+test);
            }
        }
    
        private void Stop()
        {
            try
            {
                socket.Close();

                btn_Start.Enabled = true;
                btn_stop.Enabled = false;
                
            }catch(Exception ex)
            {
                richbox_AppendText("에러메세지 : " + ex.Message+Environment.NewLine+ ex.StackTrace);
            }

        }

        private void richbox_AppendText(string text)
        {
            if (this.richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke((MethodInvoker)delegate()
                {
                    richbox_AppendText(text);
                });
            }
            else
            {
                this.richTextBox1.AppendText(text);
            }
        }

        public void close()
        {
            if (server.InvokeRequired)
            {
                server.Invoke((MethodInvoker)delegate()
                {
                    close();
                });
            }
            else
            {
                foreach (Socket pBuffer in list_clientSocket)
                {
                    if (pBuffer.Connected)
                    {
                        pBuffer.Disconnect(false);
                    }
                    pBuffer.Dispose();
                }
                socket.Dispose();
                server.Close();
            }
        }


      

    }
}

