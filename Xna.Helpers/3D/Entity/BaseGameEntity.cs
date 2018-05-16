#region

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Xna.Helpers._3D.Entity
{
    /// <summary>
    /// Base class for creating Game entity 
    /// </summary>
    public  class BaseGameEntity
    {

        #region Fields

        private static int _counterId;
        private static List<BaseGameEntity> _baseGameEntities;
        private readonly int _id;
        private Model _model;
        private Matrix[] _modelTransforms;
        private BoundingSphere _boundingSphere;
        private BoundingBox _boundingBox;

        #endregion

        #region Properties

        public static List<BaseGameEntity> BaseGameEntities
        {
            get { return _baseGameEntities; }
            set { _baseGameEntities = value; }
        }

        /// <summary>
        /// Position ofthe entity
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Rotation used by the entity
        /// </summary>
        public Vector3 Rotation { get; set; }
        /// <summary>
        /// Scale used by the entity
        /// </summary>
        public Vector3 Scale { get; set; }

        /// <summary>
        /// World matrix
        /// </summary>
        public Matrix WorldMatrix { get; set; }

        /// <summary>
        /// Id of the entity, auto generated
        /// </summary>
        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Graphics device used to draw model
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Model material
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Bounding sphere of the model
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get
            {
                Matrix worldMatrix = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
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

                Matrix worldTransform = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
                // For each mesh of the model)
                foreach (ModelMesh mesh in _model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        // Vertex buffer parameters
                        int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                        int vertexBufferSize = meshPart.NumVertices * vertexStride;

                        // Get vertex data as float
                        float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                        meshPart.VertexBuffer.GetData<float>(vertexData);

                        // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                        for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
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
        static BaseGameEntity()
        {
            _counterId = 0;
            _baseGameEntities = new List<BaseGameEntity>();
        }
        /// <summary>
        /// Deafult constructor, generates the Id, adds entity to static list of entities
        /// </summary>
        public BaseGameEntity(Vector3 position, Vector3 rotation, Vector3 scale, Model model, GraphicsDevice graphicsDevice)
        {
            _counterId++;
            _id = _counterId;
            _baseGameEntities.Add(this);
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


       

        #endregion

        #region Methods
        /// <summary>
        /// Draws the model
        /// </summary>
        /// <param name="view">View matrix</param>
        /// <param name="projection">Projection matrix</param>
        public virtual void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            Matrix baseWorld = Matrix.CreateScale(Scale) *
                            Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                            Matrix.CreateTranslation(Position);

            foreach (var mesh in _model.Meshes)
            {
                Matrix localWorld = _modelTransforms[mesh.ParentBone.Index] * baseWorld;

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
            foreach (ModelMesh mesh in _model.Meshes)
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
                effect.Parameters[paramName].SetValue((Vector3)val);
            else if (val is bool)
                effect.Parameters[paramName].SetValue((bool)val);
            else if (val is Matrix)
                effect.Parameters[paramName].SetValue((Matrix)val);
            else if (val is Texture2D)
                effect.Parameters[paramName].SetValue((Texture2D)val);
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

                    MeshTag tag = ((MeshTag)part.Tag);

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
                    ((MeshTag)meshPart.Tag).Material = material;
            }
        }

        private void GenerateTags()
        {
            foreach (ModelMesh mesh in _model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (part.Effect is BasicEffect)
                    {
                        BasicEffect effect = (BasicEffect)part.Effect;
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
                    ((MeshTag)part.Tag).CachedEffect = part.Effect;
        }

        /// <summary>
        /// Restore the effects referenced by the model's cache
        /// </summary>
        public void RestoreEffects()
        {
            foreach (ModelMesh mesh in _model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (((MeshTag)part.Tag).CachedEffect != null)
                        part.Effect = ((MeshTag)part.Tag).CachedEffect;
        }

        private void BuildBoundingSphere()
        {
            BoundingSphere boundingSphere = new BoundingSphere(Vector3.Zero, 0);
            

            //then in the foreach loop
          //  int tempIndex = Index++;
          
          //  boundingSpheres[tempIndex].Center += meshTransforms[mesh.ParentBone.Index].Translation;
            foreach (var modelMesh in _model.Meshes)
            {
             //   Microsoft.Xna.Framework.BoundingSphere boundingSphereTemp = modelMesh.BoundingSphere;

                  BoundingSphere boundingSphereTransformed = modelMesh.BoundingSphere.Transform(_modelTransforms[modelMesh.ParentBone.Index]);
                    boundingSphere = BoundingSphere.CreateMerged(boundingSphere, boundingSphereTransformed);
                //if (boundingSphere.Radius == 0)
                //    boundingSphere = modelMesh.BoundingSphere;
                //else
                //    boundingSphere = BoundingSphere.CreateMerged(boundingSphere, modelMesh.BoundingSphere);
                   
            }
            _boundingSphere = boundingSphere;
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


        ///// <summary>
        ///// Override this to draw
        ///// </summary> 
        //public virtual void Draw()
        //{
        //}
        ///// <summary>
        ///// Override this to Update
        ///// </summary>
        ///// <param name="gameTime"></param>
        //public virtual void Update(GameTime gameTime)
        //{
        //}


        ///// <summary>
        ///// Draws all BaseGameEntities 
        ///// </summary>
        ///// <param name="spriteBatch"></param>
        ////public static void DrawAll(SpriteBatch spriteBatch)
        //{
        //    for (int i = _baseGameEntities.Count - 1; i >= 0; i--)
        //    {
        //        BaseGameEntity baseGameEntity = _baseGameEntities[i];
        //        baseGameEntity.Draw(spriteBatch);
        //    }
        //}
        ///// <summary>
        ///// Updates all BaseGameEntities
        ///// </summary>
        ///// <param name="gameTime"></param>
        //public static void UpdateAll(GameTime gameTime)
        //{
        //    for (int i = _baseGameEntities.Count - 1; i >= 0; i--)
        //    {
        //        BaseGameEntity baseGameEntity = _baseGameEntities[i];
        //        baseGameEntity.Update(gameTime);
        //    }
        //}
    }
}