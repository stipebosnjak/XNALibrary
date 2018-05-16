﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xna.Helpers._3D
{

    public class Terrain : IRenderable
    {
        private VertexPositionNormalTexture[] vertices; // Vertex array
        private VertexBuffer vertexBuffer; // Vertex buffer
        private int[] indices; // Index array
        private IndexBuffer indexBuffer; // Index buffer
        private float[,] heights; // Array of vertex heights
        private float height; // Maximum height of terrain
        private float cellSize; // Distance between vertices on x and z axes
        private int width, length; // Number of vertices on x and z axes
        private int nVertices, nIndices; // Number of vertices and indices
        private Effect effect; // Effect used for rendering
        private GraphicsDevice GraphicsDevice; // Graphics device to draw with
        private Texture2D heightMap; // Heightmap texture

        private Texture2D baseTexture;
        private float textureTiling;
        private Vector3 lightDirection;

        public Texture2D RTexture, BTexture, GTexture, WeightMap;
        public Texture2D DetailTexture;
        public float DetailDistance = 2500;
        public float DetailTextureTiling = 100;

        public Terrain(Texture2D HeightMap, float CellSize, float Height,
                       Texture2D BaseTexture, float TextureTiling, Vector3 LightDirection,
                       GraphicsDevice GraphicsDevice)
        {
            this.heightMap = HeightMap;
            this.width = HeightMap.Width;
            this.length = HeightMap.Height;
            this.cellSize = CellSize;
            this.height = Height;
            this.baseTexture = BaseTexture;
            this.textureTiling = TextureTiling;
            this.lightDirection = LightDirection;

            this.GraphicsDevice = GraphicsDevice;

            effect = EffectManager.Content.Load<Effect>("TerrainEffect");

            // 1 vertex per pixel
            nVertices = width*length;

            // (Width-1) * (Length-1) cells, 2 triangles per cell, 3 indices per triangle
            nIndices = (width - 1)*(length - 1)*6;

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof (VertexPositionNormalTexture),
                                            nVertices, BufferUsage.WriteOnly);

            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits,
                                          nIndices, BufferUsage.WriteOnly);

            getHeights();
            createVertices();
            createIndices();
            genNormals();

            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);
            indexBuffer.SetData<int>(indices);
        }

        // Returns the height and steepness of the terrain at point (X, Z)
        public float GetHeightAtPosition(float X, float Z,
                                         out float Steepness)
        {
            // Clamp coordinates to locations on terrain
            X = MathHelper.Clamp(X, (-width/2)*cellSize,
                                 (width/2)*cellSize);
            Z = MathHelper.Clamp(Z, (-length/2)*cellSize,
                                 (length/2)*cellSize);

            // Map from (-Width/2->Width/2,-Length/2->Length/2) 
            // to (0->Width, 0->Length)
            X += (width/2f)*cellSize;
            Z += (length/2f)*cellSize;

            // Map to cell coordinates
            X /= cellSize;
            Z /= cellSize;

            // Truncate coordinates to get coordinates of top left cell vertex
            int x1 = (int) X;
            int z1 = (int) Z;

            // Try to get coordinates of bottom right cell vertex
            int x2 = x1 + 1 == width ? x1 : x1 + 1;
            int z2 = z1 + 1 == length ? z1 : z1 + 1;

            // Get the heights at the two corners of the cell
            float h1 = heights[x1, z1];
            float h2 = heights[x2, z2];

            // Determine steepness (angle between higher and lower vertex of cell)
            Steepness = (float) Math.Atan(Math.Abs((h1 - h2))/
                                          (cellSize*Math.Sqrt(2)));

            // Find the average of the amounts lost from coordinates during 
            // truncation above
            float leftOver = ((X - x1) + (Z - z1))/2f;

            // Interpolate between the corner vertices' heights
            return MathHelper.Lerp(h1, h2, leftOver);
        }

        private void getHeights()
        {
            // Extract pixel data
            Color[] heightMapData = new Color[width*length];
            heightMap.GetData<Color>(heightMapData);

            // Create heights[,] array
            heights = new float[width,length];

            // For each pixel
            for (int y = 0; y < length; y++)
                for (int x = 0; x < width; x++)
                {
                    // Get color value (0 - 255)
                    float amt = heightMapData[y*width + x].R;

                    // Scale to (0 - 1)
                    amt /= 255.0f;

                    // Multiply by max height to get final height
                    heights[x, y] = amt*height;
                }
        }

        private void createVertices()
        {
            vertices = new VertexPositionNormalTexture[nVertices];

            // Calculate the position offset that will center the terrain at (0, 0, 0)
            Vector3 offsetToCenter = -new Vector3(((float) width/2.0f)*cellSize,
                                                  0, ((float) length/2.0f)*cellSize);

            // For each pixel in the image
            for (int z = 0; z < length; z++)
                for (int x = 0; x < width; x++)
                {
                    // Find position based on grid coordinates and height in heightmap
                    Vector3 position = new Vector3(x*cellSize,
                                                   heights[x, z], z*cellSize) + offsetToCenter;

                    // UV coordinates range from (0, 0) at grid location (0, 0) to 
                    // (1, 1) at grid location (width, length)
                    Vector2 uv = new Vector2((float) x/width, (float) z/length);

                    // Create the vertex
                    vertices[z*width + x] = new VertexPositionNormalTexture(
                        position, Vector3.Zero, uv);
                }
        }

        private void createIndices()
        {
            indices = new int[nIndices];

            int i = 0;

            // For each cell
            for (int x = 0; x < width - 1; x++)
                for (int z = 0; z < length - 1; z++)
                {
                    // Find the indices of the corners
                    int upperLeft = z*width + x;
                    int upperRight = upperLeft + 1;
                    int lowerLeft = upperLeft + width;
                    int lowerRight = lowerLeft + 1;

                    // Specify upper triangle
                    indices[i++] = upperLeft;
                    indices[i++] = upperRight;
                    indices[i++] = lowerLeft;

                    // Specify lower triangle
                    indices[i++] = lowerLeft;
                    indices[i++] = upperRight;
                    indices[i++] = lowerRight;
                }
        }

        private void genNormals()
        {
            // For each triangle
            for (int i = 0; i < nIndices; i += 3)
            {
                // Find the position of each corner of the triangle
                Vector3 v1 = vertices[indices[i]].Position;
                Vector3 v2 = vertices[indices[i + 1]].Position;
                Vector3 v3 = vertices[indices[i + 2]].Position;

                // Cross the vectors between the corners to get the normal
                Vector3 normal = Vector3.Cross(v1 - v2, v1 - v3);
                normal.Normalize();

                // Add the influence of the normal to each vertex in the
                // triangle
                vertices[indices[i]].Normal += normal;
                vertices[indices[i + 1]].Normal += normal;
                vertices[indices[i + 2]].Normal += normal;
            }

            // Average the influences of the triangles touching each
            // vertex
            for (int i = 0; i < nVertices; i++)
                vertices[i].Normal.Normalize();
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;

            effect.Parameters["View"].SetValue(View);
            effect.Parameters["Projection"].SetValue(Projection);
            effect.Parameters["BaseTexture"].SetValue(baseTexture);
            effect.Parameters["TextureTiling"].SetValue(textureTiling);
            effect.Parameters["LightDirection"].SetValue(lightDirection);

            effect.Parameters["RTexture"].SetValue(RTexture);
            effect.Parameters["GTexture"].SetValue(GTexture);
            effect.Parameters["BTexture"].SetValue(BTexture);
            effect.Parameters["WeightMap"].SetValue(WeightMap);

            effect.Parameters["DetailTexture"].SetValue(DetailTexture);
            effect.Parameters["DetailDistance"].SetValue(DetailDistance);
            effect.Parameters["DetailTextureTiling"].SetValue(DetailTextureTiling);

            effect.Techniques[0].Passes[0].Apply();

            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                 nVertices, 0, nIndices/3);
        }

        public void SetClipPlane(Vector4? Plane)
        {
            effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

            if (Plane.HasValue)
                effect.Parameters["ClipPlane"].SetValue(Plane.Value);
        }

        public Vector3[] GetRandomPositions(int count,bool isStepnessChecked)
        {
            Vector3[] vector3s=new Vector3[count];
            for (int i = 0; i < 100; i++) // 500
            {
                // Get X and Z coordinates from the random generator, between
                //terrain width) / 2 * (cell size), (terrain width) / 2 * (cell
                //size)]
                float x = Xna.Helpers.Generators.RandomNumber(-256 * 30f, 256 * 30f);
                float z = Xna.Helpers.Generators.RandomNumber(-256 * 30f, 256 * 30f);
                // Get the height and steepness of this position on the terrain,
                // taking the height of the billboard into account
                float steepness;
                float y = GetHeightAtPosition(x, z, out steepness) + 100;
                // Reject this position if it is too low, high, or steep. Otherwise
                // add it to the list
                if (isStepnessChecked)
                {
                     if (steepness < MathHelper.ToRadians(15) && y > 2300 && y < 3200)
                         vector3s[i] = new Vector3(x, y, z);
                     else
                     i--;
                }
                else
                {
                       vector3s[i] = new Vector3(x, y, z);
                }
              
            }
            return vector3s;
        }
    }

}