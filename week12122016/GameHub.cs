using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using GameData;

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

        public PlayerData getPlayer(string tag)
        {
            PlayerData found = GameSate.players.FirstOrDefault(p => p.Tag == tag);
            if (found != null)
                return found;
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

            for (int i = 0; i < count; i++)
            {
                collectables.Add(new CollectableData
                { ACTION = COLLECTABLE_ACTION.DELIVERED,
                     collectableId = Guid.NewGuid().ToString(),
                      CollectableName = "Collecatble " + i.ToString(),
                       collectableValue = GameSate.RandomNumber(20,100),
                        X = GameSate.RandomNumber(100, WorldX),
                        Y = GameSate.RandomNumber(100, WorldY)
                });
            }
            return collectables;
        }
    }
}