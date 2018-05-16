using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xna.Helpers._3D.Cameras
{

    #region Enums

    #endregion

  public class TargetCamera:Camera
    {
        #region Fields

       
        #endregion

        #region Properties

       

        #endregion

        #region Constructors

        public TargetCamera(float nearPlane, float farPlane, float fieldOfView, GraphicsDevice graphicsDevice, Vector3 position, Vector3 target) : base(nearPlane, farPlane, fieldOfView, graphicsDevice)
        {
            Position = position;
            Target = target;
        }

        public TargetCamera(GraphicsDevice graphicsDevice, Vector3 position, Vector3 target) : base(graphicsDevice)
        {
            Position = position;
            Target = target;
        }

        #endregion

        #region Methods
        public override void Update(GameTime gameTime)
        {
            Vector3 forward = Target - Position;
            Vector3 side = Vector3.Cross(forward, Vector3.Up);
            Vector3 up = Vector3.Cross(forward, side);
            View = Matrix.CreateLookAt(Position, Target, up);
            base.Update( gameTime);
        }
        #endregion

       
    }
}