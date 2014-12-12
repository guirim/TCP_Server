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
        
       // public delegate void AppendTextCallback(string text);
       // public AppendTextCallback AppendText;

        public NetworkStream stream;
        public NetworkStream clientStream;

        List<Socket> socketList = new List<Socket>();


        TcpListener listener = null;
        TcpClient client = null;

        Thread thread = null;

        Socket socket;

        Server server ;
        Socket clientSock;

        IAsyncResult asyncResult ;

        byte[] revData = new byte[1024];
        byte[] clientData = new byte[1024]; // 클라이언트에서 송신한 데이터 수신받아 저장하는곳 

      

        public Server()
        {
            InitializeComponent();
           // server = new Server();
           // Thread serverForm = new Thread(new ThreadStart(init));
          //  server = new Server();
          //  serverForm.Start();
        }

        
        private void init()
        {
          
           // Application.Run(server);
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            this.Stop();
            try
            {
                thread.Abort();
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
                thread = new Thread(new ThreadStart(Accept));
                thread.Start();
            }
        }

        private void Accept()
        {
            try
            {
                // 동기 수신에 사용
               // IPAddress ipaddress = IPAddress.Parse(txt_ip.Text);
               // listener = new TcpListener(ipaddress,Int32.Parse(txt_port.Text));


                // 블록킹을 false 로 둚  : 클라이언트의 동작이 비정상적으로 멈추는걸 방지하기 위해 
                socket.Blocking = false;

                // 비동기 수신용 소켓을 생성함 ...
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, Int32.Parse(txt_port.Text)));
                socket.Listen(10); // 수신가능 갯수 10개 


                btn_Start.Enabled = false;
                btn_stop.Enabled = true;

              
               // socket.Listen(100);

                
                  stream = new NetworkStream(socket);
                 string currentDate = DateTime.Now.ToString();

                    // thread lock 
                    lock (this)
                    { try
                        {
                            richbox_AppendText("클라이언트 접속 :" + currentDate + Environment.NewLine);
                            stream.Write(Encoding.UTF8.GetBytes(currentDate), 0, Encoding.UTF8.GetBytes(currentDate).Length);
                        }
                        catch (Exception ee)
                        {
                            richbox_AppendText("Error : " + ee.Message + Environment.NewLine + ee.StackTrace+ Environment.NewLine);
                        }
                    }
                
                // 동기 수신시 listener 사용시 사용함 
               // listener.Start();
                //socket = listener.AcceptSocket();
                
                   while(true) // 무한히기다리면서 연결요청 수락 
                    {
                        /*멀티플렉싱 (다중접속을 위한 )**/
                        // 멀티플렉싱을 위해 접속 요청되었던 소켓들을 리스트에 담음 
                        socketList.Add(socket);
                       
                        IAsyncResult result =  socket.BeginAccept(new AsyncCallback(AcceptCallback),socket);

                        // 새로운 BeginAccept 객체를 생성하기 전에 EndAccept 객체를 대기 
                       // result.AsyncWaitHandle.WaitOne();
                   }  
            }
            catch (Exception e)
            {
                richbox_AppendText("에러메세지: " + e.Message +Environment.NewLine + e.StackTrace+ Environment.NewLine);
            }
        }
          

        public void AcceptCallback(IAsyncResult asyncResult)
        {  
            //try {
            //    listener.Start();
            //    /*멀티플렉싱 (다중접속을 위한 )**/
            //    // 리스트에 담겨있는 모든 소켓이  읽기가능확인, 쓰기가능확인, 에러확인, 제한시간 시간동안만
            //      Socket.Select(socketList, socketList, null, -1);

            //       // Thread.Sleep(10);
            //        // listener.Start();
            //               for (int i = 0; i < socketList.Count; i++)
            //               {
            //                   socket = listener.AcceptSocket();
            //                   clientStream = new NetworkStream(socket);

            //                   // client = listener.BeginAcceptTcpClient();
            //                   clientStream = client.GetStream();

            //                   // 클라이언트를 생성해서 클라이언트가 보내느 메세지를 수신케 하기 위함 
            //                   // clientStream.Read(bytes, 0, bytes.Length);
            //                   asyncResult = clientStream.BeginRead(revData, 0, revData.Length, CustomReadCallback, clientStream);
            //               }
            //               socket.Close();
            //      }
            //       catch (SocketException e) { richbox_AppendText("Error" + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine); }
            //     catch (ArgumentNullException nulle) { richbox_AppendText("Error : " + nulle.Message + Environment.NewLine + nulle.StackTrace + Environment.NewLine); }
            //     catch (NullReferenceException nulle2) { richbox_AppendText("Error : " + nulle2.Message + Environment.NewLine + nulle2.StackTrace + Environment.NewLine); }



                    /************************************/   
            Socket servSock = (Socket)asyncResult.AsyncState;
           
            try
            {
             
               clientSock= servSock.EndAccept(asyncResult);
              
                // 비동기 수신을함 
               clientSock.BeginReceive(clientData, 0, clientData.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), "");

            }catch(SocketException ess){
                richbox_AppendText(ess.ErrorCode+Environment.NewLine+ ess.Message+Environment.NewLine);
                clientSock.Close();
            }
        }


        public void ReceiveCallback(IAsyncResult asyncResult)
        {
           // richbox_AppendText("데이터를 수신함");
            Server server = (Server)asyncResult.AsyncState;
            try
            {
                int revcMsgSize = server.clientSock.EndReceive(asyncResult);

                if(revcMsgSize>0){
                    richbox_AppendText("수신받은 데이터 : "+ Encoding.UTF8.GetString(clientData));
                }
            }
            catch (SocketException s1) { richbox_AppendText(""+s1.Message +""); }
        }

        private void Stop()
        {
            try
            {
                stream.Close();
                listener.Stop();
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
                //AppendTextCallback callback = new AppendTextCallback(richbox_AppendText);
               // this.richTextBox1.Invoke(callback, new object[] { text });
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
                server.Close();
            }
        }


      public   void CustomReadCallback(IAsyncResult asyncResult)
        {
           // asyncResult.AsyncState.N
            server = (Server)asyncResult.AsyncState;
           int readResult= server.clientStream.EndRead(asyncResult);

            // 컨넥트시 수신받은 데이터를 저장한 revData 를 불러와서 사용 
           String revMsg = Encoding.UTF8.GetString(clientData, 0, clientData.Length);
            richbox_AppendText("수신 : " + revMsg);
            
        }

      public   void CutomWriteCallback(IAsyncResult asyncResult)
        {
            
        }
    }
}

