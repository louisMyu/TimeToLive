using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

using Microsoft.Xna.Framework;
using Windows.Devices.Sensors;

namespace TimeToLive
{
    public class Input
    {
        public static bool UseAccelerometer = true;
        public static Accelerometer accelerometer;
        public static float AccelerometerAlpha = 0.35f;
        public static float AccelerometerThreshold = 0.1f;
        public static Vector3 CurrentAccelerometerValues { get; set; }
        public static float Tilt_Threshold = 0.0036f;
        public TouchCollection TouchState;
        private static Queue<Vector3> LastAcceleromeratorValues = new Queue<Vector3>();
        public readonly List<GestureSample> Gestures = new List<GestureSample>();
        static Input()
        {
            //Gestures = new List<GestureSample>();
            accelerometer = Windows.Devices.Sensors.Accelerometer.GetDefault();
            if (accelerometer != null)
            {
                accelerometer = Windows.Devices.Sensors.Accelerometer.GetDefault();
                accelerometer.ReadingChanged += ReadingChanged;
                accelerometer.ReportInterval = 10;
            }
            else
            {
                UseAccelerometer = false;
            }
        }

        private static void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            AccelerometerReading reading = e.Reading;
            Vector3 accelerationAverage = new Vector3();
            //newAcceration.X = (float)(reading.AccelerationX * AccelerometerAlpha + reading.AccelerationX * (1.0f - AccelerometerAlpha));// +0.20f;
            //newAcceration.Y = (float)(reading.AccelerationY * AccelerometerAlpha + reading.AccelerationY * (1.0f - AccelerometerAlpha));
            //newAcceration.Z = (float)(reading.AccelerationZ * AccelerometerAlpha + reading.AccelerationZ * (1.0f - AccelerometerAlpha));
            if (LastAcceleromeratorValues.Count == 5)
            {
                LastAcceleromeratorValues.Dequeue();
            }
            LastAcceleromeratorValues.Enqueue(new Vector3((float)reading.AccelerationX, (float)reading.AccelerationY, (float)reading.AccelerationZ));
            foreach (Vector3 val in LastAcceleromeratorValues.ToList())
            {
                accelerationAverage.X += val.X;
                accelerationAverage.Y += val.Y;
                accelerationAverage.Z += val.Z;
            }
            accelerationAverage.X /= 5;
            accelerationAverage.Y /= 5;
            accelerationAverage.Z /= 5;

            Vector3 newAcceleration = new Vector3();
            if (accelerationAverage.X > AccelerometerThreshold)
            {
                newAcceleration.X = (float)(CurrentAccelerometerValues.X + AccelerometerAlpha * (reading.AccelerationX - CurrentAccelerometerValues.X));
            }
            else
            {
                newAcceleration.X = accelerationAverage.X;
            }
            if (accelerationAverage.Y > AccelerometerThreshold)
            {
                newAcceleration.Y = (float)(CurrentAccelerometerValues.Y + AccelerometerAlpha * (reading.AccelerationY - CurrentAccelerometerValues.Y));
            }
            else
            {
                newAcceleration.Y = accelerationAverage.Y;
            }
            if (newAcceleration.Z > AccelerometerThreshold)
            {
                newAcceleration.Z = (float)(CurrentAccelerometerValues.Z + AccelerometerAlpha * (reading.AccelerationZ - CurrentAccelerometerValues.Z));
            }
            else
            {
                newAcceleration.Z = accelerationAverage.Z;
            }

            CurrentAccelerometerValues = newAcceleration;
        }

        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            TouchState = TouchPanel.GetState();

