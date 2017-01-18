using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;


namespace ChatClient
{
    class Program
    {
        static string name; 
        static IHubProxy proxy;
        static HubConnection connection = new HubConnection("http://localhost:5864/");
        static void Main(string[] args)
        {
            proxy = connection.CreateHubProxy("ChatHub");
            connection.Received += Connection_Received;
            connection.StateChanged += Connection_StateChanged;

            Action<string, string> SendMessageRecieved = recieved_a_message;
            proxy.On("broadcastMessage", SendMessageRecieved);

            connection.Start().Wait();
            Console.Write("Enter your Name: ");
            name = Console.ReadLine();

            proxy.Invoke("Send", new object[] { name, "Has joined" });

           // proxy.Invoke("SendNewPosition", new object[] { r.Next(0, 200), r.Next(0, 400) });

            Console.ReadKey();
            connection.Stop();
        }

        

        private static void Connection_StateChanged(StateChange State)
        {
            switch (State.NewState)
            {
                case ConnectionState.Disconnected:
                    Console.WriteLine("Just been disconected");
                    Console.ReadKey();
                    break;
                default:
                    Console.WriteLine("{0}", State.NewState);
                    break;
            }

        }

        private static void recieved_a_message(string sender, string message)
        {
            Console.WriteLine("{0} : {1}", sender, message);
        }

        private static void Connection_Received(string obj)
        {
            Console.WriteLine("Message Recieved {0}", obj);
        }
    }
}
