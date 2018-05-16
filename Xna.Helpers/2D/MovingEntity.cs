using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Xna.Helpers._2D
{
    /// <summary>
    /// Class for moving objects , inherits BaseGameEntity
    /// </summary>
   public class MovingEntity:BaseGameEntity
   {
        internal static List<MovingEntity> MovingEntities { get; set; }
      
       /// <summary>
       /// Returns current Moving  entities count 
       /// </summary>
        public static int MovingEntititiesCount { get { return MovingEntities.Count; } }
        /// <summary>
       /// Velocity of the entity
       /// </summary>
        public Vector2 Velocity { get; set; }
       /// <summary>
       /// Mass of the entity
       /// </summary>
        public float Mass { get; set; }
        /// <summary>
        /// Heading of the entity
        /// </summary>
        public Vector2 Heading { get; set; }
        /// <summary>
        /// MaxSpeed of the entity, exceding speed from entity behaviour will be trimmed to this
        /// </summary>
        public float MaxSpeed { get; set; }
        /// <summary>
        /// MaxTurnRate of the entity, not used yet!!!
        /// </summary>
        public float MaxTurnRate { get; set; }
        /// <summary>
        /// MaxForce of the entity, exceding force from entity behaviour will be trimmed to this
        /// </summary>
        public float MaxForce { get; set; }
        /// <summary>
        /// World where the entity resides
        /// </summary>
        public World World { get; set; }
        /// <summary>
        /// CurrentForce of the entity, used for debug
        /// </summary>
        public Vector2 CurrentForce { get; set; }
        /// <summary>
        /// SteeringBehaviours of the entity
        /// </summary>
        public SteeringBehaviours SteeringBehaviour { get; private set; }

       /// <summary>
       /// Perpedicunal vector to heading
       /// </summary>
        public Vector2 Side
        {
            get
            {
                var temp = new Vector2(-Heading.Y, Heading.X);
                return temp;
            }
        }

        static MovingEntity()
       {
           MovingEntities=new List<MovingEntity>();
       }

       /// <summary>
       /// Creates new moving entity 
       /// </summary>
        public MovingEntity()
        {
           SteeringBehaviour=new SteeringBehaviours(this);
           if (World.CurrentWorld != null) World = World.CurrentWorld;
           MovingEntities.Add(this);
        }
       /// <summary>
        /// Prevents Overlaps with others moving entities with parametars
       /// </summary>
       /// <param name="entity"></param>
       /// <param name="movingEntities"></param>
       protected void EnforceNonPenetrationConstraint(MovingEntity entity, List<MovingEntity> movingEntities)
       {
           for (int i = movingEntities.Count - 1; i >= 0; i--)
           {
               MovingEntity movingEntity = movingEntities[i];

               if (movingEntity == entity)
               {
                   continue;
               }

               Vector2 toEntity = entity.Position - movingEntity.Position;

               float distanceEntities = Vector2.Distance(entity.Position, movingEntity.Position);

               float amountOfOverLap = entity.Radius + movingEntity.Radius - distanceEntities;

               if (amountOfOverLap >= 0)
               {
                   //move the entity a distance away equivalent to the amount of overlap.
                   entity.Position = entity.Position + (toEntity / distanceEntities) * amountOfOverLap;
               }
           }
       }

       /// <summary>
       /// Prevents Overlaps with others moving entities
       /// </summary>
       protected void EnforceNonPenetrationConstraint()
       {
           for (int i = MovingEntities.Count - 1; i >= 0; i--)
           {
               MovingEntity movingEntity = MovingEntities[i];
               
               if (movingEntity == this)
               {
                   continue;
               }

               Vector2 toEntity = this.Position - movingEntity.Position;

               float distanceEntities = Vector2.Distance(this.Position, movingEntity.Position);

               float amountOfOverLap = this.Radius + movingEntity.Radius - distanceEntities;

               if (amountOfOverLap >= 0)
               {
                   //move the entity a distance away equivalent to the amount of overlap.
                  this.Position = this.Position + (toEntity / distanceEntities) * amountOfOverLap;
                   
               }
           }
       }
    }
}
