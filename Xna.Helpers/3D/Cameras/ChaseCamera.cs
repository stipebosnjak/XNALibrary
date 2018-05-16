#region

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Xna.Helpers._3D.Cameras
{
    /// <summary>
    ///   Camera that "chases" the actor with some springiness factor
    /// </summary>
    public class ChaseCamera : Camera
    {
        #region Fields

        private float _springiness = .15f;

        #endregion

        #region Properties

        /// <summary>
        ///   Position of the target
        /// </summary>
        public Vector3 FollowTargetPosition { get; private set; }

        /// <summary>
        ///   Rotation of the target
        /// </summary>
        public Vector3 FollowTargetRotation { get; private set; }

        /// <summary>
        ///   Position offset
        /// </summary>
        public Vector3 PositionOffset { get; set; }

        /// <summary>
        ///   Target offset
        /// </summary>
        public Vector3 TargetOffset { get; set; }

        /// <summary>
        /// </summary>
        public Vector3 RelativeCameraRotation { get; set; }

        public Vector3 Up { get; set; }
        public Vector3 Right { get; set; }
        /// <summary>
        /// Default target , setting this 
        /// </summary>
        public CModel Model { get; set; }

        /// <summary>
        ///   How fast will the camera follow target
        /// </summary>
        public float Springiness
        {
            get { return _springiness; }
            set { _springiness = MathHelper.Clamp(value, 0, 1); }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///   Creates new chase camera
        /// </summary>
        /// <param name = "positionOffset"></param>
        /// <param name = "targetOffset"></param>
        /// <param name = "relativeCameraRotation"></param>
        /// <param name = "graphicsDevice"></param>
        public ChaseCamera(Vector3 positionOffset, Vector3 targetOffset,
                           Vector3 relativeCameraRotation, GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            this.PositionOffset = positionOffset;
            this.TargetOffset = targetOffset;
            this.RelativeCameraRotation = relativeCameraRotation;
        }
        /// <summary>
        /// Creates new chase  camera with default target model
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="model"></param>
        public ChaseCamera(GraphicsDevice graphicsDevice, CModel model) : base(graphicsDevice)
        {
            Model = model;
            PositionOffset = new Vector3(model.Position.X, model.Position.Y,Model.Position.Z+ 1500);
            TargetOffset=  new Vector3(Model.Position.X,Model.Position.Y+200,Model.Position.Z);
            RelativeCameraRotation = Vector3.Zero;
        }
        /// <summary>
        /// Creates new chase  camera with specific parametars
        /// </summary>
        /// <param name="nearPlane"></param>
        /// <param name="farPlane"></param>
        /// <param name="fieldOfView"></param>
        /// <param name="graphicsDevice"></param>
        /// <param name="positionOffset"></param>
        /// <param name="targetOffset"></param>
        /// <param name="relativeCameraRotation"></param>
        public ChaseCamera(float nearPlane, float farPlane, float fieldOfView, GraphicsDevice graphicsDevice, Vector3 positionOffset, Vector3 targetOffset, Vector3 relativeCameraRotation) : base(nearPlane, farPlane, fieldOfView, graphicsDevice)
        {
            PositionOffset = positionOffset;
            TargetOffset = targetOffset;
            RelativeCameraRotation = relativeCameraRotation;
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Moves the camera
        /// </summary>
        /// <param name = "newFollowTargetPosition"></param>
        /// <param name = "newFollowTargetRotation"></param>
        public void Move(Vector3 newFollowTargetPosition,
                         Vector3 newFollowTargetRotation)
        {
            FollowTargetPosition = newFollowTargetPosition;
            FollowTargetRotation = newFollowTargetRotation;
        }

        /// <summary>
        ///   Rotates the camera
        /// </summary>
        /// <param name = "rotationChange"></param>
        public void Rotate(Vector3 rotationChange)
        {
            RelativeCameraRotation += rotationChange;
        }

        public override void Update(GameTime gameTime)
        {
            UpdateInput();

            // Sum the rotations of the model and the camera to ensure it 
            // is rotated to the correct position relative to the model's 
            // rotation
            Vector3 combinedRotation = FollowTargetRotation +
                                       RelativeCameraRotation;

            // Calculate the rotation matrix for the camera
            Matrix rotation = Matrix.CreateFromYawPitchRoll(
                combinedRotation.Y, combinedRotation.X, combinedRotation.Z);

            // Calculate the position the camera would be without the spring
            // value, using the rotation matrix and target position
            Vector3 desiredPosition = FollowTargetPosition +
                                      Vector3.Transform(PositionOffset, rotation);

            // Interpolate between the current position and desired position
            Position = Vector3.Lerp(Position, desiredPosition, Springiness);
            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            // Calculate the new target using the rotation matrix
            Target = FollowTargetPosition +
                     Vector3.Transform(TargetOffset, rotation);

            // Obtain the up vector from the matrix
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);
            Up = up;
            Right = Vector3.Cross(forward, up);
            // Recalculate the view matrix
            View = Matrix.CreateLookAt(Position, Target, up);
        }

        private void UpdateInput()
        {
            if (Model!=null)
            {
                Move(Model.Position,Model.Rotation);
            }
            else
            {
                //Move(SkinnedModel.Position, SkinnedModel.Rotation);
            }
            
        }
        #endregion
    }
}