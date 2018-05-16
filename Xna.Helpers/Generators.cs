#region

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Xna.Helpers
{
    /// <summary>
    ///   Class that works with Random 
    /// </summary>
    public static class Generators
    {
        private static readonly Random Random;


        static Generators()
        {
            Random = new Random();
        }

        /// <summary>
        ///   Returns random float
        /// </summary>
        /// <param name = "min"></param>
        /// <param name = "max"></param>
        /// <returns></returns>
        public static float RandomNumber(float min, float max)
        {
            return (float) (Random.NextDouble()*(max - min) + min);
        }

        /// <summary>
        ///   Returns random double
        /// </summary>
        /// <param name = "min"></param>
        /// <param name = "max"></param>
        /// <returns></returns>
        public static double RandomNumber(double min, double max)
        {
            return (Random.NextDouble()*(max - min) + min);
        }

        /// <summary>
        ///   Returns random integer
        /// </summary>
        /// <param name = "min"></param>
        /// <param name = "max">Exclusive</param>
        /// <returns></returns>
        public static int RandomNumber(int min, int max)
        {
            return Random.Next(min, max);
        }

        /// <summary>
        ///   Not working sorry, needs to be tested
        /// </summary>
        /// <param name = "min"></param>
        /// <param name = "max"></param>
        /// <returns></returns>
        public static Vector2 RandomVector2(float min, float max)
        {
            float x = RandomNumber(min, max);
            float y = RandomNumber(min, max);
            return new Vector2(x, y);
        }



        public static Vector3 RandomVector3(float min, float max)
        {
            float x = RandomNumber(min, max);
            float y = RandomNumber(min, max);
            float z = RandomNumber(min, max);
            return new Vector3(x, y,z);
        }

     
       
    }
}