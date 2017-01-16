﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace week12122016
{
    public class ChatHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }

        public void Send(string name, string message) //broadcast messages from one client to all other clients when sent
        {
            Clients.All.broadcastMessage(name, message);

        }
    }
}