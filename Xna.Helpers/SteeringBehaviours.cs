#region

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

#endregion

namespace Xna.Helpers
{
    /// <summary>
    ///   Contains varius steering behaviours
    /// </summary>
    public class SteeringBehaviours
    {
        #region Enums

        public enum Deceleration
        {
            Slow = 3,
            Normal = 2,
            Fast = 1
        } ;

        #endregion

        #region Fields

        private bool _wanderOn, _seekOn, _fleeOn, _arriveOn, _pursuitOn, _evadeOn,_wallCollisionOn;
        private readonly MovingEntity _actor;
        private readonly List<Vector2> _feelers;
        private readonly MovingHelper _movingHelper;
        private float _wallWeightCollision;
        #endregion

        #region Properties

        public float WallDetectionLenght { get; set; }

        public float WanderRadius { get; set; }

        public float WanderDistance { get; set; }

        public float WanderJitter { get; set; }

        public Vector2 TargetPosWander { get; set; }

        public bool WanderOn
        {
            get { return _wanderOn; }
            set { _wanderOn = value; }
        }

        public bool SeekOn
        {
            get { return _seekOn; }
            set { _seekOn = value; }
        }

        public bool FleeOn
        {
            get { return _fleeOn; }
            set { _fleeOn = value; }
        }

        public bool ArriveOn
        {
            get { return _arriveOn; }
            set { _arriveOn = value; }
        }

        public bool PursuitOn
        {
            get { return _pursuitOn; }
            set { _pursuitOn = value; }
        }

        public bool EvadeOn
        {
            get { return _evadeOn; }
            set { _evadeOn = value; }
        }

        public bool WallCollisionOn
        {
            get { return _wallCollisionOn; }
            set { _wallCollisionOn = value; }
        }

        #endregion

        #region Constructors

        public SteeringBehaviours(MovingEntity movingEntity)
        {
            _wallWeightCollision = 10f;
            _movingHelper = new MovingHelper();
            _feelers = new List<Vector2>();
            WanderDistance = 3f;
            WanderRadius = 1.7f;
            WanderJitter = 1f;
            WallDetectionLenght = 50f;
            _actor = movingEntity;
        }

        #endregion

        #region Methods

        //Seek
        public Vector2 Seek(Vector2 targetPos)
        {
            Vector2 desiredVelocity = Vector2.Normalize(Vector2.Subtract(targetPos, _actor.Position))*
                                      _actor.MaxSpeed;
            return (desiredVelocity - _actor.Velocity);
        }

        //Flee
        public Vector2 Flee(Vector2 targetPos)
        {
            Vector2 desiredVelocity = Vector2.Normalize(Vector2.Subtract(_actor.Position, targetPos))*_actor.MaxSpeed;
            return (desiredVelocity - _actor.Velocity);
        }

        //Wander
        public Vector2 Wander()
        {
            // The target position the entity tries to wander to. This position is constrained in   
            // a circle around the entity  

            Vector2 wanderTarget = Vector2.Zero;
            float x = Generators.RandomNumber(-1f, 1f)*WanderJitter;
            float y = Generators.RandomNumber(-1f, 1f)*WanderJitter;
            File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\LogXna.txt",
                               Environment.NewLine + " Random :  " + x + "  " + y);
            Vector2 targetPosition = Vector2.Zero;

            // first, add a small random vector to the target's position  
            wanderTarget += new Vector2(x, y);

            // reproject this new vector back onto a unit circle  
            wanderTarget.Normalize();

            // increase the length of the vector to the same as the radius  
            // of the wander circle  
            wanderTarget *= WanderRadius;

            // move the target into a position wanderDistance in front of the agent  
            Vector2 displacement = Vector2.Zero;

            displacement = Vector2.Normalize(_actor.Heading)*WanderDistance;

            // project it in front of the entity and transform to world coordinates  
            targetPosition = _actor.Position + wanderTarget + displacement;

            // and steer toward it  
            return (targetPosition - _actor.Position);
        }

        //Arrive
        public Vector2 Arrive(Vector2 targetPos, Deceleration deceleration)
        {
            Vector2 ToTarget = targetPos - _actor.Position;

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
                Vector2 DesiredVelocity = ToTarget*speed/dist;

                return (DesiredVelocity - _actor.Velocity);
            }

