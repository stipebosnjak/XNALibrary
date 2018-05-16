using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Xna.Helpers._3D.Cameras;

namespace Xna.Helpers._3D
{
    public interface IRenderable
    {
        void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition);
        void SetClipPlane(Vector4? Plane);
    }

    // Make Water IRenderable
    public class Water
    {
        CModel waterMesh;
        Effect waterEffect;

  
        GraphicsDevice graphics;

        RenderTarget2D reflectionTarg;
        public List<IRenderable> Objects = new List<IRenderable>();

        public Water(Model model, GraphicsDevice graphics,
            Vector3 position, Vector2 size,Texture2D normalMap)
        {
           
            this.graphics = graphics;

            waterMesh = new CModel( position,
                Vector3.Zero, new Vector3(size.X, 1, size.Y),model, graphics);

            waterEffect = EffectManager.Content.Load<Effect>("WaterEffect");
            waterMesh.SetModelEffect(waterEffect, false);

            waterEffect.Parameters["viewportWidth"].SetValue(
                graphics.Viewport.Width);

            waterEffect.Parameters["viewportHeight"].SetValue(
                graphics.Viewport.Height);

            waterEffect.Parameters["WaterNormalMap"].SetValue(normalMap);

            reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width,
                graphics.Viewport.Height, false, SurfaceFormat.Color, 
                DepthFormat.Depth24);
        }

        public void renderReflection(Camera camera,GameTime gameTime)
        {
            // Reflect the camera's properties across the water plane
            Vector3 reflectedCameraPosition = camera.Position;
            reflectedCameraPosition.Y = -reflectedCameraPosition.Y
                + waterMesh.Position.Y * 2;

            Vector3 reflectedCameraTarget = camera.Target;
            reflectedCameraTarget.Y = -reflectedCameraTarget.Y
                + waterMesh.Position.Y * 2;

            // Create a temporary camera to render the reflected scene
            Camera reflectionCamera = new TargetCamera(graphics,reflectedCameraPosition, reflectedCameraTarget);

            reflectionCamera.Update(gameTime);

            // Set the reflection camera's view matrix to the water effect
            waterEffect.Parameters["ReflectedView"].SetValue(
                reflectionCamera.View);

            // Create the clip plane
            Vector4 clipPlane = new Vector4(0, 1, 0, -waterMesh.Position.Y);

            // Set the render target
            graphics.SetRenderTarget(reflectionTarg);
            graphics.Clear(Color.Black);

            // Draw all objects with clip plane
            foreach (IRenderable renderable in Objects)
            {
                renderable.SetClipPlane(clipPlane);

                renderable.Draw(reflectionCamera.View, 
                    reflectionCamera.Projection, 
                    reflectedCameraPosition);

                renderable.SetClipPlane(null);
            }

            graphics.SetRenderTarget(null);

            // Set the reflected scene to its effect parameter in
            // the water effect
            waterEffect.Parameters["ReflectionMap"].SetValue(reflectionTarg);
        }

        public void PreDraw(Camera camera, GameTime gameTime)
        {
            renderReflection(camera,gameTime);
            waterEffect.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            waterMesh.Draw(View, Projection, CameraPosition);
        }
    }
}
