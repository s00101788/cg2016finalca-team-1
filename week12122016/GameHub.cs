using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using GameData;
using System.Timers;

namespace week12122016
{
    public static class GameSate {
        public static List<PlayerData> players = new List<PlayerData>()
        {
            new PlayerData {
                PlayerID=1,
                FirstName ="Paul",
                SecondName ="Powell",
                Tag ="popple", XP=10000 }
        };
      

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();

        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }
    }
  

    public class GameHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
        public static TimeSpan countDown = new TimeSpan(0, 0, 0, 30); //ms countdown for the start fo the game
        public static Timer TimeToStart = new Timer(1000); //ms the timer for counting down in seconds
        public static TimeSpan GameCountdown = new TimeSpan(0, 5, 0); //ms how long the game lasts 5 mins here
        public static Timer GameTimer = new Timer(1000);
        public static int WorldX = 2000;
        public static int WorldY = 2000;

        public void join()
        {
            Clients.Caller.joined(WorldX, WorldY);
        }
        public PlayerData getPlayer(string tag)
        {
            PlayerData found = GameSate.players.FirstOrDefault(p => p.Tag == tag);
            if (found != null)
            {
                Clients.Caller.recievePlayer(found);
                TimeToStart.Elapsed += TimeToStart_Elapased; //ms making the methods for the time to start
                TimeToStart.Start();
                GameTimer.Elapsed += GameTimer_Elapsed; //ms making method for the game timer
                GameTimer.Start();
                return found;

            }
            else
            {
                Clients.Caller.error(new
                    ErrorMess
                { message = "Player not found for tag" + tag }
                    );
            }
            return found;

        }

        public List<CollectableData> GetCollectables(int count, int WorldX, int WorldY)
        {
            List<CollectableData> collectables = new List<CollectableData>();

            for (int i = 0; i < count; i++) //ms for each of the collectables print the message int he following format the id at the position x and y
            {
                collectables.Add(new CollectableData
                {
                    ACTION = COLLECTABLE_ACTION.DELIVERED,
                    collectableId = Guid.NewGuid().ToString(), // collectable id
                    CollectableName = "Collecatble " + i.ToString(), // the collectable name
                    collectableValue = GameSate.RandomNumber(20, 100), // collectable value
                    X = GameSate.RandomNumber(100, WorldX), // the x and y pos
                    Y = GameSate.RandomNumber(100, WorldY)
                });
            }
            return collectables;
        }
        private void GameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (GameCountdown.TotalMinutes > 0)
            {
                GameCountdown = GameCountdown.Subtract(new TimeSpan(0, 0, 0, 1)); //ms ticking down the game countdown by miliseconds
                Clients.All.recieveGameCount(GameCountdown.TotalMinutes);
            }
            else
            {

                GameTimer.Stop();
                Clients.All.End(); //ms ending the clients when the game is over

            }
        }

        public void Send(string name, string message)
        {
            Clients.All.broadcastMessage(name, message);
        }

        public void SendNewPosition(int i1, int i2)
        {
            Clients.Others.newPosition(i1, i2);

        }


        private void TimeToStart_Elapased(object sender, ElapsedEventArgs e)
        {
            if (countDown.TotalSeconds > 0)
            {
                countDown = countDown.Subtract(new TimeSpan(0, 0, 0, 1));
                Clients.All.recieveCountDown(countDown.TotalSeconds);
            }
            else
            {

                TimeToStart.Stop();
                Clients.All.Start(); // starting clients

            }
        }
    }

}