#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

#endregion

namespace Xna.Helpers._3D.Entity
{
    /// <summary>
    ///   Contains varius steering behaviours
    /// </summary>
    public class SteeringBehaviours
    {
        #region Enums

        /// <summary>
        ///   Used For Arrive behaviour
        /// </summary>
        public enum Deacceleration
        {
            /// <summary>
            ///   Slow arrival
            /// </summary>
            Slow = 3,
            /// <summary>
            ///   Normal speed arrival
            /// </summary>
            Normal = 2,
            /// <summary>
            ///   Fast arrival
            /// </summary>
            Fast = 1
        } ;

        #endregion

        #region Fields

        private bool _wanderOn,
                     _seekOn,
                     _fleeOn,
                     _arriveOn,
                     _pursuitOn,
                     _evadeOn,
                     _wallCollisionOn,
                     _obstacleCollisionOn,_cohesion,_alingment,_separation;

        private Vector3 _seekTarget, _fleeTarget, _arriveTarget;
        private MovingGameEntity _pursuer, _evader;
        private Deacceleration _deacceleration;
        private float _obstacleDetectionBoxLenght;
        private readonly MovingGameEntity _actor;
        private readonly List<Vector3> _feelers;
        //private readonly MovingHelper _movingHelper;
        private readonly List<BaseGameEntity> _taggedObstacles;
        private readonly List<MovingGameEntity> _taggedNeighbors;
        private static readonly List<object> _debugParametars;

        #endregion

        #region Properties
        /// <summary>
        ///   Helpers for debuging , can contain various objects, can only be filled from library
        /// </summary>
        public static ReadOnlyCollection<object> DebugParametars
        {
            get { return _debugParametars.AsReadOnly(); }
        }
        /// <summary>
        ///   The length of the "feelers" that detect walls,  origin is the textures origin
        /// </summary>
        public float WallDetectionLenght { get; set; }
        /// <summary>
        ///   The radius of circle where the random point will be generated and then followed
        /// </summary>
        public float WanderRadius { get; set; }
        /// <summary>
        ///   The distance of the fictional circle from the actor
        /// </summary>
        public float WanderDistance { get; set; }
        /// <summary>
        ///   How many calculations can occur , not tested
        /// </summary>
        public float WanderJitter { get; set; }
        /// <summary>
        ///   The strenght of the wallavoidance , the force accumulated from this behaviour is multiplied by this number
        /// </summary>
        public float WallWeightCollision { get; set; }
        /// <summary>
        /// The force accumulated from this behaviour is multiplied by this number
        /// </summary>
        public float CohesionWeight { get; set; }
        /// <summary>
        /// The force accumulated from this behaviour is multiplied by this number
        /// </summary>
        public float AlignmentWeight { get; set; }
        /// <summary>
        /// The force accumulated from this behaviour is multiplied by this number
        /// </summary>
        public float SeparationWeight { get; set; }
        /// <summary>
        /// Used for separation, cohesion and alignment 
        /// </summary>
        public float ViewDistance { get; set; }
        #endregion

        #region Constructors

        static SteeringBehaviours()
        {
            _debugParametars = new List<object>();
        }

