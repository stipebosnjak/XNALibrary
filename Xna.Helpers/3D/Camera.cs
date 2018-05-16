#region

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Xna.Helpers._3D
{

    #region Enums

    #endregion

    /// <summary>
    ///   Base class for all cameras
    /// </summary>
    public abstract class Camera
    {
        #region Fields

        private Matrix _view, _projection;

        private float _nearPlane, _farPlane;
        private GraphicsDevice _graphicsDevice;

        #endregion

        #region Properties

        /// <summary>
        ///   View matrix
        /// </summary>
        public Matrix View
        {
            get { return _view; }
          protected  set
            {
                _view = value;
                GenerateFrustrum();
            }
        }

        /// <summary>
        ///   Projection matrix
        /// </summary>
        public Matrix Projection
        {
            get { return _projection; }
           protected set
            {
                _projection = value;
                GenerateFrustrum();
            }
        }

        /// <summary>
        ///   Camera position
        /// </summary>
        public Vector3 Position { get; protected set; }

        /// <summary>
        ///   Cameras target
        /// </summary>
        public Vector3 Target { get; protected set; }

        /// <summary>
        /// </summary>
        public BoundingFrustum BoundingFrustum { get; private set; }

        protected MouseState LastMouseState { get; set; }

        protected MouseState CurrentMouseState { get; set; }

        public bool DefaultInput { get; set; }
        #endregion

        #region Constructors

        protected Camera(float nearPlane, float farPlane, float fieldOfView,
                         GraphicsDevice graphicsDevice)
        {
            _nearPlane = nearPlane;
            _farPlane = farPlane;
            _graphicsDevice = graphicsDevice;
            GeneratePerspectiveProjectionMatrix(fieldOfView);
            DefaultInput = true;
        }

        protected Camera(GraphicsDevice graphicsDevice)
        {
            _nearPlane = 0.1f;
            _farPlane = 10000000f;
            _graphicsDevice = graphicsDevice;
            GeneratePerspectiveProjectionMatrix();
            DefaultInput = true;
        }

        #endregion

        #region Methods

        private void GeneratePerspectiveProjectionMatrix(float fieldOfView=45f)
        {
            PresentationParameters pp = _graphicsDevice.PresentationParameters;
            float aspectRatio = pp.BackBufferWidth/
                                (float) pp.BackBufferHeight;
          _projection=  Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fieldOfView), aspectRatio, _nearPlane, _farPlane);
        }
        /// <summary>
        /// Returns bool is frustrum contains sphere
        /// </summary>
        /// <param name="sphere"></param>
        /// <returns></returns>
        public bool BoundingVolumeIsInView(BoundingSphere sphere)
        {
            return (BoundingFrustum.Contains(sphere) != ContainmentType.Disjoint);
        }
        /// <summary>
        /// Returns bool is frustrum contains box
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public bool BoundingVolumeIsInView(BoundingBox box)
        {
            return (BoundingFrustum.Contains(box) != ContainmentType.Disjoint);
        }
        /// <summary>
        ///   Used to update the camera
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
        }

        private void GenerateFrustrum()
        {
            Matrix viewProjection = View*Projection;
            BoundingFrustum = new BoundingFrustum(viewProjection);
        }

        #endregion
    }
}