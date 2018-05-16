#region

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Xna.Helpers._2D
{
    /// <summary>
    /// Base class for creating Game entity 
    /// </summary>
    public class BaseGameEntity
    {
        private static int _counterId;
        private static List<BaseGameEntity> _baseGameEntities;
        private readonly int _id;
        private float _radius;
        private Vector2 _origin;
        #region Properties

        protected static List<BaseGameEntity> BaseGameEntities
        {
            get { return _baseGameEntities; }
            set { _baseGameEntities = value; }
        }
        /// <summary>
        /// Returns current base game entities count 
        /// </summary>
        public static int BaseGameEntititiesCount { get { return BaseGameEntities.Count; }}

        /// <summary>
        /// Texture used by the entity
        /// </summary>
        public Texture2D Texture { get; set; }
        /// <summary>
        /// Color of the entity
        /// </summary>
        public Color Color { get; set; }
        /// <summary>
        /// Position ofthe entity
        /// </summary>
        public Vector2 Position { get; set; }
        /// <summary>
        /// Rotation used by the entity
        /// </summary>
        public float Rotation { get; set; }
        /// <summary>
        /// Scale used by the entity
        /// </summary>
        public float Scale { get; set; }
        /// <summary>
        /// Radius of the entity
        /// </summary>
        public float Radius
        {
            get
            {
                return Math.Max(Texture.Height / 2, Texture.Width / 2);
            }
        }


        /// <summary>
        /// Id of the entity, auto generated
        /// </summary>
        public int Id
        {
            get { return _id; }
        }
        /// <summary>
        /// Origin of the entity
        /// </summary>
        public Vector2 Origin
        {
            get { return new Vector2(Texture.Width/2, Texture.Height/2); }
        }
        /// <summary>
        /// Is entity tagged
        /// </summary>
        public bool Tagged { get; set; }
        #endregion

        static BaseGameEntity()
        {
            _counterId = 0;
            _baseGameEntities = new List<BaseGameEntity>();
        }
        /// <summary>
        /// Defult constructor, generates the Id, adds entity to static list of entities
        /// </summary>
        public BaseGameEntity()
        {
            _counterId++;
            _id = _counterId;
            _baseGameEntities.Add(this);
            Tagged = false;
        }

        /// <summary>
        /// Override this to draw
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            
        }
        /// <summary>
        /// Override this to Update
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
        }
        /// <summary>
        /// Draws all BaseGameEntities 
        /// </summary>
        /// <param name="spriteBatch"></param>
        public static void DrawAll(SpriteBatch spriteBatch)
        {
            for (int i = _baseGameEntities.Count - 1; i >= 0; i--)
            {
                BaseGameEntity baseGameEntity = _baseGameEntities[i];
                baseGameEntity.Draw(spriteBatch);
            }
        }
        /// <summary>
        /// Updates all BaseGameEntities
        /// </summary>
        /// <param name="gameTime"></param>
        public static void UpdateAll(GameTime gameTime)
        {
            for (int i = _baseGameEntities.Count - 1; i >= 0; i--)
            {
                BaseGameEntity baseGameEntity = _baseGameEntities[i];
                baseGameEntity.Update(gameTime);
            }
        }
    }
}