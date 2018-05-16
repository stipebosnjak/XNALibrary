using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xna.Helpers._2D
{
    /// <summary>
    /// Class for creating walls
    /// </summary>
    public class Wall
    {
        private Vector2 _start, _end, _normal;
        private Texture2D _texture;

        /// <summary>
        /// Creates new wall 
        /// </summary>
        /// <param name="start">Start position of the wall</param>
        /// <param name="end">Ending position of the wall</param>
        public Wall(Vector2 start, Vector2 end)
        {
            _start = start;
            _end = end;
             Vector2 temp = Vector2.Normalize(end - start);
            _normal=new Vector2 {X = -temp.Y, Y = temp.X};
        }

      /// <summary>
        /// Start position of the wall
      /// </summary>
        public Vector2 Start
        {
            get { return _start; }
            set { _start = value; }
        }
        /// <summary>
        /// Ending position of the wall
        /// </summary>
        public Vector2 End
        {
            get { return _end; }
            set { _end = value; }
        }
        /// <summary>
        /// Perpedicunal vector of the wall
        /// </summary>
        public Vector2 Normal
        {
            get { return _normal; }
            set { _normal = value; }
        }
    }
}
