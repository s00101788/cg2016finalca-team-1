﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClassLibrary
{
    public class SuperCollectable : Collectable
    {


        public SuperCollectable(Texture2D tex, Vector2 pos) : base(tex, pos)
        {
            Value = 20;
        }

        public void AvoidChasing()
        {

        }
    }
}
