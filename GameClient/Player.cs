using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sprites;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using GameData;

namespace GameClient
{
    public class Player : DrawableGameComponent
    {
        public PlayerData PlayerInfo { get; set; }
        public enum DIRECTION { LEFT, RIGHT, UP, DOWN, STANDING };
        DIRECTION _direction = DIRECTION.STANDING;

        public DIRECTION Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }
        int _score;
        float _speed;
        Texture2D[] _textures;
        SoundEffect[] _directionSounds;
        SoundEffectInstance _soundPlayer;
        SpriteFont font;

        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }


        public Texture2D _skin;
        public Vector2 position = new Vector2(0, 0);
        public Vector2 PreviousPosition = new Vector2(0, 0);
        public int score = 0;
        public int framecount;

        public Player(Game g, string TexName, Vector2 Position, int Score,int Framecount) : base(g)
        {
            _skin = _Skin;
            position = Position;
            score = Score;
            framecount = Framecount;
        }
        



        int _health;
        public int Health { get { return _health; } set { _health = value; } }

        //public Player(Texture2D[] tx, SoundEffect[] sounds,
        //    Vector2 pos, int frameCount, 
        //    int startScore, float speed) 
        //    : base(tx[0],pos,frameCount)
        //{

        //    _speed = speed;
        //    _textures = tx;
        //    _directionSounds = sounds;
        //    _health = 100;

        //}

        public override void Update(GameTime gameTime)
        {
            PreviousPosition = position;
            base.Update(gameTime);
            // TODO: Add your update logic here
            _direction = DIRECTION.STANDING;
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                _direction = DIRECTION.LEFT;
                position += new Vector2(-1, 0) * _speed;
                //base.Move(new Vector2(-1, 0) * _speed);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                _direction = DIRECTION.UP;
                position += new Vector2(0, -1) * _speed;
                //base.Move(new Vector2(0, -1) * _speed);
            }
            if
            (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                _direction = DIRECTION.DOWN;
                position += new Vector2(0, 1) * _speed;
                //base.Move(new Vector2(0, 1) * _speed);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                _direction = DIRECTION.RIGHT;
                position += new Vector2(1, 0) * _speed;
                //base.Move(new Vector2(1, 0) * _speed);
            }
            //else
            //{
            //    //Position = PreviousPosition;
            //    _direction = DIRECTION.STANDING;
            //}

            //SpriteImage = _textures[(int)_direction];

            //if (_soundPlayer == null || _soundPlayer.State == SoundState.Stopped)
            //{
            //    if (_direction != DIRECTION.STANDING)
            //    {
            //        _soundPlayer = _directionSounds[(int)_direction].CreateInstance();
            //        _soundPlayer.Play();
            //    }
            //}
        }
    }
}
