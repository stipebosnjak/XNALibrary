#region

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Xna.Helpers._3D
{
    public class BillboardSystem
    {
        // Vertex buffer and index buffer, particle
        // and index arrays
        private VertexBuffer _verts;
        private IndexBuffer _ints;
        private VertexPositionTexture[] _particles;
        private int[] _indices;

        // Billboard settings
        private int nBillboards;
        private Vector2 billboardSize;
        private Texture2D texture;

        // GraphicsDevice and Effect
        private GraphicsDevice graphicsDevice;
        private Effect effect;

        public bool EnsureOcclusion = true;

        public enum BillboardMode
        {
            Cylindrical,
            Spherical
        };

        public BillboardMode Mode = BillboardMode.Spherical;

        public BillboardSystem(GraphicsDevice graphicsDevice, Texture2D texture,
                               Vector2 billboardSize, Vector3[] particlePositions)
        {
            nBillboards = particlePositions.Length;
            this.billboardSize = billboardSize;
            this.graphicsDevice = graphicsDevice;
            this.texture = texture;

            effect = EffectManager.Content.Load<Effect>("BillboardEffect");

            GenerateParticles(particlePositions);
        }
        public BillboardSystem(GraphicsDevice graphicsDevice, Texture2D texture,
                              Vector2 billboardSize,int numberOfTrees,Vector3 sizeAndYpos)
        {
            
            Vector3[] particlePositions=new Vector3[numberOfTrees];
            for (int i = 0; i < numberOfTrees; i++)
                particlePositions[i] = new Vector3(Generators.RandomNumber(-sizeAndYpos.X,sizeAndYpos.X),sizeAndYpos.Y,Generators.RandomNumber(-sizeAndYpos.Z,sizeAndYpos.Z));
            nBillboards = numberOfTrees;
            this.billboardSize = billboardSize;
            this.graphicsDevice = graphicsDevice;
            this.texture = texture;

            effect = EffectManager.Content.Load<Effect>("BillboardEffect");

            GenerateParticles(particlePositions);
        }
        private void GenerateParticles(Vector3[] particlePositions)
        {
            // Create vertex and index arrays
            _particles = new VertexPositionTexture[nBillboards*4];
            _indices = new int[nBillboards*6];

            int x = 0;

            // For each billboard...
            for (int i = 0; i < nBillboards*4; i += 4)
            {
                Vector3 pos = particlePositions[i/4];

                // Add 4 vertices at the billboard's position
                _particles[i + 0] = new VertexPositionTexture(pos, new Vector2(0, 0));
                _particles[i + 1] = new VertexPositionTexture(pos, new Vector2(0, 1));
                _particles[i + 2] = new VertexPositionTexture(pos, new Vector2(1, 1));
                _particles[i + 3] = new VertexPositionTexture(pos, new Vector2(1, 0));

                // Add 6 indices to form two triangles
                _indices[x++] = i + 0;
                _indices[x++] = i + 3;
                _indices[x++] = i + 2;
                _indices[x++] = i + 2;
                _indices[x++] = i + 1;
                _indices[x++] = i + 0;
            }

            // Create and set the vertex buffer
            _verts = new VertexBuffer(graphicsDevice, typeof (VertexPositionTexture),
                                      nBillboards*4, BufferUsage.WriteOnly);
            _verts.SetData(_particles);

            // Create and set the index buffer
            _ints = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits,
                                    nBillboards*6, BufferUsage.WriteOnly);
            _ints.SetData(_indices);
        }

        private void SetEffectParameters(Matrix view, Matrix projection, Vector3 up, Vector3 right)
        {
            effect.Parameters["ParticleTexture"].SetValue(texture);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["Size"].SetValue(billboardSize/2f);
            effect.Parameters["Up"].SetValue(Mode == BillboardMode.Spherical ? up : Vector3.Up);
            effect.Parameters["Side"].SetValue(right);
        }

        public void Draw(Matrix view, Matrix projection, Vector3 up, Vector3 right)
        {
            // Set the vertex and index buffer to the graphics card
            graphicsDevice.SetVertexBuffer(_verts);
            graphicsDevice.Indices = _ints;

            graphicsDevice.BlendState = BlendState.AlphaBlend;

            SetEffectParameters(view, projection, up, right);

            if (EnsureOcclusion)
            {
                DrawOpaquePixels();
                DrawTransparentPixels();
            }
            else
            {
                graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                effect.Parameters["AlphaTest"].SetValue(false);
                DrawBillboards();
            }

            // Reset render states
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Un-set the vertex and index buffer
            graphicsDevice.SetVertexBuffer(null);
            graphicsDevice.Indices = null;
        }

        private void DrawOpaquePixels()
        {
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            effect.Parameters["AlphaTest"].SetValue(true);
            effect.Parameters["AlphaTestGreater"].SetValue(true);

            DrawBillboards();
        }

        private void DrawTransparentPixels()
        {
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            effect.Parameters["AlphaTest"].SetValue(true);
            effect.Parameters["AlphaTestGreater"].SetValue(false);

            DrawBillboards();
        }

        private void DrawBillboards()
        {
            effect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                 4*nBillboards, 0, nBillboards*2);
        }
    }
}