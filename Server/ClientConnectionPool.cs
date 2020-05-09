using System;
using System.Collections;
using System.Text;

namespace Server
{
    class ClientConnectionPool
    {
        // Creates a synchronized wrapper around the Queue.
        private Queue SyncdQ = System.Collections.Queue.Synchronized(new Queue());

        public void Enqueue(ClientHandler client)
        {
            SyncdQ.Enqueue(client);
        }

        public ClientHandler Dequeue()
        {
            return (ClientHandler)(SyncdQ.Dequeue());
        }

        public int Count
        {
            get { return SyncdQ.Count; }
        }

        public object SyncRoot
        {
            get { return SyncdQ.SyncRoot; }
        }
    }

}
