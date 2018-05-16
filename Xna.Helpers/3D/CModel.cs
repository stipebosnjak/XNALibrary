#region

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


#endregion

namespace Xna.Helpers._3D
{

    #region Enums

    #endregion

    /// <summary>
    /// Class for creating a 3D model
    /// </summary>
    public class CModel : IRenderable
    {
        #region Fields

        private Model _model;
        private Matrix[] _modelTransforms;
        private BoundingSphere _boundingSphere;
        private BoundingBox _boundingBox;

        private Matrix _worldMatrix;
        private Vector3 _acceleration;
       
        #endregion

        #region Properties

        /// <summary>
        /// Model position
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Model rotation
        /// </summary>
        public Vector3 Rotation { get; set; }

        /// <summary>
        /// Model velocity
        /// </summary>
        public Vector3 Velocity { get; set; }
        /// <summary>
        /// Model max speed
        /// </summary>
        public float MaxForce { get; set; }
        /// <summary>
        /// Model max speed
        /// </summary>
        public float MaxSpeed { get; set; }
        /// <summary>
        /// Model mass
        /// </summary>
        public float Mass { get; set; }
        /// <summary>
        /// Model speed
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Model scale
        /// </summary>
        public Vector3 Scale { get; set; }

        /// <summary>
        /// Positon for use Lerp
        /// </summary>
        public Vector3 GeneratedPosition { get; set; }

        /// <summary>
        /// Model material
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Graphics device used to draw model
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Is model AI enabled
        /// </summary>
        public bool IsAi { get; set; }

        /// <summary>
        /// The model is static , it will not process updateinput
        /// </summary>
        public bool IsStatic { get; set; }

        public Model Model
        {
            get { return _model; }
            set { _model = value; }
        }
      

        /// <summary>
        /// Bounding sphere of the model
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get
            {
                Matrix worldMatrix = Matrix.CreateScale(Scale)*Matrix.CreateTranslation(Position);
                BoundingSphere transformedBoundingSphere = _boundingSphere.Transform(worldMatrix);
                return transformedBoundingSphere;
            }
        }
       
