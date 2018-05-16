using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Xna.Helpers._2D
{
    /// <summary>
    /// Class that contains various methods for handling input
    /// </summary>
    public static class Input
    {
        private static MouseState _mouseStateCurrent, _mouseStatePrevious;
        private static KeyboardState _keyBoardStateCurrent, _keyboardStatePrevious;

        /// <summary>
        /// Returns velocity based on input, increasing acceleration and deacceleration
        /// </summary>
        /// <param name="Velocity"></param>
        /// <param name="ACCELERATION"></param>
        /// <param name="DEACCELERATION"></param>
        /// <param name="MAX_SPEED"></param>
        public static void CheckInput(ref Vector2 Velocity, int ACCELERATION, int DEACCELERATION, int MAX_SPEED)
        {
            KeyboardState keyboardState = Keyboard.GetState();

           if (keyboardState.GetPressedKeys().Length != 0)
            {
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    Velocity = new Vector2(MathHelper.Clamp(Velocity.X - ACCELERATION, -MAX_SPEED, 0), Velocity.Y);
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    Velocity = new Vector2(MathHelper.Clamp(Velocity.X + ACCELERATION, 0, MAX_SPEED), Velocity.Y);
                }
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    Velocity = new Vector2(Velocity.X, MathHelper.Clamp(Velocity.Y - ACCELERATION, -MAX_SPEED, 0));
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    Velocity = new Vector2(Velocity.X, MathHelper.Clamp(Velocity.Y + ACCELERATION, 0, MAX_SPEED));
                }
            }
            else
            {
                if (Velocity.X > 0)
                {
                    Velocity = new Vector2(Velocity.X - MathHelper.Clamp(DEACCELERATION, 0, Velocity.X), Velocity.Y);
                }
                if (Velocity.Y > 0)
                {
                    Velocity = new Vector2(Velocity.X, Velocity.Y - MathHelper.Clamp(DEACCELERATION, 0, Velocity.Y));
                }
                if (Velocity.X < 0)
                {
                    Velocity = new Vector2(Velocity.X + MathHelper.Clamp(DEACCELERATION, -Velocity.X, 0), Velocity.Y);
                }
                if (Velocity.Y < 0)
                {
                    Velocity = new Vector2(Velocity.X, Velocity.Y + MathHelper.Clamp(DEACCELERATION, -Velocity.Y, 0));
                }
            }
        }
        /// <summary>
        /// Returns true if the mouse clicked on specified position within radius
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static bool IsMouseClickedOnTarget(Vector2 targetPosition,float radius)
        {
                bool result = false;

                _mouseStateCurrent = Mouse.GetState();
                if (CollisionDetection2D.IsNearTarget(new Vector2(_mouseStateCurrent.X, _mouseStateCurrent.Y),targetPosition,radius))
                {
                    if (_mouseStateCurrent.LeftButton == ButtonState.Pressed &&
                        _mouseStatePrevious.LeftButton == ButtonState.Released)
                    {
                        result = true;
                    }
                }
                _mouseStatePrevious = _mouseStateCurrent;
                return result;
        }
        /// <summary>
        /// Returns true if the mouse hovers on specified position within radius
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static bool IsMouseHoverOnTarget (Vector2 targetPosition,float radius)
        {
            
                MouseState mouseState = Mouse.GetState();
                if (CollisionDetection2D.IsNearTarget(new Vector2(mouseState.X, mouseState.Y), targetPosition,radius))
                {
                    return true;
                }
                return false;
            
        }
        /// <summary>
        /// It returns true if the specified Key has been pressed
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsKeyPressed(Keys key)
        {
            _keyBoardStateCurrent = Keyboard.GetState();
            bool result;

            if (_keyBoardStateCurrent.IsKeyDown(key)&&_keyboardStatePrevious.IsKeyUp(key))
            {
               result= true;
            }
            else
            {
                 result= false;
            }


            _keyboardStatePrevious = _keyBoardStateCurrent;

            return result;
        }

        }
    }

