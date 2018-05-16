using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Xna.Helpers._3D
{
  public class EffectManager
    {
        public static ResourceContentManager Content;
        
        public static void CreateContentResource(Game game)
        {
            Content = new ResourceContentManager(game.Services, EffectsResource.ResourceManager);
        }
    }
}
