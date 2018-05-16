#region

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Xna.Helpers._3D
{
    public class SkyBox : IRenderable
    {
        private CModel _model;
        private Effect _effect;
        private GraphicsDevice graphics;

        public SkyBox(Model model, GraphicsDevice graphicsDevice,
                         TextureCube texture)
        {
            _model = new CModel(Vector3.Zero, Vector3.Zero, Vector3.One, model, graphicsDevice);

            _effect = EffectManager.Content.Load<Effect>("skysphere_effect");
            _effect.Parameters["CubeMap"].SetValue(texture);

            _model.SetModelEffect(_effect, false);

            graphics = graphicsDevice;
        }

        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            // Disable the depth buffer
            graphics.DepthStencilState = DepthStencilState.None;

            // Move the model with the sphere
            _model.Position = cameraPosition;

            _model.Draw(view, projection, cameraPosition);

            graphics.DepthStencilState = DepthStencilState.Default;
        }

        public void SetClipPlane(Vector4? Plane)
        {
            _effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

            if (Plane.HasValue)
                _effect.Parameters["ClipPlane"].SetValue(Plane.Value);
        }
    }
}