        internal SteeringBehaviours(MovingGameEntity movingEntity)
        {
          //  _movingHelper = new MovingHelper();
            _feelers = new List<Vector3>();
            _actor = movingEntity;
            _taggedNeighbors = new List<MovingGameEntity>();
            _taggedObstacles = new List<BaseGameEntity>();
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Calculates the force from all behaviours that are turned on
        /// </summary>
        /// <returns></returns>
        public Vector3 Calculate()
        {
            //clear static lists
            _debugParametars.Clear();
            _feelers.Clear();
            _actor.Velocity += Vector3.Zero;
            if (_separation||_cohesion||_alingment||_wanderOn)
            {
                TagNeighbors(_actor,ViewDistance);
            }
            //reset the force.
            var steeringForce = Vector3.Zero;
            Vector3 force;
           
            //if (_wallCollisionOn)
            //{
            //    force = WallAvoidance(_actor.World.Walls)*WallWeightCollision;
            //    if (!AccumulateForce(ref steeringForce, force))
            //        return steeringForce;
            //}
            if (_separation)
            {
                force = Separation()*SeparationWeight;
                if (!AccumulateForce(ref steeringForce, force))
                    return steeringForce;
            }

            if (_alingment)
            {
                force = Alignment()*AlignmentWeight;
                if (!AccumulateForce(ref steeringForce, force))
                    return steeringForce;
            }
         
            if (_cohesion)
            {
                force = Cohesion() *CohesionWeight;
                if (!AccumulateForce(ref steeringForce, force))
                    return steeringForce;
            }
            if (_seekOn)
            {
                force = Seek(_seekTarget);
                if (!AccumulateForce(ref steeringForce, force))
                    return steeringForce;
            }
          
            //if (_obstacleCollisionOn)
            //{
            //    force = ObstacleAvoidance();
            //    if (!AccumulateForce(ref steeringForce, force))
            //        return steeringForce;
            //}
            if (_fleeOn)
            {
                force = Flee(_fleeTarget);
                if (!AccumulateForce(ref steeringForce, force))
                    return steeringForce;
            }
            if (_arriveOn)
            {
                force = Arrive(_arriveTarget, _deacceleration);
                if (!AccumulateForce(ref steeringForce, force))
                    return steeringForce;
            }
            if (_pursuitOn)
            {
                force = Pursuit(_evader);
                if (!AccumulateForce(ref steeringForce, force))
                    return steeringForce;
            }
            if (_evadeOn)
            {
                force = Evade(_pursuer);
                if (!AccumulateForce(ref steeringForce, force))
                    return steeringForce;
            }
            if (_wanderOn)
            {
                force = Wander();
                if (!AccumulateForce(ref steeringForce, force))
                    return steeringForce;
            }
            return steeringForce;
        }

        #region Behaviours
        
        private Vector3 Seek(Vector3 targetPos)
        {
            Vector3 desiredVelocity = Vector3.Normalize(Vector3.Subtract(targetPos, _actor.Position))*
                                      _actor.MaxSpeed;
            return (desiredVelocity - _actor.Velocity);
        }
        
        private Vector3 Flee(Vector3 targetPos)
        {
            Vector3 desiredVelocity = Vector3.Normalize(Vector3.Subtract(_actor.Position, targetPos))*_actor.MaxSpeed;
            return (desiredVelocity - _actor.Velocity);
        }
    
        private Vector3 Wander()
        {
            // The target position the entity tries to wander to. This position is constrained in   
            // a circle around the entity  
            Vector3 wanderTarget = Vector3.Zero;
           Vector3 randomVector=Generators.RandomVector3(-1f,1f);
            //File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\LogXna.txt",
            //                   Environment.NewLine + " Random :  " + x + "  " + y);
            Vector3 targetPosition = Vector3.Zero;
            // first, add a small random vector to the target's position  
            wanderTarget += randomVector;
            // reproject this new vector back onto a unit circle 
            if (wanderTarget.Length()>0.00001f)
            {
                wanderTarget.Normalize();
            }
            
            // increase the length of the vector to the same as the radius  
            // of the wander circle  
            wanderTarget *= WanderRadius;
            // move the target into a position wanderDistance in front of the agent  
            Vector3 displacement = Vector3.Zero;
            Vector3 rotation = _actor.Rotation;
            if (rotation.Length()>0.000001f)
            {
                rotation.Normalize();
            }
            displacement = rotation*WanderDistance;
            // project it in front of the entity and transform to world coordinates  
            targetPosition = _actor.Position + wanderTarget + displacement;
            // and steer toward it  
            return (targetPosition - _actor.Position);
        }
       
        private Vector3 Arrive(Vector3 targetPos, Deacceleration deceleration)
        {
            Vector3 ToTarget = targetPos - _actor.Position;
            //calculate the distance to the target position
            float dist = ToTarget.Length();
            if (dist > 0)
            {
                //because Deceleration is enumerated as an int, this value is required
                //to provide fine tweaking of the deceleration.
                const float DecelerationTweaker = 0.3f;
                //calculate the speed required to reach the target given the desired
                //deceleration
                float speed = dist/((float) deceleration*DecelerationTweaker);
                //make sure the velocity does not exceed the max
                speed = Math.Min(speed, _actor.MaxSpeed);
                //from here proceed just like Seek except we don't need to normalize
                //the ToTarget vector because we have already gone to the trouble
                //of calculating its length: dist.
                Vector3 DesiredVelocity = ToTarget*speed/dist;
                return (DesiredVelocity - _actor.Velocity);
            }

            return Vector3.Zero;
        }

        private Vector3 Pursuit(MovingGameEntity evader)
        {
            //if the evader is ahead and facing the agent then we can just seek
            //for the evader's current position.
            Vector3 ToEvader = evader.Position - _actor.Position;
            var relativeHeading = Vector3.Dot(evader.Rotation, _actor.Rotation);
            if ((Vector3.Dot(ToEvader, _actor.Rotation) > 0) && (relativeHeading < -0.95)) //acos(0.95)=18 degs
            {
                return Seek(evader.Position);
            }
            //Not considered ahead so we predict where the evader will be.
            //the look-ahead time is proportional to the distance between the evader
            //and the pursuer; and is inversely proportional to the sum of the
            //agents' velocities
            var lookAheadTime = ToEvader.Length()/(_actor.MaxSpeed + evader.MaxSpeed); //todo:provjera 
            lookAheadTime += TurnAroundTime(_actor, evader.Position); //todo:provjera
            //now seek to the predicted future position of the evader
            return Seek(evader.Position + evader.Velocity*lookAheadTime);
        }

        private Vector3 Evade(MovingGameEntity pursuer)
        {
            /* Not necessary to include the check for facing direction this time */
            Vector3 ToPursuer = pursuer.Position - _actor.Position;
            //the look-ahead time is proportional to the distance between the pursuer
            //and the evader; and is inversely proportional to the sum of the
            //agents' velocities
            var LookAheadTime = ToPursuer.Length()/(_actor.MaxSpeed + pursuer.MaxSpeed); //todo:provjera 
            //now flee away from predicted future position of the pursuer
            return Flee(pursuer.Position + pursuer.Velocity*LookAheadTime);
        }
    
        private Vector3 Separation()
        {
            Vector3 steeringForce = new Vector3();
            for (int index = _taggedNeighbors.Count - 1; index >= 0; index--)
            {
                //make sure this agent isn't included in the calculations and that
                //the agent being examined is close enough.
                Vector3 toAgent = _actor.Position - _taggedNeighbors[index].Position;

                //scale the force inversely proportional to the agent's distance
                //from its neighbor.
                steeringForce += Vector3.Normalize(toAgent)/toAgent.Length();
            }
            return steeringForce;
        }
     
        private Vector3 Alignment()
        {
            //used to record the average heading of the neighbors
            Vector3 averageHeading = new Vector3();
            for (int index = _taggedNeighbors.Count - 1; index >= 0; index--)
            {
                averageHeading += _taggedNeighbors[index].Rotation;
            }
            //if the neighborhood contained one or more vehicles, average their
            //heading vectors.

            if (_taggedNeighbors.Count != 0)
            {
                averageHeading /= _taggedNeighbors.Count;

                averageHeading -= _actor.Rotation;
            }
            return averageHeading;
        }
        
        private Vector3 Cohesion()
        {
            //first find the center of mass of all the agents
            var centerOfMass = new Vector3();
            var steeringForce = new Vector3();
            //iterate through the neighbors and sum up all the position vectors
            //make sure *this* agent isn't included in the calculations and that
            //the agent being examined is a neighbor
            for (int index = _taggedNeighbors.Count - 1; index >= 0; index--)
            {
                centerOfMass += _taggedNeighbors[index].Position;
            }
            //the center of mass is the average of the sum of positions
            centerOfMass /= _taggedNeighbors.Count;
            //now seek toward that position
            steeringForce = Seek(centerOfMass);
            return steeringForce;
        }



        /// <summary>
        /// Prevents Overlaps with others moving entities
        /// </summary>
        public void EnforceNonPenetrationConstraint()
        {
            for (int i = _taggedNeighbors.Count - 1; i >= 0; i--)
            {
                MovingGameEntity movingEntity = _taggedNeighbors[i];
               

                Vector3 toEntity = _actor.Position - movingEntity.Position;

                float distanceEntities = Vector3.Distance(_actor.Position, movingEntity.Position);

                float amountOfOverLap = _actor.BoundingSphere.Radius + movingEntity.BoundingSphere.Radius - distanceEntities;

                if (amountOfOverLap >= 0)
                {
                    //move the entity a distance away equivalent to the amount of overlap.
                    _actor.Position = _actor.Position + (toEntity / distanceEntities) * amountOfOverLap;

                }
            }
        }
        /// <summary>
        /// Prevents Overlaps with others moving entities
        /// </summary>
        public void EnforceNonPenetrationConstraint(List<MovingGameEntity> gameEntities)
        {
            for (int i = gameEntities.Count - 1; i >= 0; i--)
            {
                MovingGameEntity movingEntity = gameEntities[i];
                if (movingEntity==_actor)
                {
                    continue;
                }

                Vector3 toEntity = _actor.Position - movingEntity.Position;

                float distanceEntities = Vector3.Distance(_actor.Position, movingEntity.Position);

                float amountOfOverLap = _actor.BoundingSphere.Radius + movingEntity.BoundingSphere.Radius - distanceEntities;

                if (amountOfOverLap >= 0)
                {
                    //move the entity a distance away equivalent to the amount of overlap.
                    _actor.Position = _actor.Position + (toEntity / distanceEntities) * amountOfOverLap;

                }
            }
        }
        //Interpose

        //Hide

        //Path Following

        //Offset Pursuit


        //Group Behaviours

        #endregion

        #region Private Helpers




        private bool AccumulateForce(ref Vector3 runningTot, Vector3 forceToAdd)
        {
            //calculate how much steering force the vehicle has used so far
            float magnitudeSoFar = runningTot.Length();

            //calculate how much steering force remains to be used by this vehicle
            float magnitudeRemaining = _actor.MaxForce - magnitudeSoFar;

            //return false if there is no more force left to use
            if (magnitudeRemaining <= 0.0)
                return false;

            //calculate the magnitude of the force we want to add
            float magnitudeToAdd = forceToAdd.Length();

            //if the magnitude of the sum of ForceToAdd and the running total
            //does not exceed the maximum force available to this vehicle, just
            //add together. Otherwise add as much of the ForceToAdd vector as
            //possible without going over the max.
            if (magnitudeToAdd < magnitudeRemaining)
            {
                runningTot += forceToAdd;
            }

            else
            {
                //add it to the steering force
                forceToAdd.Normalize();
                runningTot += (forceToAdd*magnitudeRemaining);
            }

            return true;
        }

        private float TurnAroundTime(MovingGameEntity pAgent, Vector3 targetPos)
        {
            //determine the normalized vector to the target
            Vector3 toTarget = targetPos - pAgent.Position;
            toTarget.Normalize();
            var dot = Vector3.Dot(pAgent.Rotation, toTarget);

            //change this value to get the desired behavior. The higher the max turn
            //rate of the vehicle, the higher this value should be. If the vehicle is
            //heading in the opposite direction to its target position then a value
            //of 0.5 means that this function will return a time of 1 second for the
            //vehicle to turn around.
            const float coefficient = 0.5f;

            //the dot product gives a value of 1 if the target is directly ahead and -1
            //if it is directly behind. Subtracting 1 and multiplying by the negative of
            //the coefficient gives a positive value proportional to the rotational
            //displacement of the vehicle and target.
            return (dot - 1.0f)*-coefficient;
        }

      

        private void TagNeighbors(BaseGameEntity baseGameEntity, float radius)
        {
            _taggedNeighbors.Clear();
            for (int index = MovingGameEntity.MovingGameEntities.Count - 1; index >= 0; index--)
            {
                var neighbor = MovingGameEntity.MovingGameEntities[index];

                if (neighbor == baseGameEntity) continue;

                Vector3 vectorTo = neighbor.Position - baseGameEntity.Position;

                float range = radius + neighbor.BoundingSphere.Radius; //Todo:provjeritiii da li j edobro zato sto range nije dobar

                var rangeMult = range*range;
                var vectorLenght = vectorTo.Length();

                if (vectorLenght*vectorLenght < rangeMult) _taggedNeighbors.Add(neighbor);
                
            }
        }
    

        #endregion

        #region Tooglers

        /// <summary>
        ///   Turns wander behaviour on
        /// </summary>
        public void WanderOn()
        {
            _wanderOn = true;
        }

        /// <summary>
        ///   Turns wander behaviour off
        /// </summary>
        public void WanderOff()
        {
            _wanderOn = false;
        }

        /// <summary>
        ///   Turns wallcollision on
        /// </summary>
        public void WallColisionOn()
        {
            _wallCollisionOn = true;
        }

        /// <summary>
        ///   Turns wallcollision off
        /// </summary>
        public void WallCollisionOff()
        {
            _wallCollisionOn = false;
        }

        /// <summary>
        /// </summary>
        /// <param name = "seekTarget">The position of the target</param>
        public void SeekOn(Vector3 seekTarget)
        {
            _seekTarget = seekTarget;
            _seekOn = true;
        }

        /// <summary>
        ///   Turns seek behaviour off
        /// </summary>
        public void SeekOff()
        {
            _seekOn = false;
        }

        /// <summary>
        ///   Turns flee behaviour on
        /// </summary>
        /// <param name = "fleeTarget">The position of the seeker</param>
        public void FleeOn(Vector3 fleeTarget)
        {
            _fleeTarget = fleeTarget;
            _fleeOn = true;
        }

        /// <summary>
        ///   Turns flee behaviour off
        /// </summary>
        public void FleeOff()
        {
            _fleeOn = false;
        }

        /// <summary>
        ///   Turns arrival behaviour on
        /// </summary>
        /// <param name = "arriveTarget">The destination of arrival</param>
        /// <param name = "deacceleration">The speed of arrival</param>
        public void ArriveOn(Vector3 arriveTarget, Deacceleration deacceleration)
        {
            _arriveTarget = arriveTarget;
            _deacceleration = deacceleration;
            _arriveOn = true;
        }

        /// <summary>
        ///   Turns arrival behaviour off
        /// </summary>
        public void ArriveOff()
        {
            _arriveOn = false;
        }

        /// <summary>
        ///   Turns pursuit behaviour on
        /// </summary>
        /// <param name = "evader">The entity whitch we are pursuing</param>
        public void PursuitOn(MovingGameEntity evader)
        {
            _evader = evader;
            _pursuitOn = true;
        }

        /// <summary>
        ///   Turns pursuit behaviour off
        /// </summary>
        /// <param name = "evader"></param>
        public void PursuitOff(MovingGameEntity evader)
        {
            _pursuitOn = false;
        }

        /// <summary>
        ///   Turns evade behaviour on
        /// </summary>
        /// <param name = "pursuer">The entity witch we are evading</param>
        public void EvadeOn(MovingGameEntity pursuer)
        {
            _pursuer = pursuer;
            _evadeOn = true;
        }

        /// <summary>
        ///   Turns evade behaviour off
        /// </summary>
        /// <param name = "pursuer"></param>
        public void EvadeOff(MovingGameEntity pursuer)
        {
            _evadeOn = false;
        }

        /// <summary>
        ///   Turns obstacle collision behaviour on
        /// </summary>
        public void ObstacleCollisionOn()
        {
            _obstacleCollisionOn = true;
        }

        /// <summary>
        ///   Turns obstacle collision behaviour off
        /// </summary>
        public void ObstacleCollisionOff()
        {
            _obstacleCollisionOn = false;
        }

        /// <summary>
        /// Turns cohesion behaviour on, moving entities gather in center of the mass
        /// </summary>
        public void CohesionOn()
        {
            _cohesion = true;
        }
        /// <summary>
        /// Turns cohesion behavior off
        /// </summary>
        public void CohesionOff()
        {
            _cohesion = false;
        }
        /// <summary>
        /// Turns alignment behaviour on, forces the average heading between moving entities
        /// </summary>
        public void AlignmentOn()
        {
            _alingment = true;
        }
        /// <summary>
        /// Turns alignment behaviour off
        /// </summary>
        public void AlignmentOff()
        {
            _alingment = false;
        }
        /// <summary>
        /// Turns separation behaviour on, separates the moving entities
        /// </summary>
        public void SeparationOn()
        {
            _separation = true;
        }
        /// <summary>
        /// Turns separation behaviour off
        /// </summary>
        public void SeparationOff()
        {
            _separation = false;
        }


        #endregion
 
        #endregion
    }
}