using Newtonsoft.Json;
using Socket_Programming_Server_Side;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiServer
{
    class Program
    {
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly Dictionary<string, Socket> clientSockets = new Dictionary<string, Socket>();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 1031;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];

        static void Main()
        {
            Console.Title = "Server";
            SetupServer();
            Console.ReadLine(); // When we press enter close everything
            CloseAllSockets();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private static void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets.Values)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }

            clientSockets.Add(((IPEndPoint)socket.RemoteEndPoint).Address.ToString(), socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Client connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current.RemoteEndPoint.ToString());
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Message message = JsonConvert.DeserializeObject<Message>(text);
            Console.WriteLine("Received Text: " + message.MessageContent);

            Console.WriteLine("Text is a get time request");
            byte[] data = Encoding.ASCII.GetBytes(DateTime.Now.ToLongTimeString());
            clientSockets[message.ToAddress].Send(message.MessageContent);
            Console.WriteLine("Time sent to client");


//            NF.MulticastSender m_MulticastSender = new NF.MulticastSender(Address, Port, TTL);
//            WinSound.Recorder m_Recorder = new WinSound.Recorder();
//            //Define a callback function for receiving datas from soundcard
//            m_Recorder.DataRecorded += new WinSound.Recorder.DelegateDataRecorded(OnDataReceivedFromSoundcard);
//            //Start Recorder 
//            m_Recorder.Start
//            (SoundDeviceName, SamplesPerSecond, BitsPerSample, ChannelCount, BufferCount, BufferSize)
////In callback function, get the linear datas from soundcard and translate to MuLaw
//Byte[] mulawData = WinSound.Utils.LinearToMulaw(linearData, Config.BitsPerSample, Config.Channels);
//            //Create the RTP Header 
//            Byte[] rtpBytes = RTPHeader.ToBytes();
//            //Combine RTP Header and mulawData
//            Byte[] bytes = new Byte[mulawData.Length + WinSound.RTPPacket.MinHeaderLength];
//            Array.Copy(rtpBytes, 0, bytes, 0, WinSound.RTPPacket.MinHeaderLength);
//            Array.Copy(mulawData, 0, bytes, WinSound.RTPPacket.MinHeaderLength, mulawData.Length);
//            //Send Bytes to Multicast
//            m_MulticastSender.SendBytes(bytes);


            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }
    }
}