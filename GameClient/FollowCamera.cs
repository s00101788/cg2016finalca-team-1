using Microsoft.Xna.Framework;
using Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameClient;

namespace Cameras
{
    class FollowCamera : DrawableGameComponent
    {
        Vector2 worldSize;
        Rectangle worldRect;
        Vector2 cameraPosition;

        Matrix cameraTransform;

        public Rectangle WorldRect
        {
            get
            {
                return worldRect;
            }

            set
            {
                worldRect = value;
            }
        }

        public Matrix CameraTransform
        {
            get
            {
                return cameraTransform;
            }

            set
            {
                cameraTransform = value;
            }
        }

        public FollowCamera(Game g ,Vector2 pos, Vector2 WorldBound): base(g)
        {
            cameraPosition = pos;
            worldSize = WorldBound;
            cameraTransform = Matrix.Identity * Matrix.CreateTranslation(-cameraPosition.X, -cameraPosition.Y, 0);
        }


        // Assumes the character is centered on the screen in the Viewport to begin
        public void Follow(Player player)
        {
            Vector2 CenterView = new Vector2(GraphicsDevice.Viewport.Width / 2 , GraphicsDevice.Viewport.Height/2 );
            if (
                player.position.X > CenterView.X &&
                player.position.X < worldSize.X - CenterView.X)
                cameraPosition.X += player.position.X - player.PreviousPosition.X;
            if (
                player.position.Y > CenterView.Y &&
                player.position.Y < worldSize.Y - CenterView.Y)
                cameraPosition.Y += player.position.Y - player.PreviousPosition.Y;

            // Clamp the camera to the world bounds
            cameraPosition = Vector2.Clamp(cameraPosition,
                    Vector2.Zero,
                    new Vector2(worldSize.X - GraphicsDevice.Viewport.Width,
                                worldSize.Y - GraphicsDevice.Viewport.Height));
            // Calculate the Camera Transform
        CameraTransform = Matrix.Identity * Matrix.CreateTranslation(-cameraPosition.X, -cameraPosition.Y, 0);

        }
}
}
