#region

using System;
using System.IO;
using Microsoft.Xna.Framework;

#endregion

namespace Xna.Helpers._2D
{
    /// <summary>
    /// Class that contains various helpers for moving your objects
    /// </summary>
    public  class MovingHelper
    {
        

        private static Random _random;


        static MovingHelper()
        {
            _random = new Random();
        }

        /// <summary>
        ///   Returns an offset in a Vector2: VectorX influenced by Sinus and VectorY influenced by Cosinus
        /// </summary>
        /// <param name = "frequency">required frequency</param>
        /// <param name = "amplitude"></param>
        /// <param name = "gameTime"></param>
        /// <returns></returns>
        public static Vector2 Stager(float frequency, float amplitude, GameTime gameTime)
        {
            var offset = new Vector2
                             {
                                 X = (float) (Math.Sin(gameTime.TotalGameTime.TotalSeconds*frequency)*amplitude),
                                 Y = (float) (Math.Cos(gameTime.TotalGameTime.TotalSeconds*frequency)*amplitude)
                             };

            return offset;
        }

        /// <summary>
        ///   Returns direction from two vectors as Vector2 
        /// </summary>
        /// <param name = "followerPosition"></param>
        /// <param name = "beingFollowedPosition"></param>
        /// <returns></returns>
        public Vector2 GetDirection(Vector2 followerPosition, Vector2 beingFollowedPosition)
        {
            Vector2 direction = beingFollowedPosition - followerPosition;
            return direction;
        }
       
        /// <summary>
        /// Gets normalized direction Vector2 between two positions , can be multiplied
        /// </summary>
        /// <param name="target"></param>
        /// <param name="origPos"></param>
        /// <param name="speedMultiplier"></param>
        /// <returns></returns>
        public Vector2 GetVelocity(Vector2 target, Vector2 origPos, float speedMultiplier = 1)
        {
            Vector2 calc = Vector2.Subtract(target, origPos);
            float distance = Vector2.Distance(target, origPos);
            if (distance < 1)
                return Vector2.Zero;

            calc.Normalize();
            return calc*speedMultiplier;
        }

        private Vector2 ProjectVector(Vector2 Source, Vector2 Target)
        {
            float dotProduct = 0.0f;
            dotProduct = Vector2.Dot(Source, Target);

            Vector2 projectedVector = (dotProduct/Target.LengthSquared())*Target;
            return projectedVector;
        }
    }
}