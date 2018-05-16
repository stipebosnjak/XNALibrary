#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

#endregion

namespace Xna.Helpers._2D
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

        private Vector2 _seekTarget, _fleeTarget, _arriveTarget;
        private MovingEntity _pursuer, _evader;
        private Deacceleration _deacceleration;
        private float _obstacleDetectionBoxLenght;
        private readonly MovingEntity _actor;
        private readonly List<Vector2> _feelers;
        private readonly MovingHelper _movingHelper;
        private readonly List<BaseGameEntity> _taggedObstacles;
        private readonly List<MovingEntity> _taggedNeighbors;
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

        internal SteeringBehaviours(MovingEntity movingEntity)
        {
            _movingHelper = new MovingHelper();
            _feelers = new List<Vector2>();
            _actor = movingEntity;
            _taggedNeighbors = new List<MovingEntity>();
            _taggedObstacles = new List<BaseGameEntity>();
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Calculates the force from all behaviours that are turned on
        /// </summary>
        /// <returns></returns>
        public Vector2 Calculate()
        {
            //clear static lists
            _debugParametars.Clear();
            _feelers.Clear();
            _actor.Velocity += Vector2.Zero;
            if (_separation||_cohesion||_alingment)
            {
                 TagNeighbors(_actor,ViewDistance);
            }
            //reset the force.
            var steeringForce = Vector2.Zero;
            Vector2 force;
           
            if (_wallCollisionOn)
            {
                force = WallAvoidance(_actor.World.Walls)*WallWeightCollision;
                if (!AccumulateForce(ref steeringForce, force))
                    return steeringForce;
            }
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
        
        private Vector2 Seek(Vector2 targetPos)
        {
            Vector2 desiredVelocity = Vector2.Normalize(Vector2.Subtract(targetPos, _actor.Position))*
                                      _actor.MaxSpeed;
            return (desiredVelocity - _actor.Velocity);
        }
        
        private Vector2 Flee(Vector2 targetPos)
        {
            Vector2 desiredVelocity = Vector2.Normalize(Vector2.Subtract(_actor.Position, targetPos))*_actor.MaxSpeed;
            return (desiredVelocity - _actor.Velocity);
        }
    
        private Vector2 Wander()
        {
            // The target position the entity tries to wander to. This position is constrained in   
            // a circle around the entity  
            Vector2 wanderTarget = Vector2.Zero;
            float x = Generators.RandomNumber(-1f, 1f)*WanderJitter;
            float y = Generators.RandomNumber(-1f, 1f)*WanderJitter;
            //File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\LogXna.txt",
            //                   Environment.NewLine + " Random :  " + x + "  " + y);
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
       
        private Vector2 Arrive(Vector2 targetPos, Deacceleration deceleration)
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

        private Vector2 Pursuit(MovingEntity evader)
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
        
        private Vector2 Evade(MovingEntity pursuer)
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
       
        private Vector2 ObstacleAvoidance()
        {
            float minDetectionBoxLenght = _actor.Radius*4f;
            //the detection box length is proportional to the agent's velocity
            _obstacleDetectionBoxLenght = minDetectionBoxLenght +
                                          ((_actor.Velocity/_actor.MaxSpeed).Length()*minDetectionBoxLenght);
            //tag all obstacles within range of the box for processing
            //   m_pVehicle.World().TagObstaclesWithinViewRange(m_pVehicle, m_dDBoxLength);
            TagObstacles(_actor, _obstacleDetectionBoxLenght);
            //this will keep track of the closest intersecting obstacle (CIB)
            BaseGameEntity closestIntersectingObstacle = null;
            //this will be used to track the distance to the CIB
            float distToClosestIP = 50000f;
            //this will record the transformed local coordinates of the CIB
            Vector2 localPosOfClosestObstacle = new Vector2();
            //foreach (var baseGameEntity in BaseGameEntity.BaseGameEntities)
            foreach (BaseGameEntity taggedEntity in _taggedObstacles)
            {
                // Vector2 localPos = PointToLocalSpace(baseGameEntity.Position, _actor.Heading, _actor.Position);
                Matrix matrix = Matrix.CreateTranslation(new Vector3(_actor.Position.X, _actor.Position.Y, 0));
                Matrix invert = Matrix.Invert(matrix);
                Vector2 localPos = Vector2.Transform(taggedEntity.Position, invert);
                //   Vector2 direction = _actor.Position - baseGameEntity.Position;
                //Vector2 localPos_unused = PointToLocalSpace(baseGameEntity.Position, _actor.Heading, _actor.Side,_actor.Position);
                // _debugParametars.Add(PointToWorldSpace(localPos, _actor.Heading, _actor.Side, _actor.Position));
                _debugParametars.Add((_obstacleDetectionBoxLenght*_actor.Heading) + _actor.Position);
                _debugParametars.Add(taggedEntity.Position);
                if (localPos.X >= 0)
                {
                    //if the distance from the x axis to the object's position is less
                    //than its radius + half the width of the detection box then there
                    //is a potential intersection.
                    float expandedRadius = taggedEntity.Radius + _actor.Radius;
                    if (Math.Abs(localPos.Y) < expandedRadius)
                    {
                        //now to do a line/circle intersection test. The center of the
                        //circle is represented by (cX, cY). The intersection points are
                        //given by the formulax=cX +/-sqrt(r^2-cY^2) for y=0.
                        //We only need to look at the smallest positive value of x because
                        //that will be the closest point of intersection.
                        float cX = localPos.X;
                        float cY = localPos.Y;
                        //we only need to calculate the sqrt part of the above equation once
                        float sqrtPart = (float) Math.Sqrt((expandedRadius*expandedRadius) - (cY*cY));
                        float ip = cX - sqrtPart;
                        if (ip <= 0)
                        {
                            ip = cX + sqrtPart;
                        }
                        //test to see if this is the closest so far. If it is, keep a
                        //record of the obstacle and its local coordinates
                        if (ip < distToClosestIP)
                        {
                            distToClosestIP = ip;
                            closestIntersectingObstacle = taggedEntity;
                            localPosOfClosestObstacle = localPos;
                        }
                    }
                }
            }
            var steeringForce = new Vector2();
            if (closestIntersectingObstacle != null)
            {
                //the closer the agent is to an object, the stronger the steering force
                //should be
                float multiplier = (float) 1.0 +
                                   ((_obstacleDetectionBoxLenght - localPosOfClosestObstacle.X)/
                                    _obstacleDetectionBoxLenght);
                //calculate the lateral force
                steeringForce.Y = (closestIntersectingObstacle.Radius - localPosOfClosestObstacle.Y)*multiplier;
                //apply a braking force proportional to the obstacle's distance from
                //the vehicle.
                const float brakingWeight = 0.1f;//
                steeringForce.X = ((closestIntersectingObstacle.Radius - localPosOfClosestObstacle.X)*brakingWeight);
            }
            //finally, convert the steering vector from local to world space
            //  return VectorToWorldSpace(SteeringForce, m_pVehicle.Heading(), m_pVehicle.Side());

            //Matrix matrix1=new Matrix();
            //Matrix.CreateRotationZ()
            //worldVector = Vector2.Transform(localVector, entityWorldOrient);
            Vector2 steeringVector;
            var angle = VectorToAngle(_actor.Heading);
            angle = MathHelper.WrapAngle(angle);
            Matrix matrixR = Matrix.CreateRotationZ(angle);
            steeringVector = Vector2.Transform(steeringForce, matrixR);
            var steeringVector1 = VectorToWorldSpace(steeringForce, _actor.Heading, _actor.Side);
            _debugParametars.Add(_actor.Position + steeringVector);
            return steeringVector;
        }
        
        private Vector2 WallAvoidance(ReadOnlyCollection<Wall> walls)
        {
            //the feelers are contained in a std::vector, m_Feelers
            CreateFeelers();
            float distToThisIp = 0.0f;
            float distToClosestIp = 1000f;
            //this will hold an index into the vector of walls
            Wall closestWall = null;
            Vector2 steeringForce = new Vector2();
            //holds the closest intersection point - used for storing temporary info
            Vector2 point = new Vector2();
            Vector2 closestPoint = new Vector2();
            //examine each feeler in turn
            for (int i = _feelers.Count - 1; i >= 0; i--)
            {
                Vector2 feeler = _feelers[i];
                //run through each wall checking for any intersection points
                for (int index = walls.Count - 1; index >= 0; index--)
                {
                    Wall wall = walls[index];
                    if (LineIntersection2D(_actor.Position, feeler, wall.Start, wall.End,
                                           ref distToThisIp, ref point))
                    {
                        //is this the closest found so far? If so keep a record
                        if (distToThisIp < distToClosestIp)
                        {
                            distToClosestIp = distToThisIp;
                            closestWall = wall;
                            closestPoint = point;
                        }
                    }
                }
                //if an intersection point has been detected, calculate a force
                //that will direct the agent away
                if (closestWall != null)
                {
                    //calculate by what distance the projected position of the agent
                    //will overshoot the wall
                    Vector2 OverShoot = feeler - closestPoint;
                    //
                    //create a force in the direction of the wall normal, with a
                    //magnitude of the overshoot   todo:promjenaaa 
                    float overShotLenght = OverShoot.Length();
                    steeringForce = closestWall.Normal*overShotLenght;
                }
            }

            return steeringForce;
        }
    
        private Vector2 Separation()
        {
            Vector2 steeringForce = new Vector2();
            for (int index = _taggedNeighbors.Count - 1; index >= 0; index--)
            {
                //make sure this agent isn't included in the calculations and that
                //the agent being examined is close enough.
                Vector2 toAgent = _actor.Position - _taggedNeighbors[index].Position;

                //scale the force inversely proportional to the agent's distance
                //from its neighbor.
                steeringForce += Vector2.Normalize(toAgent)/toAgent.Length();
            }
            return steeringForce;
        }
     
        private Vector2 Alignment()
        {
            //used to record the average heading of the neighbors
            Vector2 averageHeading = new Vector2();
            for (int index = _taggedNeighbors.Count - 1; index >= 0; index--)
            {
                averageHeading += _taggedNeighbors[index].Heading;
            }
            //if the neighborhood contained one or more vehicles, average their
            //heading vectors.

            if (_taggedNeighbors.Count != 0)
            {
                averageHeading /= _taggedNeighbors.Count;

                averageHeading -= _actor.Heading;
            }
            return averageHeading;
        }
        
        private Vector2 Cohesion()
        {
            //first find the center of mass of all the agents
            var centerOfMass = new Vector2();
            var steeringForce = new Vector2();
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

        //Interpose

        //Hide

        //Path Following

        //Offset Pursuit


        //Group Behaviours

        #endregion

        #region Private Helpers

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

        private float TurnAroundTime(MovingEntity pAgent, Vector2 targetPos)
        {
            //determine the normalized vector to the target
            Vector2 toTarget = targetPos - pAgent.Position;
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
            float feelersAngle = 60, trimSideFeelers = 2f;

            //feeler pointing straight in front
            Vector2 feeler = _actor.Position + WallDetectionLenght*_actor.Heading;
            _feelers.Add(feeler);
            _debugParametars.Add(feeler);

            //feeler to left
            Vector2 temp = _actor.Heading;
            temp = Rotation.RotateAroundPoint(temp, MathHelper.ToRadians((feelersAngle)));
            // (float) ((Math.PI/2)*3.5f));
            feeler = _actor.Position + WallDetectionLenght/trimSideFeelers*temp;
            _feelers.Add(feeler);
            _debugParametars.Add(feeler);

            //feeler to right
            temp = _actor.Heading;
            temp = Rotation.RotateAroundPoint(temp, MathHelper.ToRadians(((-feelersAngle))));
            // (float) ((Math.PI/2)*0.5f));
            feeler = _actor.Position + WallDetectionLenght/trimSideFeelers*temp;
            _feelers.Add(feeler);
            _debugParametars.Add(feeler);
        }

        private bool LineIntersection2D(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref float dist, ref Vector2 point)
        {
            float rTop = (a.Y - c.Y)*(d.X - c.X) - (a.X - c.X)*(d.Y - c.Y);
            float rBot = (b.X - a.X)*(d.Y - c.Y) - (b.Y - a.Y)*(d.X - c.X);

            float sTop = (a.Y - c.Y)*(b.X - a.X) - (a.X - c.X)*(b.Y - a.Y);
            float sBot = (b.X - a.X)*(d.Y - c.Y) - (b.Y - a.Y)*(d.X - c.X);

            if ((rBot == 0) || (sBot == 0))
            {
                //lines are parallel
                return false;
            }

            float r = rTop/rBot;
            float s = sTop/sBot;

            if ((r > 0) && (r < 1) && (s > 0) && (s < 1))
            {
                dist = Vector2.Distance(a, b)*r;

                point = a + r*(b - a);

                return true;
            }

            else
            {
                dist = 0;

                return false;
            }
        }


        private void TagObstacles(BaseGameEntity baseGameEntity, float radius)
        {
            _taggedObstacles.Clear();
            for (int index = _actor.World.Obstacles.Count - 1; index >= 0; index--)
            {
                var neighbor = _actor.World.Obstacles[index];
                if (neighbor is MovingEntity || neighbor == baseGameEntity) continue;

                //neighbor.Tagged = false;

                Vector2 vectorTo = neighbor.Position - baseGameEntity.Position;

                float range = radius + neighbor.Radius; //Todo:provjeritiii da li j edobro zato sto range nije dobar

                var rangeMult = range*range;
                var vectorLenght = vectorTo.Length();

                if (vectorLenght*vectorLenght < rangeMult) _taggedObstacles.Add(neighbor);
                // neighbor.Tagged = true;
            }
        }

        private void TagNeighbors(BaseGameEntity baseGameEntity, float radius)
        {
            _taggedNeighbors.Clear();
            for (int index = _actor.World.MovingEntities.Count - 1; index >= 0; index--)
            {
                var neighbor = _actor.World.MovingEntities[index];

                if (neighbor == baseGameEntity) continue;

                //neighbor.Tagged = false;

                Vector2 vectorTo = neighbor.Position - baseGameEntity.Position;

                float range = radius + neighbor.Radius; //Todo:provjeritiii da li j edobro zato sto range nije dobar

                var rangeMult = range*range;
                var vectorLenght = vectorTo.Length();

                if (vectorLenght*vectorLenght < rangeMult) _taggedNeighbors.Add(neighbor);
                // neighbor.Tagged = true;
            }
        }

        //

        private static Vector2 VectorToLocalSpace(Vector2 vec,
                                                  Vector2 AgentHeading,
                                                  Vector2 AgentSide)
        {
            Vector2 TransVec = vec;
            float desiredAngle1 = (float) Math.Atan2(AgentHeading.Y, AgentHeading.X);
            if (desiredAngle1 < 0)
                desiredAngle1 += MathHelper.TwoPi;
            TransVec = Vector2.Transform(vec, Matrix.CreateRotationZ(desiredAngle1*-1));


            return TransVec;
        }

        private static Vector2 PointToLocalSpace(Vector2 point,
                                                 Vector2 AgentHeading,
                                                 Vector2 AgentSide,
                                                 Vector2 AgentPosition)
        {
            //make a copy of the point
            Vector2 TransPoint = point;
            float desiredAngle1 = (float) Math.Atan2(AgentHeading.Y, AgentHeading.X);
            if (desiredAngle1 < 0)
                desiredAngle1 += MathHelper.TwoPi;
            TransPoint = Vector2.Transform(point - AgentPosition, Matrix.CreateRotationZ(desiredAngle1*-1));


            return TransPoint;
        }


        private static Vector2 PointToWorldSpace(Vector2 point,
                                                 Vector2 AgentHeading,
                                                 Vector2 AgentSide,
                                                 Vector2 AgentPosition)
        {
            //make a copy of the point
            Vector2 TransPoint = point;
            float desiredAngle1 = (float) Math.Atan2(AgentHeading.Y, AgentHeading.X);
            if (desiredAngle1 < 0)
                desiredAngle1 += MathHelper.TwoPi;
            TransPoint = Vector2.Transform(point, Matrix.CreateRotationZ(desiredAngle1)*
                                                  Matrix.CreateTranslation(new Vector3(AgentPosition, 0)));

            return TransPoint;
        }

        private static Vector2 VectorToWorldSpace(Vector2 vec,
                                                  Vector2 AgentHeading,
                                                  Vector2 AgentSide)
        {
            Vector2 TransPoint = vec;
            float desiredAngle1 = (float) Math.Atan2(AgentHeading.Y, AgentHeading.X);
            if (desiredAngle1 < 0)
                desiredAngle1 += MathHelper.TwoPi;
            if (desiredAngle1 > MathHelper.TwoPi)
                desiredAngle1 -= MathHelper.TwoPi;
            TransPoint = Vector2.Transform(vec, Matrix.CreateRotationZ(desiredAngle1*-1));


            return TransPoint;
        }

        private float VectorToAngle(Vector2 vector)
        {
            return (float) Math.Atan2(vector.X, -vector.Y);
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
        public void SeekOn(Vector2 seekTarget)
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
        public void FleeOn(Vector2 fleeTarget)
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
        public void ArriveOn(Vector2 arriveTarget, Deacceleration deacceleration)
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
        public void PursuitOn(MovingEntity evader)
        {
            _evader = evader;
            _pursuitOn = true;
        }

        /// <summary>
        ///   Turns pursuit behaviour off
        /// </summary>
        /// <param name = "evader"></param>
        public void PursuitOff(MovingEntity evader)
        {
            _pursuitOn = false;
        }

        /// <summary>
        ///   Turns evade behaviour on
        /// </summary>
        /// <param name = "pursuer">The entity witch we are evading</param>
        public void EvadeOn(MovingEntity pursuer)
        {
            _pursuer = pursuer;
            _evadeOn = true;
        }

        /// <summary>
        ///   Turns evade behaviour off
        /// </summary>
        /// <param name = "pursuer"></param>
        public void EvadeOff(MovingEntity pursuer)
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

        private Vector2 PointToLocalSpace(Vector2 point, Vector2 agentHeading, Vector2 agentPosition)
        {
            //make a copy of the point
            Vector2 transPoint = point;

            //create a transformation matrix

            var agentSide = new Vector2(-agentHeading.Y, agentHeading.X);

            Matrix matTransform = new Matrix();
            float tx = Vector2.Dot(agentPosition, agentHeading);
            float ty = Vector2.Dot(agentPosition, agentSide);


            //create the transformation matrix
            matTransform.M11 = agentHeading.X;
            matTransform.M12 = agentSide.X;
            matTransform.M21 = agentHeading.Y;
            matTransform.M22 = agentSide.Y;
            matTransform.M31 = tx;
            matTransform.M32 = ty;

            //now transform the vertices
            TransformVector2Ds(ref transPoint, matTransform);

            return transPoint;
        }

        private void TransformVector2Ds(ref Vector2 vPoint, Matrix matrix)
        {
            float tempX = (matrix.M11*vPoint.X) + (matrix.M21*vPoint.Y) + (matrix.M31);

            float tempY = (matrix.M12*vPoint.X) + (matrix.M22*vPoint.Y) + (matrix.M32);

            vPoint.X = tempX;

            vPoint.Y = tempY;
        }

        #endregion
    }
}