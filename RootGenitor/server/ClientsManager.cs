using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace RootGenitor.server
{
    class ClientsManager
    {
        ConcurrentDictionary<string, NetworkStream> clientsByToken;
        ConcurrentDictionary<NetworkStream, string> clientsByStream;
        public ClientsManager()
        {
            clientsByToken = new ConcurrentDictionary<string, NetworkStream>();
            clientsByStream = new ConcurrentDictionary<NetworkStream, string>();
        }

        public void addClient(string token, NetworkStream n)
        {
            clientsByToken.AddOrUpdate(token, n, (s, n) => n);
            clientsByStream.AddOrUpdate(n, token, (s, n) => token);
        }

        public NetworkStream getStreamByToken(string token)
        {
            return clientsByToken[token]; //TODO: try/catch with logging
        }

        public string getTokenByStream(NetworkStream n)
        {
            return clientsByStream[n]; //TODO: try/catch with logging
        }

        public void removeClient(NetworkStream n)
        {
            string token;
            var ok = clientsByStream.TryGetValue(n, out token);
            if (ok)
            {
                clientsByToken.TryRemove(token, out NetworkStream absent);
                clientsByStream.TryRemove(n, out string absent2);
            }
        }
    }
}
