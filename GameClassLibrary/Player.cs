using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClassLibrary
{
    public class Player : Sprite
    {
        static int ID = 1;
        int PlayerID;
        public Character PlayerChar;
        //public Vector2 Position;
        public Vector2 FireDirection = new Vector2(1,0);
        Texture2D healthBar;
        public int score;
        Game game;


        public Player() : base()
        {

        }

        public Player(Character character, Texture2D health, Vector2 pos, Color c, Game g) : base(character._texture, pos, c) //create the player
        {
            PlayerID = ID++;
            PlayerChar = character;
            //Position = pos;
            healthBar = health;
            game = g;
        }

        public void Move(KeyboardState state)
        {
            if (state.IsKeyDown(Keys.W) && _position.Y > 0) //check the move direction and update position
            {
                _position -= new Vector2(0, PlayerChar.movementSpeed); 
                FireDirection = new Vector2(0, -1);
            }
            if (state.IsKeyDown(Keys.S) && _position.Y < 550)
            {
                _position += new Vector2(0,PlayerChar.movementSpeed);
                FireDirection = new Vector2(0,1);
            }
            if (state.IsKeyDown(Keys.D) && _position.X < 760)
            {
                _position += new Vector2(PlayerChar.movementSpeed, 0);
                FireDirection = new Vector2(1, 0);
            }
            if (state.IsKeyDown(Keys.A) && _position.X > 0)
            {
                _position -= new Vector2(PlayerChar.movementSpeed, 0);
                FireDirection = new Vector2(-1, 0);
            }
            //if (state.IsKeyDown(Keys.Space))
            //    PlayerChar.Shoot(FireDirection, game);
        }

        public void Collect(Collectable c)
        {
            score += c.Value;
        }

        public void Draw(SpriteBatch sp, SpriteFont sf)
        {
            sp.Begin();
            sp.Draw(PlayerChar._texture, _position, pColor); //draw the player
            sp.Draw(healthBar, new Rectangle((int)_position.X , (int)_position.Y + _texture.Height + 2, healthBar.Width, healthBar.Height), Color.Red); //draw the negative HealthBar
            sp.Draw(healthBar, new Rectangle((int)_position.X , (int)_position.Y + _texture.Height + 2, (int)(healthBar.Width * ((double)PlayerChar.Health / 100)), healthBar.Height), Color.Green); //calculate and draw the positive HealthBar above 
            //sp.Draw(healthBar, Position + new Vector2(_texture.Width - healthBar.Width, _texture.Height * 2) / 2, Color.Red);
            //sp.Draw(healthBar, Position + new Vector2(_texture.Width - healthBar.Width, _texture.Height * 2) / 2, Color.Green);
            sp.End();
        }
    }
}
