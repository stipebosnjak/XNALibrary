using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xna.Helpers._3D.Entity
{

    #region Enums

    #endregion

    public class MovingGameEntity :BaseGameEntity
    {
        #region Fields

        private SteeringBehaviours _steeringBehaviours;

        private static List<MovingGameEntity> _movingGameEntitiesList;
        #endregion

        #region Properties

        /// <summary>
        /// Model velocity
        /// </summary>
        public Vector3 Velocity { get; set; }
        /// <summary>
        /// Model max speed
        /// </summary>
        public float MaxForce { get; set; }
        /// <summary>
        /// Model max speed
        /// </summary>
        public float MaxSpeed { get; set; }
        /// <summary>
        /// Model mass
        /// </summary>
        public float Mass { get; set; }
        /// <summary>
        /// Model speed
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// List that contains all Moving game entities
        /// </summary>
        public static List<MovingGameEntity> MovingGameEntities
        {
            get { return _movingGameEntitiesList; }
        }
        /// <summary>
        /// Steering behaviours
        /// </summary>
        public SteeringBehaviours SteeringBehaviours
        {
            get { return _steeringBehaviours; }
        }

        #endregion

        #region Constructors

        static MovingGameEntity()
        {
            _movingGameEntitiesList=new List<MovingGameEntity>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <param name="model"></param>
        /// <param name="graphicsDevice"></param>
        public MovingGameEntity(Vector3 position, Vector3 rotation, Vector3 scale, Model model, GraphicsDevice graphicsDevice) : base(position, rotation, scale, model, graphicsDevice)
        {
            _steeringBehaviours=new SteeringBehaviours(this);
            _movingGameEntitiesList.Add(this);
        }

        #endregion

        #region Methods
        /// <summary>
        /// Ereases the moving game entity from list
        /// </summary>
        public void Kill()
        {
            _movingGameEntitiesList.Remove(this);
        }
        /// <summary>
        /// It will randomize  the properties like position, scale 
        /// </summary>
        public void RandomizeProperties(float posMin, float posMax, float scaleMin, float scaleMax)
        {
            this.Rotation = Vector3.Zero;
            Position = Generators.RandomVector3(posMin, posMax);
            Scale = Vector3.One * Generators.RandomNumber(scaleMin, scaleMax);
        }
        /// <summary>
        /// Default update for Moving entity , it not overriden then it calculates the force from steeringBehaviours and updates acordingly
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            Vector3 steeringForce = _steeringBehaviours.Calculate();
            UpdateMovement(steeringForce);
        }

        private void UpdateMovement(Vector3 force)
        {
            Vector3 steeringForce = force;
            //CurrentForce = steeringForce;
            Vector3 acceleration = steeringForce / this.Mass;

            this.Velocity += acceleration;//orig * gametime;
            if (acceleration == Vector3.Zero)
            {
                this.Velocity = Vector3.Zero;
            }

            float velLenght = this.Velocity.Length();
            if (velLenght > this.MaxSpeed)
            {
                this.Velocity = Vector3.Normalize(this.Velocity);
                this.Velocity *= this.MaxSpeed;
            }

            this.Position += this.Velocity;//orig * gametime
        }
        #endregion
    }
}