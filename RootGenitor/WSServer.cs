using RootGenitor.datasource;
using RootGenitor.matchmaking;
using RootGenitor.server;
using RootGenitor.utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RootGenitor
{
    class WSServer
    {
        public ClientsManager manager;

        public WSServer()
        {
            MatchMaking.server = this;
            manager = new ClientsManager();
        }
        public void sendMessage(NetworkStream n, string msg)
        {
            var response = Util.EncodeMessageToSend(msg);
            n.Write(response, 0, response.Length);
        }
        public void trySendMessage(string token, string msg)
        {
            NetworkStream n = manager.getStreamByToken(token);
            sendMessage(n, msg);
        }
        public void serve(string ip, int port)
        {
            var server = new TcpListener(IPAddress.Parse(ip), port);
            server.Start();
            Console.WriteLine("Server started");
            try
            {
                while(true)
                {
                    var client = server.AcceptTcpClient();
                    var task = Task.Run(async () =>
                    {
                        Console.WriteLine("Task start");
                        await handle(client);
                        Console.WriteLine("Task end");
                    });
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                Console.WriteLine("Server stop");
                server.Stop();
            }
        }


        void unreg(NetworkStream n)
        {
            string token = manager.getTokenByStream(n);
            manager.removeClient(n);
            MatchMaking.unreg(token);
        }

        public async Task handleHandshake(string str, NetworkStream n)
        {
            Console.WriteLine("=====Handshaking from client=====\n{0}", str);

            // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
            // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
            // 3. Compute SHA-1 and Base64 hash of the new value
            // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
            string swk = Regex.Match(str, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
            string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
            string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

            // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
            byte[] response = Encoding.UTF8.GetBytes(
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Connection: Upgrade\r\n" +
                "Upgrade: websocket\r\n" +
                "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

            await n.WriteAsync(response, 0, response.Length);
        }
        public async Task handle(TcpClient client)
        {
            try
            {
                using (client)
                using (NetworkStream n = client.GetStream())
                {
                    Console.WriteLine("A client connected: " + client.Client.RemoteEndPoint.ToString());

                    while (true)
                    {
                        while (!n.DataAvailable) ;
                        byte[] data = new byte[client.Available];
                        await n.ReadAsync(data, 0, client.Available);
                        string s = Encoding.UTF8.GetString(data);

                        string str = Encoding.Default.GetString(data);
                        if (Regex.IsMatch(str, "^GET", RegexOptions.IgnoreCase))
                        {
                            await handleHandshake(str, n);
                        }

                        else
                        {
                            bool fin       = (data[0] & 0b10000000) != 0; //https://tools.ietf.org/html/rfc6455
                            bool mask      = (data[1] & 0b10000000) != 0; 
                            bool textframe = (data[0] & 0b00000001) != 0;
                            bool binary    = (data[0] & 0b00000010) != 0;
                            bool closing   = (data[0] & 0b00001000) != 0;

                            if (textframe)
                            {
                                string text = Util.getTextFromData(data);
                                MatchMaking.processTextFrame(text, n);
                            }
                            else if (closing)
                            {
                                Console.WriteLine("client closed connection: {0}", client.Client.RemoteEndPoint.ToString());
                                unreg(n);
                                break;
                            }
                        }
                    }
                }
            }

            catch(Exception ex)
            {
                Console.WriteLine("ex1");
                Console.WriteLine(ex.Message);
            }
            
        }
    }
}
