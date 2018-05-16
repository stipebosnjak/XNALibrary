using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xna.Helpers
{

 static class Shapes
    {


       public static Texture2D CreateRectangle(int width, int height,GraphicsDevice graphicsDevice)
       {
           Texture2D rectangleTexture = new Texture2D(graphicsDevice, width, height,true,SurfaceFormat.Color);// create the rectangle texture, ,but it will have no color! lets fix that
           Color[] color = new Color[width * height];//set the color to the amount of pixels

           for (int i = 0; i < color.Length; i++)//loop through all the colors setting them to whatever values we want
           {
               color[i] = new Color(0, 0, 0, 255);
           }
           rectangleTexture.SetData(color);//set the color data on the texture
           return rectangleTexture;
       }

    }

}
