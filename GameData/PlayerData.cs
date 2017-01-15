using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameData
{
    public class PlayerData
    {
        public int PlayerID { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Tag { get; set; }
        public int XP { get; set; }
        public Guid playerid;
        public string GamerTag;
        public int topscore;

        public string GamerTagScore { get { return GamerTag + " ==> " + topscore.ToString(); } }

        public PlayerData()
        {
        }
        public static PlayerData FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            PlayerData player = new PlayerData();
            player.playerid = Guid.NewGuid();
            player.FirstName = values[0];
            player.SecondName = values[1];
            player.GamerTag = values[2];
            player.topscore = Int32.Parse(values[3]);

            return player;
        }
}
