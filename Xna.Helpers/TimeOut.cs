#region

using System;
using System.Diagnostics;

#endregion

namespace Xna.Helpers
{

    #region Enums

    #endregion

    /// <summary>
    ///   Holds methods for checking timeout at desired time
    /// </summary>
    public class TimeOut
    {
        #region Fields

        private static Random _random;

        private bool _doOnce = true;
        private bool _rushTimer;

        private Stopwatch _stopWatch;
        private TimeSpan _timeSpan;
        private double _timeoutTime;

        #endregion

        #region Properties

        /// <summary>
        ///   Returns is the stopwatch running
        /// </summary>
        public bool IsStopWatchRunning
        {
            get { return _stopWatch.IsRunning; }
        }


        //temporaraly!!!
        /// <summary>
        /// Timespan of timeout
        /// </summary>
        public TimeSpan TimeSpan
        {
            get { return _timeSpan; }
            set { _timeSpan = value; }
        }
        /// <summary>
        /// Stopwatch of timeout
        /// </summary>
        public Stopwatch StopWatch
        {
            get { return _stopWatch; }
            set { _stopWatch = value; }
        }

        #endregion

        #region Constructors

        static TimeOut()
        {
            _random = new Random();
        }

        /// <summary>
        ///   Creates new TimeOut object , to use it - use Update method to return bool whenever timer triggers
        /// </summary>
        /// <param name = "timeoutTime">Timeout time in miliseconds</param>
        public TimeOut(double timeoutTime)
        {
            _stopWatch = new Stopwatch();
            _timeoutTime = timeoutTime;
            _timeSpan = TimeSpan.FromMilliseconds(timeoutTime);
            _rushTimer = false;
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Returns true if timespan is reached, it uses elapsedTime, prefered GameTime.TotalElapsed
        /// </summary>
        /// <param name = "elapsedTimeSpan">Prefered GameTime.TotalElapsed</param>
        /// <returns></returns>
        public bool Update(TimeSpan elapsedTimeSpan)
        {
            _timeSpan -= elapsedTimeSpan;
            if (_timeSpan <= TimeSpan.Zero)
            {
                _timeSpan = TimeSpan.FromMilliseconds(_timeoutTime);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if timespan is reached, it uses built-in stopwatch to measure timepass
        /// </summary>
        /// <param name="isSelfRestarting">If you want to handle the timer yourself set this to false</param>
        /// <returns></returns>
        public bool Update(bool isSelfRestarting = true)
        {
            bool result=false;
            if (isSelfRestarting)
            {
                if (!_stopWatch.IsRunning)
                {
                    _stopWatch.Start();
                }
            }
            if (_timeSpan <= _stopWatch.Elapsed || _rushTimer)
            {
                _stopWatch.Reset();
                _timeSpan = TimeSpan.FromMilliseconds(_timeoutTime);
                result= true; 
                _rushTimer = false;
            }

            return result;
        }
        /// <summary>
        /// Returns true if timespan is reached, it uses built-in stopwatch to measure timepass,randomizes the timespan
        /// </summary>
        /// <param name="min">Min random timeout time</param>
        /// <param name="max">Max random timeout time </param>
        /// <param name="isSelfRestarting">If you want to handle the timer yourself set this to false otherwise the timer will stop at next timeout</param>
        /// <returns></returns>
        public bool Update(float min,float max, bool isSelfRestarting = true)
        {
            bool result = false;
            if (isSelfRestarting)
            {
                if (!_stopWatch.IsRunning)
                {
                    _stopWatch.Start();
                }
            }
            if (_timeSpan <= _stopWatch.Elapsed || _rushTimer)
            {
                _stopWatch.Reset();
                RandomizeTimeout(min, max);
                _timeSpan = TimeSpan.FromMilliseconds(_timeoutTime);
                result = true;
                _rushTimer = false;
            }

            return result;
        }

        /// <summary>
        ///   Restarts the stopwatch or timespan
        /// </summary>
        public void RestartTimer()
        {
            if (_stopWatch != null)
            {
                _stopWatch.Reset();
            }
            else
            {
                _timeSpan = TimeSpan.FromMilliseconds(_timeoutTime);
            }
        }

        /// <summary>
        ///   Checks if the StopWatch is running and starts the stopwatch
        /// </summary>
        public void StartTimer()
        {
            if (IsStopWatchRunning) return;
            StopWatch.Start();
        }
        /// <summary>
        /// Rush the timer to timeout
        /// </summary>
        public void RushTimer()
        {
            _rushTimer = true;
        }

        /// <summary>
        /// Gets the random timeout time
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void RandomizeTimeout(float min, float max)
        {
            _timeoutTime = Generators.RandomNumber(min, max);
        }

        #endregion
    }
}