            return Vector2.Zero;
        }

        //Pursui
        public Vector2 Pursuit(MovingEntity evader)
        {
            //if the evader is ahead and facing the agent then we can just seek
            //for the evader's current position.
            Vector2 ToEvader = evader.Position - _actor.Position;

            var relativeHeading = Vector2.Dot(evader.Heading, _actor.Heading);

            if ((Vector2.Dot(ToEvader, _actor.Heading) > 0) && (relativeHeading < -0.95)) //acos(0.95)=18 degs
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

        //Evade
        public Vector2 Evade(MovingEntity pursuer)
        {
            /* Not necessary to include the check for facing direction this time */

            Vector2 ToPursuer = pursuer.Position - _actor.Position;

            //the look-ahead time is proportional to the distance between the pursuer
            //and the evader; and is inversely proportional to the sum of the
            //agents' velocities
            var LookAheadTime = ToPursuer.Length()/(_actor.MaxSpeed + pursuer.MaxSpeed); //todo:provjera 

            //now flee away from predicted future position of the pursuer
            return Flee(pursuer.Position + pursuer.Velocity*LookAheadTime);
        }

        //ObstacleAvoidance
        //public void ObstacleAvoidance(){}//todo napravit obstacle avoidance ali za to treba napraviti world i listu svi h objekata u svijetu

        //WallAvoidance
        public Vector2 WallAvoidance(List<Wall> walls)
        {
            //the feelers are contained in a std::vector, m_Feelers
            CreateFeelers();

            float DistToThisIP = 0.0f;
            float DistToClosestIP = 60f;

            //this will hold an index into the vector of walls
            Wall ClosestWall=null; 
            Vector2 SteeringForce = new Vector2();
            //holds the closest intersection point - used for storing temporary info
            Vector2 point = new Vector2();
            Vector2 ClosestPoint = new Vector2();

            //examine each feeler in turn
            foreach (Vector2 feeler in _feelers)
            {
//run through each wall checking for any intersection points
                foreach (Wall wall in walls)
                {
                    if (LineIntersection2D(_actor.Position, feeler, wall.Start, wall.End,
                                           ref DistToThisIP, ref point))
                    {
                        //is this the closest found so far? If so keep a record
                        if (DistToThisIP < DistToClosestIP)
                        {
                            DistToClosestIP = DistToThisIP;

                            ClosestWall = wall;

                            ClosestPoint = point;
                        }
                    }
                } //next wall

                //if an intersection point has been detected, calculate a force
                //that will direct the agent away
                if (ClosestWall!=null)
                {
                    //calculate by what distance the projected position of the agent
                    //will overshoot the wall
                    Vector2 OverShoot = feeler - ClosestPoint;

                 

                    //create a force in the direction of the wall normal, with a
                    //magnitude of the overshoot   todo:promjenaaa 
                    float overShotLenght = OverShoot.Length();
                    SteeringForce = ClosestWall.Normal*overShotLenght;
                }
            }

            return SteeringForce;
        }


        //Interpose

        //Hide

        //Path Following

        //Offset Pursuit


        //Group Behaviours


        public Vector2 Calculate()
        {
            //reset the force.
            var steeringForce = Vector2.Zero;
            var force = new Vector2();


            if (WallCollisionOn)
            {
                force = WallAvoidance(_actor.World.Walls) * _wallWeightCollision;
                if (!AccumulateForce(ref steeringForce, force))
                    return steeringForce;
            }

            if (WanderOn)
            {
                force = Wander();
                  if (!AccumulateForce(ref steeringForce, force))
                return steeringForce;
            }
            //if (On(wall_avoidance))
            //{
            //    force = WallAvoidance(m_pVehicle.World().Walls()) * m_dMultWallAvoidance;

            //    if (!AccumulateForce(ref steeringForce, force))
            //        return steeringForce;
            //}

            //if (On(obstacle_avoidance))
            //{
            //    force = ObstacleAvoidance(m_pVehicle.World().Obstacles()) * m_dMultObstacleAvoidance;

            //    if (!AccumulateForce(ref steeringForce, force))
            //        return steeringForce;
            //}

            //if (On(separation))
            //{
            //    force = Separation(m_pVehicle.World().Agents()) * m_dMultSeparation;

            //    if (!AccumulateForce(ref steeringForce, force))
            //        return steeringForce;
            //}

            /* EXTRANEOUS STEERING FORCES OMITTED */
            return steeringForce;
        }

        #region Private

        private bool AccumulateForce(ref Vector2 runningTot, Vector2 forceToAdd)
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


        private float TurnAroundTime(MovingEntity pAgent, Vector2 TargetPos)
        {
            //determine the normalized vector to the target
            Vector2 toTarget = TargetPos - pAgent.Position;
            toTarget.Normalize();
            var dot = Vector2.Dot(pAgent.Heading, toTarget);

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

        private void CreateFeelers()
        {
            //feeler pointing straight in front
            _feelers.Add(_actor.Position + WallDetectionLenght*_actor.Heading);
            //feeler to left
            Vector2 temp = _actor.Heading;
            temp= _movingHelper.RotateAroundPoint(temp, (float) ((Math.PI/2)*3.5f));
            _feelers.Add(_actor.Position + WallDetectionLenght/1.0f*temp);

            //feeler to right
            temp = _actor.Heading;
            temp= _movingHelper.RotateAroundPoint(temp, (float) ((Math.PI/2)*0.5f));
            _feelers.Add(_actor.Position + WallDetectionLenght/1.0f*temp);
        }

        private bool LineIntersection2D(Vector2 A, Vector2 B, Vector2 C, Vector2 D, ref float dist, ref Vector2 point)
        {
            float rTop = (A.Y - C.Y)*(D.X - C.X) - (A.X - C.X)*(D.Y - C.Y);
            float rBot = (B.X - A.X)*(D.Y - C.Y) - (B.Y - A.Y)*(D.X - C.X);

            float sTop = (A.Y - C.Y)*(B.X - A.X) - (A.X - C.X)*(B.Y - A.Y);
            float sBot = (B.X - A.X)*(D.Y - C.Y) - (B.Y - A.Y)*(D.X - C.X);

            if ((rBot == 0) || (sBot == 0))
            {
                //lines are parallel
                return false;
            }

            float r = rTop/rBot;
            float s = sTop/sBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1))
            {
                dist = Vector2.Distance(A, B)*r;

                point = A + r*(B - A);

                return true;
            }

            else
            {
                dist = 0;

                return false;
            }
        }

        #endregion

        #endregion
    }
}