            Gestures.Clear();
            while (TouchPanel.IsGestureAvailable)
            {
                Gestures.Add(TouchPanel.ReadGesture());
            }
            
        }
        public bool IsNewKeyPress(Microsoft.Xna.Framework.Input.Buttons button)
        {
            return GamePad.GetState(0).IsButtonDown(button);
        }
        ///// <summary>
        ///// Helper for checking if a key was newly pressed during this update. The
        ///// controllingPlayer parameter specifies which player to read input for.
        ///// If this is null, it will accept input from any player. When a keypress
        ///// is detected, the output playerIndex reports which player pressed it.
        ///// </summary>
        //public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer,
        //                                    out PlayerIndex playerIndex)
        //{
        //    if (controllingPlayer.HasValue)
        //    {
        //        // Read input from the specified player.
        //        playerIndex = controllingPlayer.Value;

        //        int i = (int)playerIndex;

        //        //return (CurrentKeyboardStates[i].IsKeyDown(key) &&
        //        //        LastKeyboardStates[i].IsKeyUp(key));
        //        return 
        //    }
        //    else
        //    {
        //        // Accept input from any player.
        //        return (IsNewKeyPress(key, PlayerIndex.One, out playerIndex) ||
        //                IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) ||
        //                IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) ||
        //                IsNewKeyPress(key, PlayerIndex.Four, out playerIndex));
        //    }
        //}


        ///// <summary>
        ///// Helper for checking if a button was newly pressed during this update.
        ///// The controllingPlayer parameter specifies which player to read input for.
        ///// If this is null, it will accept input from any player. When a button press
        ///// is detected, the output playerIndex reports which player pressed it.
        ///// </summary>
        //public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer,
        //                                             out PlayerIndex playerIndex)
        //{
        //    if (controllingPlayer.HasValue)
        //    {
        //        // Read input from the specified player.
        //        playerIndex = controllingPlayer.Value;

        //        int i = (int)playerIndex;

        //        //return (CurrentGamePadStates[i].IsButtonDown(button) &&
        //        //        LastGamePadStates[i].IsButtonUp(button));
        //    }
        //    else
        //    {
        //        // Accept input from any player.
        //        return (IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
        //                IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
        //                IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
        //                IsNewButtonPress(button, PlayerIndex.Four, out playerIndex));
        //    }
        //}


        ///// <summary>
        ///// Checks for a "menu select" input action.
        ///// The controllingPlayer parameter specifies which player to read input for.
        ///// If this is null, it will accept input from any player. When the action
        ///// is detected, the output playerIndex reports which player pressed it.
        ///// </summary>
        //public bool IsMenuSelect(PlayerIndex? controllingPlayer,
        //                         out PlayerIndex playerIndex)
        //{
        //    return IsNewKeyPress(Keys.Space, controllingPlayer, out playerIndex) ||
        //           IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) ||
        //           IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex) ||
        //           IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        //}


        ///// <summary>
        ///// Checks for a "menu cancel" input action.
        ///// The controllingPlayer parameter specifies which player to read input for.
        ///// If this is null, it will accept input from any player. When the action
        ///// is detected, the output playerIndex reports which player pressed it.
        ///// </summary>
        //public bool IsMenuCancel(PlayerIndex? controllingPlayer,
        //                         out PlayerIndex playerIndex)
        //{
        //    return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
        //           IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex) ||
        //           IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
        //}


        ///// <summary>
        ///// Checks for a "menu up" input action.
        ///// The controllingPlayer parameter specifies which player to read
        ///// input for. If this is null, it will accept input from any player.
        ///// </summary>
        //public bool IsMenuUp(PlayerIndex? controllingPlayer)
        //{
        //    PlayerIndex playerIndex;

        //    return IsNewKeyPress(Keys.Up, controllingPlayer, out playerIndex) ||
        //           IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex) ||
        //           IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
        //}


        ///// <summary>
        ///// Checks for a "menu down" input action.
        ///// The controllingPlayer parameter specifies which player to read
        ///// input for. If this is null, it will accept input from any player.
        ///// </summary>
        //public bool IsMenuDown(PlayerIndex? controllingPlayer)
        //{
        //    PlayerIndex playerIndex;

        //    return IsNewKeyPress(Keys.Down, controllingPlayer, out playerIndex) ||
        //           IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
        //           IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
        //}


        ///// <summary>
        ///// Checks for a "pause the game" input action.
        ///// The controllingPlayer parameter specifies which player to read
        ///// input for. If this is null, it will accept input from any player.
        ///// </summary>
        //public bool IsPauseGame(PlayerIndex? controllingPlayer)
        //{
        //    PlayerIndex playerIndex;

        //    return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
        //           IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex) ||
        //           IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        //}
    }
}