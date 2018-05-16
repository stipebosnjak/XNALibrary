#region

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Xna.Helpers._3D
{
    public class PrelightingRenderer
    {
        // Normal, depth, and light map render targets
        private RenderTarget2D depthTarg;
        private RenderTarget2D normalTarg;
        private RenderTarget2D lightTarg;

        // Depth/normal effect and light mapping effect
        private Effect depthNormalEffect;
        private Effect lightingEffect;
        private Effect _pPModel;
        // Point light (sphere) mesh
        private Model lightMesh;

        // List of models, lights, and the camera
        public List<CModel> Models { get; set; }
        public List<PPPointLight> Lights { get; set; }
        public Camera Camera { get; set; }

        private GraphicsDevice graphicsDevice;
        private int viewWidth, viewHeight;

        // Position and target of the shadowing light
        public Vector3 ShadowLightPosition { get; set; }
        public Vector3 ShadowLightTarget { get; set; }

        // Shadow depth target and depth-texture effect
        private RenderTarget2D shadowDepthTarg;
        private Effect shadowDepthEffect;

        // Depth texture parameters
        private int shadowMapSize = 1024;
        private int shadowFarPlane = 10000;

        // Shadow light view and projection
        private Matrix shadowView, shadowProjection;

        // Shadow properties
        public bool DoShadowMapping { get; set; }
        public float ShadowMult { get; set; }

        public Effect PpModel
        {
            get { return _pPModel; }
            set { _pPModel = value; }
        }

        private SpriteBatch spriteBatch;
        private RenderTarget2D shadowBlurTarg;
        private Effect shadowBlurEffect;

        public PrelightingRenderer(GraphicsDevice GraphicsDevice)
        {
            viewWidth = GraphicsDevice.Viewport.Width;
            viewHeight = GraphicsDevice.Viewport.Height;

            // Create the three render targets
            depthTarg = new RenderTarget2D(GraphicsDevice, viewWidth,
                                           viewHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);

            normalTarg = new RenderTarget2D(GraphicsDevice, viewWidth,
                                            viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);

            lightTarg = new RenderTarget2D(GraphicsDevice, viewWidth,
                                           viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);

            // Load effects
            depthNormalEffect = EffectManager.Content.Load<Effect>("PPDepthNormal");
            lightingEffect = EffectManager.Content.Load<Effect>("PPLight");
            _pPModel = EffectManager.Content.Load<Effect>("PPModel");
            // Set effect parameters to light mapping effect
            lightingEffect.Parameters["viewportWidth"].SetValue(viewWidth);
            lightingEffect.Parameters["viewportHeight"].SetValue(viewHeight);

            // Load point light mesh and set light mapping effect to it
            lightMesh = EffectManager.Content.Load<Model>("PPLightMesh");
            lightMesh.Meshes[0].MeshParts[0].Effect = lightingEffect;

            shadowDepthTarg = new RenderTarget2D(GraphicsDevice, shadowMapSize,
                                                 shadowMapSize, false, SurfaceFormat.HalfVector2, DepthFormat.Depth24);

            shadowDepthEffect = EffectManager.Content.Load<Effect>("ShadowDepthEffect");
            shadowDepthEffect.Parameters["FarPlane"].SetValue(shadowFarPlane);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            shadowBlurEffect = EffectManager.Content.Load<Effect>("GaussianBlur");

            shadowBlurTarg = new RenderTarget2D(GraphicsDevice, shadowMapSize,
                                                shadowMapSize, false, SurfaceFormat.HalfVector2, DepthFormat.Depth24);

            graphicsDevice = GraphicsDevice;
        }

        public void Draw()
        {
            DrawDepthNormalMap();
            DrawLightMap();

            if (DoShadowMapping)
            {
                DrawShadowDepthMap();
                BlurShadow(shadowBlurTarg, shadowDepthTarg, 0);
                BlurShadow(shadowDepthTarg, shadowBlurTarg, 1);
            }

            PrepareMainPass();
        }

        private void DrawDepthNormalMap()
        {
            // Set the render targets to 'slots' 1 and 2
            graphicsDevice.SetRenderTargets(normalTarg, depthTarg);

            // Clear the render target to 1 (infinite depth)
            graphicsDevice.Clear(Color.White);

            // Draw each model with the PPDepthNormal effect
            foreach (CModel model in Models)
            {
                model.CacheEffects();
                model.SetModelEffect(depthNormalEffect, false);
                model.Draw(Camera.View, Camera.Projection,
                           Camera.Position);
                model.RestoreEffects();
            }

            // Un-set the render targets
            graphicsDevice.SetRenderTargets(null);
        }

        private void DrawShadowDepthMap()
        {
            // Calculate view and projection matrices for the "light"
            // shadows are being calculated for
            shadowView = Matrix.CreateLookAt(ShadowLightPosition,
                                             ShadowLightTarget, Vector3.Up);

            shadowProjection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), 1, 1, shadowFarPlane);

            // Set render target
            graphicsDevice.SetRenderTarget(shadowDepthTarg);

            // Clear the render target to 1 (infinite depth)
            graphicsDevice.Clear(Color.White);

            // Draw each model with the ShadowDepthEffect effect
            foreach (CModel model in Models)
            {
                model.CacheEffects();
                model.SetModelEffect(shadowDepthEffect, false);
                model.Draw(shadowView, shadowProjection, ShadowLightPosition);
                model.RestoreEffects();
            }

            // Un-set the render targets
            graphicsDevice.SetRenderTarget(null);
        }

        private void DrawLightMap()
        {
            // Set the depth and normal map info to the effect
            lightingEffect.Parameters["DepthTexture"].SetValue(depthTarg);
            lightingEffect.Parameters["NormalTexture"].SetValue(normalTarg);

            // Calculate the view * projection matrix
            Matrix viewProjection = Camera.View*Camera.Projection;

            // Set the inverse of the view * projection matrix to the effect
            Matrix invViewProjection = Matrix.Invert(viewProjection);
            lightingEffect.Parameters["InvViewProjection"].SetValue(
                invViewProjection);

            // Set the render target to the graphics device
            graphicsDevice.SetRenderTarget(lightTarg);

            // Clear the render target to black (no light)
            graphicsDevice.Clear(Color.Black);

            // Set render states to additive (lights will add their influences)
            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.DepthStencilState = DepthStencilState.None;

            foreach (PPPointLight light in Lights)
            {
                // Set the light's parameters to the effect
                light.SetEffectParameters(lightingEffect);

                // Calculate the world * view * projection matrix and set it to 
                // the effect
                Matrix wvp = (Matrix.CreateScale(light.Attenuation)
                              *Matrix.CreateTranslation(light.Position))*viewProjection;

                lightingEffect.Parameters["WorldViewProjection"].SetValue(wvp);

                // Determine the distance between the light and camera
                float dist = Vector3.Distance(Camera.Position,
                                              light.Position);

                // If the camera is inside the light-sphere, invert the cull mode
                // to draw the inside of the sphere instead of the outside
                if (dist < light.Attenuation)
                    graphicsDevice.RasterizerState =
                        RasterizerState.CullClockwise;

                // Draw the point-light-sphere
                lightMesh.Meshes[0].Draw();

                // Revert the cull mode
                graphicsDevice.RasterizerState =
                    RasterizerState.CullCounterClockwise;
            }

            // Revert the blending and depth render states
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Un-set the render target
            graphicsDevice.SetRenderTarget(null);
        }

        private void PrepareMainPass()
        {
            foreach (CModel model in Models)
                foreach (ModelMesh mesh in model.Model.Meshes)
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        // Set the light map and viewport parameters to each model's effect
                        if (part.Effect.Parameters["LightTexture"] != null)
                            part.Effect.Parameters["LightTexture"].SetValue(lightTarg);

                        if (part.Effect.Parameters["viewportWidth"] != null)
                            part.Effect.Parameters["viewportWidth"].SetValue(viewWidth);

                        if (part.Effect.Parameters["viewportHeight"] != null)
                            part.Effect.Parameters["viewportHeight"].SetValue(viewHeight);

                        if (part.Effect.Parameters["DoShadowMapping"] != null)
                            part.Effect.Parameters["DoShadowMapping"].SetValue(DoShadowMapping);

                        if (!DoShadowMapping) continue;

                        if (part.Effect.Parameters["ShadowMap"] != null)
                            part.Effect.Parameters["ShadowMap"].SetValue(shadowDepthTarg);

                        if (part.Effect.Parameters["ShadowView"] != null)
                            part.Effect.Parameters["ShadowView"].SetValue(shadowView);

                        if (part.Effect.Parameters["ShadowProjection"] != null)
                            part.Effect.Parameters["ShadowProjection"].SetValue(shadowProjection);

                        if (part.Effect.Parameters["ShadowLightPosition"] != null)
                            part.Effect.Parameters["ShadowLightPosition"].SetValue(ShadowLightPosition);

                        if (part.Effect.Parameters["ShadowFarPlane"] != null)
                            part.Effect.Parameters["ShadowFarPlane"].SetValue(shadowFarPlane);

                        if (part.Effect.Parameters["ShadowMult"] != null)
                            part.Effect.Parameters["ShadowMult"].SetValue(ShadowMult);
                    }
        }

        private void BlurShadow(RenderTarget2D to, RenderTarget2D from, int dir)
        {
            // Set the target render target
            graphicsDevice.SetRenderTarget(to);

            graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            // Start the Gaussian blur effect
            shadowBlurEffect.CurrentTechnique.Passes[dir].Apply();

            // Draw the contents of the source render target so they can
            // be blurred by the gaussian blur pixel shader
            spriteBatch.Draw(from, Vector2.Zero, Color.White);

            spriteBatch.End();

            // Clean up after the sprite batch
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Remove the render target
            graphicsDevice.SetRenderTarget(null);
        }
        /// <summary>
        /// Returns a list of Point lights randomized trough x-axis and z-axis
        /// </summary>
        /// <param name="xRadius"></param>
        /// <param name="numberOfLights"></param>
        /// <returns></returns>
        public static List<PPPointLight> RandomizeLights(float xRadius, int numberOfLights)
        {
            List<PPPointLight> lights = new List<PPPointLight>();
            for (int i = 0; i < numberOfLights; i++)
            {
                lights.Add(
                    new PPPointLight(
                        new Vector3(Generators.RandomNumber(-xRadius, xRadius), 100,
                                    Generators.RandomNumber(-xRadius, xRadius)),
                        new Color(Generators.RandomVector3(0, 255)), xRadius/numberOfLights*3));
            }
            return lights;
        }
    }
}