        /// <summary>
        /// Bounding box of a model
        /// </summary>
        public BoundingBox BoundingBox
        {
            get
            {
                // Initialize minimum and maximum corners of the bounding box to max and min values
                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                Matrix worldTransform = Matrix.CreateScale(Scale)*Matrix.CreateTranslation(Position);
                // For each mesh of the model)
                foreach (ModelMesh mesh in _model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        // Vertex buffer parameters
                        int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                        int vertexBufferSize = meshPart.NumVertices*vertexStride;

                        // Get vertex data as float
                        float[] vertexData = new float[vertexBufferSize/sizeof (float)];
                        meshPart.VertexBuffer.GetData<float>(vertexData);

                        // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                        for (int i = 0; i < vertexBufferSize/sizeof (float); i += vertexStride/sizeof (float))
                        {
                            Vector3 transformedPosition =
                                Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]),
                                                  worldTransform);

                            min = Vector3.Min(min, transformedPosition);
                            max = Vector3.Max(max, transformedPosition);
                        }
                    }
                }

                // Create and return bounding box
                return new BoundingBox(min, max);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new custom model
        /// </summary>
        /// <param name="position">Starting position</param>
        /// <param name="rotation">Starting rotation</param>
        /// <param name="scale">Starting scale</param>
        /// <param name="model">Model</param>
        /// <param name="graphicsDevice">Graphics device used by the model</param>
        public CModel(Vector3 position, Vector3 rotation, Vector3 scale, Model model, GraphicsDevice graphicsDevice)
        {
            _model = model;

            _modelTransforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_modelTransforms);

            BuildBoundingSphere();
            GenerateTags();

            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
           // _steeringBehaviours = new SteeringBehaviours(this);
            GraphicsDevice = graphicsDevice;
          
           
            Material = new Material();
        }
        /// <summary>
        /// Constructor for creating model with randomized properties(position, scale)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="graphicsDevice"></param>
        /// <param name="posMin"></param>
        /// <param name="posMax"></param>
        /// <param name="scaleMin"></param>
        /// <param name="scaleMax"></param>
        public CModel(Model model, GraphicsDevice graphicsDevice,float posMin, float posMax,float scaleMin,float scaleMax)
        {
            _model = model;

            _modelTransforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_modelTransforms);

            BuildBoundingSphere();
            GenerateTags();
            this.Rotation = Vector3.Zero;
            RandomizeProperties(posMin, posMax, scaleMin, scaleMax);

            GraphicsDevice = graphicsDevice;
         //   _steeringBehaviours = new SteeringBehaviours(this);
            Material = new Material();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Draws the model
        /// </summary>
        /// <param name="view">View matrix</param>
        /// <param name="projection">Projection matrix</param>
        public void Draw(Matrix view, Matrix projection,Vector3 cameraPosition)
        {
            Matrix baseWorld = Matrix.CreateScale(Scale) *
                            Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                            Matrix.CreateTranslation(Position);

            foreach (var mesh in _model.Meshes)
            {
                Matrix localWorld = _modelTransforms[mesh.ParentBone.Index]*baseWorld;

                foreach (var meshPart in mesh.MeshParts)
                {
                    Effect effect = meshPart.Effect;

                    if (effect is BasicEffect)
                    { 
                        ((BasicEffect)effect).World = localWorld;
                        ((BasicEffect)effect).View = view;
                        ((BasicEffect)effect).Projection = projection;
                        ((BasicEffect)effect).EnableDefaultLighting();
                    }
                    else
                    {
                        SetEffectParameter(effect, "World", localWorld);
                        SetEffectParameter(effect, "View", view);
                        SetEffectParameter(effect, "Projection", projection);
                        SetEffectParameter(effect, "CameraPosition", cameraPosition);
                    }
                    ((MeshTag)meshPart.Tag).Material.SetEffectParameters(effect);
                    // Material.SetEffectParameters(effect);
                }
                mesh.Draw();
            }
        }

        public void SetClipPlane(Vector4? Plane)
        {
            foreach (ModelMesh mesh in Model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (part.Effect.Parameters["ClipPlaneEnabled"] != null)
                        part.Effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

                    if (Plane.HasValue)
                        if (part.Effect.Parameters["ClipPlane"] != null)
                            part.Effect.Parameters["ClipPlane"].SetValue(Plane.Value);
                }
        }

        private void SetEffectParameter(Effect effect, string paramName, object val)
        {
            if (effect.Parameters[paramName] == null)
                return;

            if (val is Vector3)
                effect.Parameters[paramName].SetValue((Vector3) val);
            else if (val is bool)
                effect.Parameters[paramName].SetValue((bool) val);
            else if (val is Matrix)
                effect.Parameters[paramName].SetValue((Matrix) val);
            else if (val is Texture2D)
                effect.Parameters[paramName].SetValue((Texture2D) val);
        }

        public void SetModelEffect(Effect effect, bool copyEffect)
        {
            foreach (ModelMesh mesh in _model.Meshes)
                SetMeshEffect(mesh.Name, effect, copyEffect);
        }

        public void SetModelMaterial(Material material)
        {
            foreach (ModelMesh mesh in _model.Meshes)
                SetMeshMaterial(mesh.Name, material);
        }

        public void SetMeshEffect(string meshName, Effect effect, bool copyEffect)
        {
            foreach (ModelMesh mesh in _model.Meshes)
            {
                if (mesh.Name != meshName)
                    continue;

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Effect toSet = effect;

                    // Copy the effect if necessary
                    if (copyEffect)
                        toSet = effect.Clone();

                    MeshTag tag = ((MeshTag) part.Tag);

                    // If this ModelMeshPart has a texture, set it to the effect
                    if (tag.Texture != null)
                    {
                        SetEffectParameter(toSet, "BasicTexture", tag.Texture);
                        SetEffectParameter(toSet, "TextureEnabled", true);
                    }
                    else
                        SetEffectParameter(toSet, "TextureEnabled", false);

                    // Set our remaining parameters to the effect
                    SetEffectParameter(toSet, "DiffuseColor", tag.Color);
                    SetEffectParameter(toSet, "SpecularPower", tag.SpecularPower);

                    part.Effect = toSet;
                }
            }
        }

        /// <summary>
        /// Sets the material for the mesh
        /// </summary>
        /// <param name="meshName"></param>
        /// <param name="material"></param>
        public void SetMeshMaterial(string meshName, Material material)
        {
            foreach (ModelMesh mesh in _model.Meshes)
            {
                if (mesh.Name != meshName)
                    continue;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    ((MeshTag) meshPart.Tag).Material = material;
            }
        }

        private void GenerateTags()
        {
            foreach (ModelMesh mesh in _model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (part.Effect is BasicEffect)
                    {
                        BasicEffect effect = (BasicEffect) part.Effect;
                        MeshTag tag = new MeshTag(effect.DiffuseColor,
                                                  effect.Texture, effect.SpecularPower);
                        part.Tag = tag;
                    }
        }

        /// <summary>
        /// Store references to all of the model's current effects
        /// </summary>
        public void CacheEffects()
        {
            foreach (ModelMesh mesh in _model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    ((MeshTag) part.Tag).CachedEffect = part.Effect;
        }

        /// <summary>
        /// Restore the effects referenced by the model's cache
        /// </summary>
        public void RestoreEffects()
        {
            foreach (ModelMesh mesh in _model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (((MeshTag) part.Tag).CachedEffect != null)
                        part.Effect = ((MeshTag) part.Tag).CachedEffect;
        }

        private void BuildBoundingSphere()
        {
            BoundingSphere boundingSphere = new BoundingSphere(Vector3.Zero, 0);

            foreach (var modelMesh in _model.Meshes)
            {
             //   BoundingSphere boundingSphereTransformed = modelMesh.BoundingSphere.Transform(_modelTransforms[modelMesh.ParentBone.Index]);
           //    boundingSphere = BoundingSphere.CreateMerged(boundingSphere, boundingSphereTransformed);
                if (boundingSphere.Radius == 0)
                    boundingSphere = modelMesh.BoundingSphere;
                else
                boundingSphere = BoundingSphere.CreateMerged(boundingSphere, modelMesh.BoundingSphere);
            }
            _boundingSphere = boundingSphere;
        }

        /// <summary>
        /// Updates the model with default input
        /// </summary>
        public void UpdateInput(GameTime gameTime)
        {
            if (IsStatic)return;

            Matrix rotation;
            if (IsAi)
            {
                // Determine what direction to move in
                 rotation = Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
                // Move in the direction dictated by our rotation matrix
                Position += Vector3.Transform(Vector3.Forward, rotation) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 20;
                return;
            }

            KeyboardState keyState = Keyboard.GetState();
            Vector3 rotChange = new Vector3(0, 0, 0);
            // Determine on which axes the ship should be rotated on, if any
            if (keyState.IsKeyDown(Keys.W))
                rotChange += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.S))
                rotChange += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.A))
                rotChange += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.D))
                rotChange += new Vector3(0, -1, 0);

            Rotation += rotChange * .025f;
            // Determine what direction to move in
            rotation = Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            // Move in the direction dictated by our rotation matrix
            Position += Vector3.Transform(Vector3.Forward, rotation) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 20;
        }

        /// <summary>
        /// It will randomize  the properties like position, scale 
        /// </summary>
        private void RandomizeProperties(float posMin, float posMax,float scaleMin,float scaleMax)
        {
            Position = Generators.RandomVector3(posMin, posMax);
            Scale = Vector3.One*Generators.RandomNumber(scaleMin, scaleMax);
        }

     

        #endregion

        #region MeshTag

        public class MeshTag
        {
            public Vector3 Color;
            public Texture2D Texture;
            public float SpecularPower;
            public Effect CachedEffect = null;
            public Material Material = new Material();

            public MeshTag(Vector3 color, Texture2D texture, float specularPower)
            {
                this.Color = color;
                this.Texture = texture;
                this.SpecularPower = specularPower;
            }

        }

        #endregion
    }
}