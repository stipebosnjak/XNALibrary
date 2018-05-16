#region

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Xna.Helpers._3D.Cameras
{

    #region Enums

    #endregion
    /// <summary>
    /// Arc ball camera, for avatar scenes or editors
    /// </summary>
    public class ArcBallCamera : Camera
    {
        #region Fields

        #endregion

        #region Properties

        public float RotationX { get; set; }
        public float RotationY { get; set; }

        public float MinRotationY { get; set; }
        public float MaxRotationY { get; set; }

        public float Distance { get; set; }

        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }

        #endregion

        #region Constructors

        public ArcBallCamera(Vector3 target, float nearPlane, float farPlane, float fieldOfView,
                             GraphicsDevice graphicsDevice, float rotationX, float rotationY, float minRotationY,
                             float maxRotationY, float distance, float minDistance, float maxDistance)
            : base(nearPlane, farPlane, fieldOfView, graphicsDevice)
        {
            Target = target;

            RotationX = rotationX;
            RotationY = rotationY;
            MinRotationY = minRotationY;
            MaxRotationY = maxRotationY;
            Distance = distance;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }

        public ArcBallCamera(Vector3 target, GraphicsDevice graphicsDevice, float rotationX, float rotationY,
                             float minRotationY, float maxRotationY, float distance, float minDistance,
                             float maxDistance)
            : base(graphicsDevice)
        {
            Target = target;
            MinRotationY = minRotationY;
            MaxRotationY = maxRotationY;

            RotationX = rotationX;
            RotationY = MathHelper.Clamp(RotationY, MinRotationY, MaxRotationY);

            MinDistance = minDistance;
            MaxDistance = maxDistance;

            Distance = MathHelper.Clamp(Distance, MinDistance, MaxDistance);
        }

        #endregion

        #region Methods

        public void Move(float distanceChange)
        {
            Distance += distanceChange;
            Distance = MathHelper.Clamp(Distance, MinDistance,
                                        MaxDistance);
        }

        public void Rotate(float rotationXChange, float rotationYChange)
        {
            RotationX += rotationXChange;
            RotationY += -rotationYChange;
            RotationY = MathHelper.Clamp(RotationY, MinRotationY,
                                         MaxRotationY);
        }

        public void Translate(Vector3 positionChange)
        {
            Position += positionChange;
        }

        public override void Update(GameTime gameTime)
        {
            if (DefaultInput)UpdateInput(gameTime);
            // Calculate rotation matrix from rotation values
            Matrix rotation = Matrix.CreateFromYawPitchRoll(RotationX, -
                                                                       RotationY, 0);
            // Translate down the Z axis by the desired distance
            // between the camera and object, then rotate that
            // vector to find the camera offset from the target
            Vector3 translation = new Vector3(0, 0, Distance);
            translation = Vector3.Transform(translation, rotation);
            Position = Target + translation;
            // Calculate the up vector from the rotation matrix
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);
            View = Matrix.CreateLookAt(Position, Target, up);

            LastMouseState = CurrentMouseState;
        }
        private void UpdateInput(GameTime gameTime)
        {
            // Get the new keyboard and mouse state
            CurrentMouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();
            // Determine how much the camera should turn
            float deltaX = (float)LastMouseState.X - (float)CurrentMouseState.X;
            float deltaY = (float)LastMouseState.Y - (float)CurrentMouseState.Y;
            // Rotate the camera
           Rotate(deltaX * .01f, deltaY * .01f);
            // Calculate scroll wheel movement
           float scrollDelta = (float)LastMouseState.ScrollWheelValue -
              (float)CurrentMouseState.ScrollWheelValue;
            // Move the camera
            Move(scrollDelta);
            // Update the camera
         
            // Update the mouse state
           
        }
        #endregion
    }
}