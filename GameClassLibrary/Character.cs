using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClassLibrary
{
    public class Character : Sprite
    {
        string ID;
        int Ammunition;
        public int Health;
        public int movementSpeed;
        public int strength;

        public Character(string id, Texture2D tex, int speed, int str) : base(tex, Vector2.Zero)
        {
            ID = id;
            Health = 100;
            Ammunition = 20;
            movementSpeed = speed;
            strength = str;

        }
        
        public Bullet Shoot(Vector2 pos, Vector2 direction, Color c)
        {
            if (Ammunition > 0)
            {
                Ammunition--;
                return new Bullet(ID, _texture, strength, pos, direction, c);
            }
            else
                return null;
        }

        public void GotShoot(Bullet b)
        {
            if(ID != b.createdPlayerID)
                Health -= b.damage;
        }


    }
}
