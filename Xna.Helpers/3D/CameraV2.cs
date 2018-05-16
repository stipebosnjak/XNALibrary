using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Xna.Helpers._3D
{
    /// <summary> 
    /// How the camera should behave. 
    /// </summary> 
    public enum CameraMode
    {
        FreeLook,
        FirstPerson,
        ThirdPerson,
        Target
    }


    /// <summary> 
    /// CameraComponent is a representation of a 3D camera 
    /// </summary> 
    ///  
    public class CameraV2 : Microsoft.Xna.Framework.DrawableGameComponent
    {
        MouseState previousMouseState;
        MouseState currentMouseState;
        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;


        Matrix viewMatrix;
        Matrix projectionMatrix;
        Matrix rotationMatrix;  //regenerated each frame based on the yaw/pitch/roll 

        float nearClip = 0.1f;
        float farClip = 1000.0f;
        float aspectRatio;
        float fieldOfView = MathHelper.ToRadians(45.0f);

        float yaw = 0.0f;
        float pitch = 0.0f;     //todo: figure out good default values 
        float roll = 0.0f;

        float yawVelocity = 0.0f;
        float pitchVelocity = 0.0f; //Todo: modify these instead of the actual values 
        float rollVelocity = 0.0f;

        float yawSensitivity = 1.0f;   //Todo: multiply yaw/pitch/roll velocities by this to allow for different sensitivities 
        float pitchSensitivity = 1.0f;
        float rollSensitivity = 1.0f;

        Vector3 position = new Vector3(0.0f, 1.0f, 10.0f);
        Vector3 target = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
        Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
        Vector3 velocity = new Vector3(0.0f, 0.0f, 0.0f);


        //temp stuff 
        Vector3 initialPosition = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 initialTarget = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 initialRight = new Vector3(1.0f, 0.0f, 0.0f);
        Vector3 initialUp = new Vector3(0.0f, 1.0f, 0.0f);

        public Vector3 InitialPosition
        {
            get { return initialPosition; }
            set { initialPosition = value; }
        }

        public Vector3 InitialTarget
        {
            get { return initialTarget; }
            set { initialTarget = value; }
        }

        public Vector3 InitialRight
        {
            get { return initialRight; }
            set { initialRight = value; }
        }

        public Vector3 InitialUp
        {
            get { return initialUp; }
            set { initialUp = value; }
        }


        //Things to add in eventually: 
        // - allow the real camera to break away from the in-game camera and draw the frustum 
        // - ability to follow catmull-rom splines 
        // - camera shake/waddle 
        #region Properties

        /// <summary> 
        /// View Matrix 
        /// </summary> 
        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        }

        /// <summary> 
        /// Projection Matrix 
        /// </summary> 
        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
            set { projectionMatrix = value; }
        }

        /// <summary> 
        /// Distance of the near clipping plane 
        /// </summary> 
        public float NearClip
        {
            get { return nearClip; }
            set { nearClip = value; }
        }


        /// <summary> 
        /// Distance of the far clipping plane 
        /// </summary> 
        public float FarClip
        {
            get { return farClip; }
            set { farClip = value; }
        }


        /// <summary> 
        /// Aspect ratio of the camera 
        /// </summary> 
        public float AspectRatio
        {
            get { return aspectRatio; }
            set { aspectRatio = value; }
        }

        /// <summary> 
        /// Field of view 
        /// </summary> 
        public float FieldOfView
        {
            get { return fieldOfView; }
            set { fieldOfView = value; }
        }


        /// <summary> 
        /// The target that the camera is pointed at 
        /// </summary> 
        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }


        /// <summary> 
        /// Position of the camera 
        /// </summary> 
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }


        /// <summary> 
        /// points to the right of the camera 
        /// </summary> 
        public Vector3 Right
        {
            get { return right; }
            set { right = value; }
        }

        /// <summary> 
        /// Velocity of the actual camera 
        /// </summary> 
        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }


        /// <summary> 
        /// Yaw of the camera 
        /// </summary> 
        public float Yaw
        {
            get { return yaw; }
            set { yaw = value; }
        }


        /// <summary> 
        /// Pitch of the camera 
        /// </summary> 
        public float Pitch
        {
            get { return pitch; }
            set { pitch = value; }
        }

        /// <summary> 
        /// Roll of the camera 
        /// </summary> 
        public float Roll
        {
            get { return roll; }
            set { roll = value; }
        }


        /// <summary> 
        /// Direction that is 'up' for the camera 
        /// </summary> 
        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }

        /// <summary> 
        /// Yaw velocity (side to side movement) 
        /// </summary> 
        public float YawVelocity
        {
            get { return yawVelocity; }
            set { yawVelocity = value; }
        }

        /// <summary> 
        /// Pitch Velocity 
        /// </summary> 
        public float PitchVelocity
        {
            get { return pitchVelocity; }
            set { pitchVelocity = value; }
        }

        /// <summary> 
        /// Roll velocity 
        /// </summary> 
        public float RollVelocity
        {
            get { return rollVelocity; }
            set { rollVelocity = value; }
        }


        /// <summary> 
        /// Sensitivity of the Yaw 
        /// </summary> 
        public float YawSensitivity
        {
            get { return yawSensitivity; }
            set { yawSensitivity = value; }
        }


        /// <summary> 
        /// Sensitivity of the Pitch 
        /// </summary> 
        public float PitchSensitivity
        {
            get { return pitchSensitivity; }
            set { pitchSensitivity = value; }
        }


        /// <summary> 
        /// Sensitivity of the roll 
        /// </summary> 
        public float RollSensitivity
        {
            get { return rollSensitivity; }
            set { rollSensitivity = value; }
        }
        #endregion



        /// <summary> 
        /// Constructor 
        /// </summary> 
        /// <param name="game"></param> 
        public CameraV2(Game game)
            : base(game)
        {

            viewMatrix = new Matrix();
            projectionMatrix = new Matrix();
            rotationMatrix = new Matrix();

        }


        /// <summary> 
        /// Sets up some starting matrices as well as sets up event handler for changes to the Graphics Device 
        /// </summary> 
        protected override void LoadContent()
        {
            Debug.WriteLine("Camera.LoadContent() called...");

            GraphicsDevice.DeviceReset +=new EventHandler<EventArgs>(GraphicsDevice_DeviceReset);

            aspectRatio = aspectRatio = GraphicsDevice.Viewport.AspectRatio;

            UpdateProjectionMatrix();
            UpdateViewMatrix();

            Mouse.SetPosition((int)(GraphicsDevice.Viewport.Width / 2.0f), (int)(GraphicsDevice.Viewport.Height / 2.0f));

            currentKeyboardState = Keyboard.GetState();
            previousKeyboardState = currentKeyboardState;

            currentMouseState = Mouse.GetState();
            previousMouseState = currentMouseState;

        }


        /// <summary> 
        /// Recalculate the projection matrix when the device has been reset 
        /// </summary> 
        /// <param name="sender"></param> 
        /// <param name="e"></param> 
        void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            Debug.WriteLine("GraphicsDevice_DeviceReset in CameraComponent called.  Calling UpdateProjectionMatrix()...");
            UpdateProjectionMatrix();
        }


        /// <summary> 
        /// Updates the projection matrix using the provided field of view, aspect ratio, and near and far clipping distances 
        /// </summary> 
        private void UpdateProjectionMatrix()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClip, farClip);
        }


        /// <summary> 
        /// Updates the view matrix using the position, target, and up members of the CameraComponent class 
        /// </summary> 
        private void UpdateViewMatrix()
        {
            viewMatrix = Matrix.CreateLookAt(position, target, up);
        }


        /// <summary> 
        /// Allows the game component to perform any initialization it needs to before starting 
        /// to run.  This is where it can query for any required services and load content. 
        /// </summary> 
        public override void Initialize()
        {
            Debug.WriteLine("Camera.Initialize() called...");

            //Set up some initial matrices 
            UpdateProjectionMatrix();
            UpdateViewMatrix();

            base.Initialize();
        }


        /// <summary> 
        /// Updates the camera movement based on the camera's velocity 
        /// </summary> 
        /// <param name="gameTime">Provides a snapshot of timing values.</param> 
        public override void Update(GameTime gameTime)
        {

            previousKeyboardState = currentKeyboardState;
            previousMouseState = currentMouseState;

            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();

            //Todo: reverse things if upside down... 

            float centerScreenX = GraphicsDevice.Viewport.Width / 2.0f;
            float centerScreenY = GraphicsDevice.Viewport.Height / 2.0f;

            //change in movement of mouse 
            float mouseDeltaX = previousMouseState.X - centerScreenX;
            float mouseDeltaY = previousMouseState.Y - centerScreenY;

            Mouse.SetPosition((int)centerScreenX, (int)centerScreenY);


            mouseDeltaX = mouseDeltaX * 0.005f;
            mouseDeltaY = mouseDeltaY * 0.005f * -1.0f;

            yaw -= mouseDeltaX;
            pitch += mouseDeltaY;

            if (currentKeyboardState.IsKeyDown(Keys.Q))
                roll += 0.002f * gameTime.ElapsedGameTime.Milliseconds * rollSensitivity;

            if (currentKeyboardState.IsKeyDown(Keys.E))
                roll -= 0.002f * gameTime.ElapsedGameTime.Milliseconds * rollSensitivity;

            //position += velocity; 
            //yaw += yawVelocity; 
            //pitch += pitchVelocity; 
            //roll += rollVelocity; 

            yaw = MathHelper.WrapAngle(yaw);
            pitch = MathHelper.WrapAngle(pitch);
            roll = MathHelper.WrapAngle(roll);


            //update the rotation matrix according to the yaw/pitch/roll.  Rotate the lookat point 
            //rotationMatrix = Matrix.Identity * Matrix.CreateFromYawPitchRoll(yaw, pitch, roll); 
            rotationMatrix = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);


            //right = rotationMatrix.Right; 

            //another thought - could do the rotation around the Y and X axis (which works), then rotate around 
            //the direction the camera is facing using this: 
            //http://www.xnawiki.com/index.php/Vector_Math#Translate_a_Point_around_an_Origin 

            //rotationMatrix = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw) * Matrix.CreateRotationZ(roll); 

            //Debug.WriteLine("yaw/pitch/roll: " + yaw + " " + pitch + " " + roll); 

            initialTarget = Vector3.Forward;
            initialUp = Vector3.Up;
            initialRight = Vector3.Right;
            initialPosition = Vector3.Zero;

            //rotate the target  
            initialTarget = Vector3.Transform(initialTarget, rotationMatrix);
            initialUp = Vector3.Transform(initialUp, rotationMatrix);
            initialRight = Vector3.Transform(initialRight, rotationMatrix);


            target = initialTarget + position;
            right = initialRight + position;


            //********** problem might be with not updating the camera position, up, and target vectors 
            //********** appropriately.  Not all of them are changing positions, etc. 

            //move forward 
            if (currentKeyboardState.IsKeyDown(Keys.W))
            {
                position += (initialTarget * 0.05f);
                target += (initialTarget * 0.05f);
                right += initialTarget * 0.05f;
                //up += initialTarget * 0.05f; 
            }


            //move backward 
            if (currentKeyboardState.IsKeyDown(Keys.S))
            {
                position -= (initialTarget * 0.05f);
                target -= initialTarget * 0.05f;
                right -= initialTarget * 0.05f;
                //up -= initialTarget * 0.05f; 
            }


            //strafe left 
            if (currentKeyboardState.IsKeyDown(Keys.A))
            {
                position -= initialRight * 0.05f;
                target -= initialRight * 0.05f;
                right -= initialRight * 0.05f;
                //up -= initialRight * 0.05f; 
            }

            //strafe right 
            if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                position += initialRight * 0.05f;
                target += initialRight * 0.05f;
                right += initialRight * 0.05f;
                //up += initialRight * 0.05f; 
            }

            up = initialUp + position;

            rotationMatrix.Translation = position;
            viewMatrix = Matrix.Invert(rotationMatrix);
            UpdateProjectionMatrix();



            base.Update(gameTime);
        }


        /// <summary> 
        /// Doesn't do anything.  CameraComponent is a DrawableGameComponent so that we have access 
        /// to the graphics device 
        /// </summary> 
        /// <param name="gameTime"></param> 
        public override void Draw(GameTime gameTime)
        {
            Debug.WriteLine("CameraComponent.Draw called...");
        }
    } 
}
