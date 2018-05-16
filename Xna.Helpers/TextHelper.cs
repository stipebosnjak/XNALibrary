using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Utility;

namespace Xna.Helpers
{
    public enum TextAlignment
    {
        TopLeft = 0,
        MiddleLeft = 1,
        BottomLeft = 2,
        TopCenter = 3,
        MiddleCenter = 4,
        BottomCenter = 5,
        TopRight = 6,
        MiddleRight = 7,
        BottomRight = 8,
    }
  public class TextHelper
    {
        //Font to use
        private SpriteFont font;
        //Change the size of our font on-screen
        private float fontScale;
        //Size of our font. We also update this when fontText changes.
        private Vector2 fontSize;
        //Color of the font
        public Color fontColor;
        //Position of the text on screen
        private Vector2 fontPosition;
        //Text to draw to the screen
        private string fontText;
        public string FontText
        {
            get { return fontText; }
            set
            {
                fontText = value;
                //Whenever we update the text we also need to update
                //fontSize and fontOrigin and UpdateText() does just that
                UpdateText();
            }
        }
        //Used for aligning our text in different ways
        private Vector2 fontOrigin;
        TextAlignment fontAlignment = TextAlignment.TopLeft;
        //Timers and interpolators. Used for fading text.
        private TimerCollection timers = new TimerCollection();
        private InterpolatorCollection interpolators = new InterpolatorCollection();
        //Used to tell if we're fading in/out
        bool isFadingIn;
        /// <summary>
        /// Constructor WITHOUT alignment assigned default to TextAlignment.TopLeft
        /// </summary>
        /// <param name="spriteFont">font to use</param>
        /// <param name="text">text to display</param>
        /// <param name="position">position of text on-screen</param>
        /// <param name="scale">scale of the text</param>
        /// <param name="color">color of the text</param>
        public TextHelper(SpriteFont spriteFont, string text,
                          Vector2 position, float scale, Color color)
            : this(spriteFont, text, position, TextAlignment.TopLeft, scale, color)
        {
        }
        /// <summary>
        /// Constructor used to specify textAlignment
        /// </summary>
        /// <param name="spriteFont">font to use</param>
        /// <param name="text">text to display</param>
        /// <param name="position">position of text on-screen</param>
        /// <param name="alignment">alignment of the text</param>
        /// <param name="scale">scale of the text</param>
        /// <param name="color">color of the text</param>
        public TextHelper(SpriteFont spriteFont, string text, Vector2 position,
                          TextAlignment alignment, float scale, Color color)
        {
            font = spriteFont;
            fontText = text;
            fontPosition = position;
            fontAlignment = alignment;
            fontScale = scale;
            fontColor = color;
            fontOrigin = Vector2.Zero;
            fontSize = font.MeasureString(text);
            //Horizontal alignment
            if (fontAlignment == TextAlignment.TopCenter ||
                fontAlignment == TextAlignment.MiddleCenter ||
                fontAlignment == TextAlignment.BottomCenter)
            {
                fontOrigin.X = fontSize.X / 2;
            }
            else if (fontAlignment == TextAlignment.TopRight ||
                     fontAlignment == TextAlignment.MiddleRight ||
                     fontAlignment == TextAlignment.BottomRight)
            {
                fontOrigin.X = fontSize.X;
            }
            //Vertical alignment
            if (fontAlignment == TextAlignment.MiddleLeft ||
                fontAlignment == TextAlignment.MiddleCenter ||
                fontAlignment == TextAlignment.MiddleRight)
            {
                fontOrigin.Y = fontSize.Y / 2;
            }
            else if (fontAlignment == TextAlignment.BottomLeft ||
                     fontAlignment == TextAlignment.BottomCenter ||
                     fontAlignment == TextAlignment.BottomRight)
            {
                fontOrigin.Y = fontSize.Y;
            }
        }
        public void Update(GameTime gameTime)
        {
            timers.Update(gameTime);
            interpolators.Update(gameTime);
        }
        private void UpdateText()
        {
            fontSize = font.MeasureString(fontText);
            //Horizontal alignment
            if (fontAlignment == TextAlignment.TopCenter ||
                fontAlignment == TextAlignment.MiddleCenter ||
                fontAlignment == TextAlignment.BottomCenter)
            {
                fontOrigin.X = fontSize.X / 2;
            }
            else if (fontAlignment == TextAlignment.TopRight ||
                     fontAlignment == TextAlignment.MiddleRight ||
                     fontAlignment == TextAlignment.BottomRight)
            {
                fontOrigin.X = fontSize.X;
            }
            //Vertical alignment
            if (fontAlignment == TextAlignment.MiddleLeft ||
                fontAlignment == TextAlignment.MiddleCenter ||
                fontAlignment == TextAlignment.MiddleRight)
            {
                fontOrigin.Y = fontSize.Y / 2;
            }
            else if (fontAlignment == TextAlignment.BottomLeft ||
                     fontAlignment == TextAlignment.BottomCenter ||
                     fontAlignment == TextAlignment.BottomRight)
            {
                fontOrigin.Y = fontSize.Y;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, fontText, fontPosition, fontColor,
                0f, fontOrigin, fontScale, SpriteEffects.None, 0);
        }
        public static string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = spriteFont.MeasureString(" ").X;
            foreach (string word in words)
            {
                Vector2 size = spriteFont.MeasureString(word);
                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }
            return sb.ToString();
        }
        public void FadeIn(float howLongToFadeIn)
        {
            //Changing howLongToFadeIn will increase/decrease the fade in duration
            interpolators.Create(0, 1, howLongToFadeIn, FadeInInterpolator, null);
            isFadingIn = true;
        }
        private void FadeInInterpolator(Interpolator i)
        {
            //Color.Lerp does a linear interpolation
            //between the colors specified.
            // Argument #1 = transparent (see-through)
            // Argument #2 = opaque (fully visible)
            fontColor = Color.Lerp(new Color(fontColor.R, fontColor.G, fontColor.B, 0), new Color(fontColor.R, fontColor.G, fontColor.B, 255),
               i.Value);
        }
        public void FadeOut(float howLongToFadeOut)
        {
            //Changing howLongToFadeOut will increase/decrease the fade in duration
            interpolators.Create(0, 1, howLongToFadeOut, FadeOutInterpolator, null);
            isFadingIn = false;
        }
        private void FadeOutInterpolator(Interpolator i)
        {
            //Color.Lerp does a linear interpolation
            //between the colors specified.
            // Argument #1 = opaque (fully visible)
            // Argument #2 = transparent (see-through)
            fontColor = Color.Lerp(new Color(fontColor.R, fontColor.G, fontColor.B, 255), new Color(fontColor.R, fontColor.G, fontColor.B, 0),
               i.Value);
        }
        public void Pulsate(float howLongBetweenEachBurst)
        {
            //We begin at full alpha so the only
            //way to go is to start fading out
            isFadingIn = false;
            //Since text starts at fully visible then we'll fade out right away.
            interpolators.Create(0, 1, howLongBetweenEachBurst, FadeOutInterpolator, null);
            //create a timer that will active once the first fade out occurs
            timers.Create(howLongBetweenEachBurst, true, timer =>
            {
                if (isFadingIn)
                    interpolators.Create(0, 1, howLongBetweenEachBurst, FadeOutInterpolator,
                        interpolator => { isFadingIn = false; });
                else
                    interpolators.Create(0, 1, howLongBetweenEachBurst, FadeInInterpolator,
                        interpolator => { isFadingIn = true; });
            });
        }
    }
}