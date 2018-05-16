using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using C3.XNA;
using Xna.Helpers._2D;


namespace Xna.Helpers._2D
{
    /// <summary>
    /// Class for creating the world that contains various objects where the entities are interacting
    /// </summary>
  public class World
    {

        public static World CurrentWorld;

        //Layer names
        private const string LAYER_OBSTACLES = "Obstacles";
        private const string LAYER_WALLS = "Walls";
        private const string LAYER_DRAWABLE = "Drawable";
        private Level _map;
        private Layer _wallsLayer, _obstaclesLayer;
        private  List<Wall> _walls;
        private List<PrimitiveLine> _primitiveLines;
        private List<BaseGameEntity> _obstacles;
        
        private List<object> _drawingList;

      /// <summary>
      /// The walls in the world
      /// </summary>
      public ReadOnlyCollection<Wall> Walls
      {
          get { return _walls.AsReadOnly(); }
          
      }
      /// <summary>
      /// The obstacles in the world
      /// </summary>
      public ReadOnlyCollection<BaseGameEntity> Obstacles
       {
            get { return _obstacles.AsReadOnly(); }
       }


        internal ReadOnlyCollection<MovingEntity> MovingEntities
        {
            get { return MovingEntity.MovingEntities.AsReadOnly(); }
        }

        /// <summary>
      /// Creates a new world
      /// </summary>
     public World()
      {
          _walls=new List<Wall>();
          _primitiveLines=new List<PrimitiveLine>();
          _drawingList=new List<object>();
          _obstacles=new List<BaseGameEntity>();
      }


      /// <summary>
      /// Creates walls that fit screen bounds 
      /// </summary>
      /// <param name="screenHeight">Specify screen height</param>
     /// <param name="screenWidth">>Specify screen width</param>
     public void AddWorldBounds(int screenHeight,int screenWidth)
     {
         Wall wallUp = new Wall(new Vector2(0, 0), new Vector2(screenWidth, 0));
         Wall wallDown = new Wall(new Vector2(screenWidth, screenHeight), new Vector2(0, screenHeight));
         Wall wallLeft = new Wall(new Vector2(0, screenHeight), new Vector2(0, 0));
         Wall wallRight = new Wall(new Vector2(screenWidth, 0), new Vector2(screenWidth, screenHeight));
         _walls.Add(wallUp);
         _walls.Add(wallDown);
         _walls.Add(wallLeft);
         _walls.Add(wallRight);
     }

        /// <summary>
        /// Imports the map from specified xml document, must be created with GLEED2D tool.
        /// </summary>
        /// <param name="nameOfMap">Name of the map, resides in bin/ folder</param>
        /// <param name="contentManager"></param>
        
        public void ImportMap(string nameOfMap,ContentManager contentManager)
      {
          _map = Level.FromFile(nameOfMap,contentManager);
          _wallsLayer = _map.getLayerByName(LAYER_WALLS);
          _obstaclesLayer = _map.getLayerByName(LAYER_OBSTACLES);
         
          foreach (Item item in _obstaclesLayer.Items)
          {
              if (item is TextureItem)
              {
                  var textureItem = item as TextureItem;
                  var baseGameEntity=new BaseGameEntity();
                  textureItem.load(contentManager);
                  baseGameEntity.Texture = contentManager.Load<Texture2D>(textureItem.asset_name);
                  baseGameEntity.Position = textureItem.Position;
                  _obstacles.Add(baseGameEntity);
              }
          }
      }


     
      /// <summary>
     /// Imports the walls from specified map(xml document), must be created with GLEED2D tool and items must be named Wall%.
     /// Walls will not be drawn
     /// 
     /// </summary>
      public void GetWalls()
      {
          foreach (Item item in _wallsLayer.Items)
          {
              if (item is PathItem )
              {
                  var pathItem = item as PathItem;
                  Vector2 startVector, endVector;
                  for (int index = 0; index < pathItem.WorldPoints.Length-1; index++)
                  {
                      startVector = pathItem.WorldPoints[index];
                      endVector = pathItem.WorldPoints[index + 1];
                     _walls.Add(new Wall(startVector, endVector));
                  }
              }
          }
      }

    
      public void ImportWallsAsDrawableObjects(string nameOfmap)
      {
      
      }
      

      public void GetObstacles()
      {
          
      }

      private void ImportWall()
      {
          
      }
      /// <summary>
      /// Draws the drawable objects , (Walls)
      /// </summary>
     public void Draw(SpriteBatch spriteBatch)
     {
         foreach (var layer in _map.Layers)
         {
             foreach (var item in layer.Items)
             {
                 if (item is CircleItem)
                 {
                     var circleItem = item as CircleItem;
                     Primitives2D.DrawCircle(spriteBatch,circleItem.Position,circleItem.Radius,30,Color.DarkBlue);
                 }
                 if (item is RectangleItem)
                 {
                     var rectangleItem = item as RectangleItem;
                     Rectangle rectangle=new Rectangle((int) rectangleItem.Position.X,(int) rectangleItem.Position.Y,(int) rectangleItem.Width,(int) rectangleItem.Height);
                    
                     Primitives2D.DrawRectangle(spriteBatch,rectangle,Color.DarkBlue);
                 }

                 if (item is TextureItem)
                 {
                     var textureItem = item as TextureItem;
                     textureItem.draw(spriteBatch);
                 }
             }
         }
     }

     //private void CreatePrimitiveLines(GraphicsDevice graphicsDevice)
     //{
         
     //    //     _primitiveLines = new List<PrimitiveLine>();
     //    //foreach (var layer in _map.Layers)
     //    //{
     //    //    foreach (var item in layer.Items)
     //    //    {
     //    //        if (item is CircleItem)
     //    //        {
     //    //            var circleItem = item as CircleItem;

     //    //            PrimitiveLine primitiveLine = new PrimitiveLine(graphicsDevice);
     //    //            primitiveLine.Position = circleItem.Position;
     //    //            primitiveLine.CreateCircle(circleItem.Radius, 50);
     //    //            _primitiveLines.Add(primitiveLine);
     //    //        }
     //    //        if (item is RectangleItem)
     //    //        {
     //    //            var rectangleItem = item as RectangleItem;

     //    //            PrimitiveLine primitiveLine = new PrimitiveLine(graphicsDevice);
     //    //            primitiveLine.Position = rectangleItem.Position;
     //    //             C3.XNA.Primitives2D.
                    
     //    //            _primitiveLines.Add(primitiveLine);
     //    //        }
     //    //    }
     //    //}

     //}
  }


}

