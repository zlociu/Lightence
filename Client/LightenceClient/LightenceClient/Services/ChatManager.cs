using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using LightenceClient.Models;

namespace LightenceClient.Services
{
    class ChatManager: IDisposable
    {
        public readonly object _lock = new object();

        public List<Message> messages;
        public ConcurrentDictionary<string, List<Message> > privMessages;

        public ChatManager()
        {
            messages = new List<Message>();
            privMessages = new ConcurrentDictionary<string, List<Message>>();
        }

        public void AddMessage(Message message)
        {
            lock (_lock)
            {
                messages.Add(message);
            }
            
        }

        public void AddPrivMessage(string receiver, Message message)
        {
            if (privMessages.ContainsKey(receiver))
            {
                privMessages[receiver].Add(message);
            }
            else
            {
                Message[] array = {message};
                privMessages.TryAdd(receiver, new List<Message>(array));
            }
        }

        public void Dispose()
        {

        }
    }
}
