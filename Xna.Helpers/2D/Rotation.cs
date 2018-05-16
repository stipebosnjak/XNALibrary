using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xna.Helpers._2D
{
    /// <summary>
    /// Contains 
    /// </summary>
  public  class Rotation
    {

      /// <summary>
      /// Returns rotation to match the current heading
      /// </summary>
      /// <param name="position"></param>
      /// <param name="faceThis"></param>
      /// <param name="currentAngle"></param>
      /// <param name="turnSpeed"></param>
      /// <returns></returns>
        public static float TurnToFace(Vector2 position, Vector2 faceThis,
            float currentAngle, float turnSpeed)
        {
            // consider this diagram:
            //         C 
            //        /|
            //      /  |
            //    /    | y
            //  / o    |
            // S--------
            //     x
            // 
            // where S is the position of the spot light, C is the position of the cat,
            // and "o" is the angle that the spot light should be facing in order to 
            // point at the cat. we need to know what o is. using trig, we know that
            //      tan(theta)       = opposite / adjacent
            //      tan(o)           = y / x
            // if we take the arctan of both sides of this equation...
            //      arctan( tan(o) ) = arctan( y / x )
            //      o                = arctan( y / x )
            // so, we can use x and y to find o, our "desiredAngle."
            // x and y are just the differences in position between the two objects.
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;

            // we'll use the Atan2 function. Atan will calculates the arc tangent of 
            // y / x for us, and has the added benefit that it will use the signs of x
            // and y to determine what cartesian quadrant to put the result in.
            // http://msdn2.microsoft.com/en-us/library/system.math.atan2.aspx
            float desiredAngle = (float)Math.Atan2(y, x);

            // so now we know where we WANT to be facing, and where we ARE facing...
            // if we weren't constrained by turnSpeed, this would be easy: we'd just 
            // return desiredAngle.
            // instead, we have to calculate how much we WANT to turn, and then make
            // sure that's not more than turnSpeed.

            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            float difference = WrapAngle(desiredAngle - currentAngle);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            return WrapAngle(currentAngle + difference);
        }

        private static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }
      /// <summary>
      /// Returns angle between vectors 
      /// </summary>
      /// <param name="aPlayer"></param>
      /// <param name="aTarget"></param>
      /// <returns></returns>
        public static float RadianAngleBetweenVectors(Vector2 aPlayer, Vector2 aTarget)
        {
            return -(float)Math.Atan2((aTarget.X - aPlayer.X), (aTarget.Y - aPlayer.Y));
        }
        /// <summary>
        /// Rotates the vector around specified point around point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="originPoint"></param>
        /// <param name="radiansToRotate"></param>
        /// <returns></returns>
        public static Vector2 RotateAroundPoint(Vector2 point, Vector2 originPoint, float radiansToRotate)
        {
            point = Vector2.Transform(point, Matrix.CreateRotationZ(radiansToRotate));
            point += originPoint;
            return point;
        }
        /// <summary>
        /// Rotates the vector around specified point around point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="radiansToRotate"></param>
        /// <returns></returns>
        public static Vector2 RotateAroundPoint(Vector2 point, float radiansToRotate)
        {
            point = Vector2.Transform(point, Matrix.CreateRotationZ(radiansToRotate));
            return point;
        }

    }
}
