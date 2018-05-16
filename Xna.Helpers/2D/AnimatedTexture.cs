using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Xna.Helpers._2D
{
    /// <summary>
    /// Class 
    /// </summary>
    public class AnimatedTexture
    {
        private int _framecount;
        private Texture2D _myTexture;
        private float _timePerFrame;
        private int _frame;
        private float _totalElapsed;
        private bool _paused;
        private float _rotation, _scale, _depth;
       
        /// <summary>
        /// Origion of the texture
        /// </summary>
        public Vector2 Origin;

        /// <summary>
        /// Creates ne Animated texture
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <param name="depth"></param>
        public AnimatedTexture(Vector2 origin, float rotation,
                               float scale, float depth)
        {
            this.Origin = origin;
            this.Rotation = rotation;
            this.Scale = scale;
            this.Depth = depth;
        }
        /// <summary>
        /// Loads the spritesheet in texture
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <param name="asset">Path to content</param>
        /// <param name="frameCount">The number of frames</param>
        /// <param name="framesPerSec">The speed od switching frames</param>
        public void Load(ContentManager content, string asset,
                         int frameCount, int framesPerSec)
        {
            _framecount = frameCount;
            _myTexture = content.Load<Texture2D>(asset);
            _timePerFrame = (float) 1/framesPerSec;
            _frame = 0;
            _totalElapsed = 0;
            _paused = false;
        }

        /// <summary>
        /// Updates the Animated texture
        /// </summary>
        /// <param name="elapsed"></param>
        public void UpdateFrame(float elapsed)
        {
            if (_paused)
                return;
            _totalElapsed += elapsed;
            if (_totalElapsed > _timePerFrame)
            {
                _frame++;
                // Keep the Frame between 0 and the total frames, minus one.
                _frame = _frame%_framecount;
                _totalElapsed -= _timePerFrame;
            }
        }

        /// <summary>
        /// Draws the Animated Texture
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="screenPos"></param>
        public void DrawFrame(SpriteBatch batch, Vector2 screenPos)
        {
            DrawFrame(batch, _frame, screenPos);
        }
        /// <summary>
        /// Draws the Animated Texture
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="frame"></param>
        /// <param name="screenPos"></param>
        public void DrawFrame(SpriteBatch batch, int frame, Vector2 screenPos)
        {
            int frameWidth = _myTexture.Width/_framecount;
            Rectangle sourcerect = new Rectangle(frameWidth*frame, 0,
                                                 frameWidth, _myTexture.Height);
            batch.Draw(_myTexture, screenPos, sourcerect, Color.White,
                       Rotation, Origin, Scale, SpriteEffects.None, Depth);
        }

        /// <summary>
        /// Rotation of the texture
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }
        /// <summary>
        /// Scale of the texture
        /// </summary>
        public float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }
        /// <summary>
        /// Depth of the texture
        /// </summary>
        public float Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        /// <summary>
        /// Is Updating 
        /// </summary>
        public bool IsPaused
        {
            get { return _paused; }
        }
        /// <summary>
        /// Resets the Animated texture frame
        /// </summary>
        public void Reset()
        {
            _frame = 0;
            _totalElapsed = 0f;
        }
        /// <summary>
        /// Stops Animated texture
        /// </summary>
        public void Stop()
        {
            Pause();
            Reset();
        }
        /// <summary>
        /// Plays the Animated texture
        /// </summary>
        public void Play()
        {
            _paused = false;
        }
        /// <summary>
        /// Pauses the Animated Texture
        /// </summary>
        public void Pause()
        {
            _paused = true;
        }
    }
}