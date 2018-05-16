using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Xna.Helpers._2D
{
    /// <summary>
    /// Sides of the wall
    /// </summary>
    public enum Sides
    {
        /// <summary>
        /// 
        /// </summary>
        Left,
        /// <summary>
        /// 
        /// </summary>
        Right,
        /// <summary>
        /// 
        /// </summary>
        Up,
        /// <summary>
        /// 
        /// </summary>
        Down,
        /// <summary>
        /// 
        /// </summary>
        None
    }

   
    /// <summary>
    /// Class containg various collisions methods for 2D objects
    /// </summary>
    public static class CollisionDetection2D
    {
        

        #region Actor Collisions
        //todo: napravit bolje, sa positions, rectangle, i sa samo izracunavajucim radiusom , sa texturom
        /// <summary>
        /// Old !!!- Detect bounding rectangles 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="width1"></param>
        /// <param name="height1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="width2"></param>
        /// <param name="height2"></param>
        /// <returns></returns>
        public static bool BoundingRectangle(int x1,int y1,int width1,int height1,int x2,int y2,int width2,int height2)
        {
            Rectangle rectangleA = new Rectangle((int)x1, (int)y1, width1, height1);
            Rectangle rectangleB = new Rectangle((int)x2, (int)y2, width2, height2);

            int top = Math.Max(rectangleA.Top, rectangleB.Top);
            int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
            int left = Math.Max(rectangleA.Left, rectangleB.Left);
            int right = Math.Min(rectangleA.Right, rectangleB.Right);

            if (top >= bottom || left >= right)
                return false;

            return true;
        }

        /// <summary>
        /// Old !!!- Detect bounding circles
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="radius1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="radius2"></param>
        /// <returns></returns>
        public static bool BoundingCircle(int x1, int y1, int radius1, int x2, int y2, int radius2)
        {
            Vector2 V1 = new Vector2(x1, y1);
            Vector2 V2 = new Vector2(x2, y2);

            Vector2 Distance = V1 - V2;

            if (Distance.Length() < radius1 + radius2)
                return true;

            return false;
        }
        /// <summary>
        /// Detect bounding circles
        /// </summary>
        /// <param name="centeredPosition1"></param>
        /// <param name="radius1"></param>
        /// <param name="centeredPosition2"></param>
        /// <param name="radius2"></param>
        /// <returns></returns>
        public static bool BoundingCircle(Vector2 centeredPosition1, float radius1, Vector2 centeredPosition2, float radius2)
        {
            Vector2 v1 = centeredPosition1;
            Vector2 v2 = centeredPosition2;

            Vector2 distance = v1 - v2;

            if (distance.Length() < radius1 + radius2)
                return true;

            return false;
        }
       
        ///<summary>
        /// Used to detect bounding triangles
        ///</summary>
        ///<param name="p1"></param>
        ///<param name="p2"></param>
        ///<returns></returns>
        public static bool BoundingTriangles(List<Vector2> p1, List<Vector2> p2)
        {
            for (int i = 0; i < 3; i++)
                if (IsPointInsideTriangle(p1, p2[i])) return true;

            for (int i = 0; i < 3; i++)
                if (IsPointInsideTriangle(p2, p1[i])) return true;
            return false;
        }
      
        private static bool IsPointInsideTriangle(List<Vector2> trianglePoints, Vector2 p)
        {
            // Translated to C# from: http://www.ddj.com/184404201
            Vector2 e0 = p - trianglePoints[0];
            Vector2 e1 = trianglePoints[1] - trianglePoints[0];
            Vector2 e2 = trianglePoints[2] - trianglePoints[0];

            float u, v = 0;
            if (e1.X == 0)
            {
                if (e2.X == 0) return false;
                u = e0.X / e2.X;
                if (u < 0 || u > 1) return false;
                if (e1.Y == 0) return false;
                v = (e0.Y - e2.Y * u) / e1.Y;
                if (v < 0) return false;
            }
            else
            {
                float d = e2.Y * e1.X - e2.X * e1.Y;
                if (d == 0) return false;
                u = (e0.Y * e1.X - e0.X * e1.Y) / d;
                if (u < 0 || u > 1) return false;
                v = (e0.X - e2.X * u) / e1.X;
                if (v < 0) return false;
                if ((u + v) > 1) return false;
            }

            return true;
        }

        ///<summary>
        /// Detect bounding textures by their texture data
        ///</summary>
        ///<param name="rectangleA"></param>
        ///<param name="dataA"></param>
        ///<param name="rectangleB"></param>
        ///<param name="dataB"></param>
        ///<returns></returns>
        public static bool IntersectPixels(Rectangle rectangleA, Color[] dataA,
                                   Rectangle rectangleB, Color[] dataB)
        {
            // Find the bounds of the rectangle intersection
            int top = Math.Max(rectangleA.Top, rectangleB.Top);
            int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
            int left = Math.Max(rectangleA.Left, rectangleB.Left);
            int right = Math.Min(rectangleA.Right, rectangleB.Right);

            // Check every point within the intersection bounds
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    // Get the color of both pixels at this point
                    Color colorA = dataA[(x - rectangleA.Left) +
                                         (y - rectangleA.Top) * rectangleA.Width];
                    Color colorB = dataB[(x - rectangleB.Left) +
                                         (y - rectangleB.Top) * rectangleB.Width];

                    // If both pixels are not completely transparent,
                    if (colorA.A != 0 && colorB.A != 0)
                    {
                        // then an intersection has been found
                        return true;
                    }
                }
            }

            // No intersection found
            return false;
        }
        #endregion

        #region Misc Collisions
        /// <summary>
        /// Returns bool whenever the position collided with the bounds, it will set origin for you
        /// </summary>
        /// <param name="position"></param>
        /// <param name="texture"></param>
        /// <param name="widthBound"></param>
        /// <param name="heightBound"></param>
        /// <returns></returns>
        public static bool CheckWallCollision(Vector2 position,Texture2D texture,float widthBound,float heightBound)
        {
            //wall hit
            if (position.X < 0 || position.X > widthBound - texture.Width/2 || position.Y < 0 ||
                position.Y > heightBound - texture. Height/2)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Returns bool whenever the position collided with the bounds , use if u set origin to center of texture
        /// </summary>
        /// <param name="position"></param>
        /// <param name="widthBound"></param>
        /// <param name="heightBound"></param>
        /// <returns></returns>
        public static bool CheckWallCollision(Vector2 position, float widthBound, float heightBound)
        {
            //wall hit
            if (position.X < 0 || position.X > widthBound  || position.Y < 0 ||
                position.Y > heightBound)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks distance to wall and returns the side  of the wall 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="widthBound"></param>
        /// <param name="heightBound"></param>
        /// <param name="distance"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        public static bool NearestDistanceToWall(Vector2 position, float widthBound, float heightBound,float distance,out Sides side)
        {
            side = Sides.None;
            if (position.X < distance)
            {
                side = Sides.Left;
            }
            else if(position.X > widthBound-distance)
            {
                side = Sides.Right;
            }
            else if(position.Y < distance)
            {
                side = Sides.Up;
            }
            else if (position.Y > heightBound - distance)
            {
                side = Sides.Down;
            }
            if (position.X < distance || position.X > widthBound-distance || position.Y < distance ||
                position.Y > heightBound-distance)
            {
                return true;
            }
            
            return false;
        }


        /// <summary>
        /// Sets the object position inside bounds in case of a wall/bounds hit(origin of texture be must set in center)
        /// </summary>
        /// <param name="position">Position of the object</param>
        /// <param name="texture"> </param>
        /// <param name="widthBound"></param>
        /// <param name="heightBound"></param>
        public static Vector2 ProcessWallCollision( Vector2 position, Texture2D texture, float widthBound, float heightBound)
        {
            if (position.X < texture.Width/2)
            {
                position = new Vector2(texture.Width / 2, position.Y);
            }
            if (position.X > widthBound - texture.Width / 2)
            {
                position = new Vector2(widthBound - texture.Width / 2, position.Y);
            }
            if (position.Y < texture.Height / 2)
            {
                position = new Vector2(position.X, texture.Height / 2);
            }
            if (position.Y > heightBound - texture.Height / 2)
            {
                position = new Vector2(position.X, heightBound - texture.Height / 2);
            }
            return position;
        }





        #endregion

        #region Distance Checks
        /// <summary>
        /// Returns the bool result if the two positions distances are under the specified range
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pos"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool IsNearTarget(Vector2 target, Vector2 pos, float range)
        {
            float distance = Vector2.Distance(target, pos);
            bool result = distance < range;

            return result;
        }
        #endregion

    }
}
