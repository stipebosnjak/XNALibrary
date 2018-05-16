 #region

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Xna.Helpers._3D.Cameras
{

    #region Enums

    #endregion

    public class FreeCamera : Camera
    {
        #region Fields

        private GraphicsDevice _graphicsDevice;
        private float _yaw, _pitch, _roll;
        private Vector3 _translation;
       private MouseState _originalMouseState;
        private float _speed;
        #endregion

        #region Properties
        /// <summary>
        /// Speed of the camera
        /// </summary>
        public float Speed { get; set; }
        /// <summary>
        /// Up vector of the camera
        /// </summary>
        public Vector3 Up { get; private set; }
        /// <summary>
        /// Right vector of the camera
        /// </summary>
        public Vector3 Right { get; private set; }

        /// <summary>
        /// Camera yaw
        /// </summary>
        public float Yaw
        {
            get { return _yaw; }
        }
        /// <summary>
        /// Camera pitch
        /// </summary>
        public float Pitch
        {
            get { return _pitch; }
        }
        /// <summary>
        /// Camera roll
        /// </summary>
        public float Roll
        {
            get { return _roll; }
        }

        /// <summary>
        /// Rotational matrix of the Camera
        /// </summary>
        public Matrix Rotation { get
        {
            Matrix rotation = Matrix.CreateFromYawPitchRoll(_yaw, _pitch, _roll);
            return rotation;
        } }
        #endregion

        #region Constructors


        public FreeCamera(Vector3 position, float nearPlane, float farPlane, float fieldOfView,
                          GraphicsDevice graphicsDevice, float yaw, float pitch)
            : base(nearPlane, farPlane, fieldOfView, graphicsDevice)
        {
            Position = position;
            _yaw = yaw;
            _pitch = pitch;
            _roll = 0;
            _translation = Vector3.Zero;
            Speed = 2f;
            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
            _originalMouseState = Mouse.GetState();
            _graphicsDevice = graphicsDevice;
            
        }

        public FreeCamera(Vector3 position, GraphicsDevice graphicsDevice, float yaw, float pitch)
            : base(graphicsDevice)
        {
            Position = position;
            _yaw = yaw;
            _pitch = pitch;
            _roll = 0;
            _translation = Vector3.Zero;
           
            Speed = 2f;
            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
            _originalMouseState = Mouse.GetState();
            _graphicsDevice = graphicsDevice;
        }

        #endregion

        #region Methods

      
        public override void Update(GameTime gameTime)
        {
            if (DefaultInput)
                UpdateInput(gameTime);

            Matrix rotation = Rotation;
            //Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, _roll);
           // rotation = Matrix.CreateFromQuaternion(quaternion);
          _yaw=  MathHelper.WrapAngle(_yaw);
          _pitch = MathHelper.WrapAngle(_pitch);
        //  _yaw = MathHelper.WrapAngle(_yaw);
            _translation = Vector3.Transform(_translation, rotation);
             Position += _translation;
            _translation = Vector3.Zero;
            Target = Position + rotation.Forward;

            //Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            //Target = Position + forward;
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);
            Up = up;//rotation.Up;
            Right = rotation.Right;
            //Vector3.Cross(forward, up);
            LastMouseState = CurrentMouseState;
            View = Matrix.CreateLookAt(Position, Target, up);
            base.Update(gameTime);
        }

        /// <summary>
        /// Updates the camera , the camera will follow height
        /// </summary>
        /// <param name="height"></param>
        /// <param name="gameTime"></param>
        public void Update(int height, GameTime gameTime)
         {
             if (DefaultInput)
                 UpdateInput(gameTime);

             Matrix rotation = Matrix.CreateFromYawPitchRoll(_yaw, _pitch, _roll);

             _translation = Vector3.Transform(_translation, rotation);
             Position += _translation;
             _translation = Vector3.Zero;

             Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
             Target = Position + forward;

             Vector3 up = Vector3.Transform(Vector3.Up, rotation);

             View = Matrix.CreateLookAt(Position, Target, up);
             Up = up;
             Right = Vector3.Cross(forward, up);
             LastMouseState = CurrentMouseState;

            float yOffsetPerc = height*1.1f;
            float yOffset = yOffsetPerc - height;

            if ( Position.Y <= height+yOffset)
            {
                  Vector3 newPosition = new Vector3(Position.X, height + yOffset, Position.Z);
                  Position = Vector3.Lerp(Position, newPosition, 0.5f);
            }
             base.Update(gameTime);
         }

        private void UpdateInput(GameTime gameTime)
        {
            // Get the new keyboard and mouse state
            CurrentMouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            float deltaX = _originalMouseState.X - (float)CurrentMouseState.X;
            float deltaY = _originalMouseState.Y - (float)CurrentMouseState.Y;
           
            // Rotate the camera
            Rotate(deltaY*.01f,deltaX*.01f);
            Vector3 translation = Vector3.Zero;
            
            // Determine in which direction to move the camera
            if (keyState.IsKeyDown(Keys.W)) translation += Vector3.Forward ;
            if (keyState.IsKeyDown(Keys.S)) translation += Vector3.Backward;
            if (keyState.IsKeyDown(Keys.A)) translation += Vector3.Left;
            if (keyState.IsKeyDown(Keys.D)) translation += Vector3.Right;
            
            if (keyState.IsKeyDown(Keys.LeftShift))
            {
                _speed = Speed*5;
            }
            else if (keyState.IsKeyDown(Keys.LeftControl))
            {
                _speed = Speed/5;
            }
            else
            {
                    _speed = Speed;
            }


            // Move 3 units per millisecond, independent of frame rate
            translation *= 3*(float) gameTime.ElapsedGameTime.TotalMilliseconds;
            // Move the camera
            Move(translation);
            if (CurrentMouseState == _originalMouseState) return;
            Mouse.SetPosition(_graphicsDevice.Viewport.Width / 2, _graphicsDevice.Viewport.Height / 2);
        }

        private void Rotate(float pitchAmount, float yawAmount)
        {
            CheckIfCameraIsUpsideDown(ref yawAmount);

            _yaw += yawAmount;
            _pitch += pitchAmount;
        }

        private void Move(Vector3 translation)
        {
            _translation += translation*_speed;
        }


        private void CheckIfCameraIsUpsideDown(ref float yawAmount)
        {
            if (_pitch>1.5f||_pitch<-1.5f)
            {
                yawAmount = -yawAmount;
            }
        }
        #endregion
    }
}