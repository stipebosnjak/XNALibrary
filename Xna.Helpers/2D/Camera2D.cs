using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Xna.Helpers._2D
{
    /// <summary>
    /// Class for creating a camera
    /// </summary>
   public class Camera2D
    {
        private float _zoom; // Camera Zoom
        private Matrix _transform; // Matrix Transform
        private Vector2 _pos; // Camera Position
        private float _rotation; // Camera Rotation
        private bool _selfUpdating;
        private KeyboardState _oldState;
        private float _width;
        private float _height;
       /// <summary>
       /// Creates a new camera 
       /// </summary>
        public Camera2D()
        {
            _selfUpdating = false;
            _zoom = 1.0f;
            _rotation = 0.0f;
            _pos = Vector2.Zero;
            _oldState=new KeyboardState();
        }
       /// <summary>
       /// Width of camera 
       /// </summary>
        public float Width
        {
            get { return _width; }
            set { _width = value; }
        }
       /// <summary>
       /// Height of the camera
       /// </summary>
        public float Height
        {
            get { return _height; }
            set { _height = value; }
        }
       /// <summary>
       ///
       /// </summary>
        public bool SelfUpdating
        {
            get { return _selfUpdating; }
            set { _selfUpdating = value; }
        }
       /// <summary>
       /// Zoom of the camera , minimum is 0.1f
       /// </summary>
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                if (_zoom < 0.1f) _zoom = 0.1f;
            } 
        }
       /// <summary>
       /// Rotation of the camera
       /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

       /// <summary>
       /// Move the camera by specified amount
       /// </summary>
       /// <param name="amount"></param>
        public void Move(Vector2 amount)
        {
            _pos += amount;
        }
       /// <summary>
       /// Position of the camera
       /// </summary>
        public Vector2 Position
        {
            get { return _pos; }
            set { _pos = value; }
        }
       /// <summary>
       /// Gets the Matrix transformation 
       /// </summary>
       /// <param name="graphicsDevice"></param>
       /// <returns></returns>
        public Matrix GetTransformation(GraphicsDevice graphicsDevice)
        {
            _transform = // Thanks to o KB o for this solution
                Matrix.CreateTranslation(new Vector3(-_pos.X, -_pos.Y, 0))*Matrix.CreateRotationZ(Rotation)*Matrix.CreateScale(new Vector3(Zoom, Zoom, 1))*Matrix.CreateTranslation(new Vector3(_width*0.5f, _height*0.5f, 0));
            return _transform;
        }
       /// <summary>
       /// Input for handling the camera
       /// </summary>
        public void CheckInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            var newState = Keyboard.GetState();
            foreach (var pressedKey in keyboardState.GetPressedKeys())
            {
                if (!_selfUpdating)
                {
                    if (pressedKey == Keys.A)
                        _pos = new Vector2(_pos.X - 1, _pos.Y);

                    if (pressedKey == Keys.S)
                        _pos = new Vector2(_pos.X, _pos.Y + 1);

                    if (pressedKey == Keys.D)
                        _pos = new Vector2(_pos.X + 1, _pos.Y);

                    if (pressedKey == Keys.W)
                        _pos = new Vector2(_pos.X, _pos.Y - 1);

                    if (pressedKey == Keys.F)
                        _zoom -= 0.01f;

                    if (pressedKey == Keys.G)
                        _zoom += 0.01f;

                    if (pressedKey == Keys.R)
                        Rotation += 0.01f;

                    if (pressedKey == Keys.T)
                        Rotation -= 0.01f;

                    if (pressedKey==Keys.E)
                    {
                        _zoom = 1.0f;
                        _rotation = 0.0f;
                        _pos=new Vector2(_width,_height);
                    }
                }
            }
            if (newState.IsKeyDown(Keys.P) && _oldState.IsKeyUp(Keys.P))
            {
                _selfUpdating = !_selfUpdating;
            }
            _oldState = newState;
        }

    }
}