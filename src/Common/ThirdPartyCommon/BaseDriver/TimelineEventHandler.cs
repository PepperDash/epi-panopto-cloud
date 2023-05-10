using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto.Common.BasicDriver
{
    /// <summary>
    /// Helper to avoid using CTimers
    /// This should not be referenced by any drivers
    /// Timing resolution is 100ms
    /// </summary>
    internal class TimedEventHelper : IDisposable
    {
        /// <summary>
        /// The event handler that will be used to notify listenerws the timed event has happened
        /// </summary>
        public Action<object> ExecuteEventHandler
        {
            get { return _executeEventHandler; }
            set
            {
                try
                {
                    _executeEventHandlerLock.Enter();
                    _executeEventHandler = value;
                }
                finally
                {
                    _executeEventHandlerLock.Leave();
                }
            }
        }

        private Action<object> _executeEventHandler;
        private readonly CCriticalSection _executeEventHandlerLock;
        private int _setDueTime;
        private int _currentDueTime;
        private bool _repeat;
        private object _callbackObject;

        private static CTimer _timedEvent100msClock = new CTimer(TimedEvent100msEventCallback, null, 0, 100);
        private static Action _timedEvent100msClockEventHandler;

        public TimedEventHelper()
        {
            _executeEventHandlerLock = new CCriticalSection();
        }

        public void Start(int dueTime)
        {
            Start(dueTime, false, null);
        }

        public void Start(int dueTime, bool repeat)
        {
            Start(dueTime, repeat, null);
        }

        public void State(int dueTime, object callbackObject)
        {
            Start(dueTime, false, callbackObject);
        }

        public void Start(int dueTime, bool repeat, object callbackObject)
        {
            _setDueTime = dueTime;
            _currentDueTime = dueTime;
            _repeat = repeat;
            _callbackObject = callbackObject;

            _timedEvent100msClockEventHandler -= HandleDriver100msClockEvent;
            _timedEvent100msClockEventHandler += HandleDriver100msClockEvent;
        }

        public void Stop()
        {
            _timedEvent100msClockEventHandler -= HandleDriver100msClockEvent;
        }

        public void Dispose()
        {
            ExecuteEventHandler = null;
        }

        private void HandleDriver100msClockEvent()
        {
            if (_currentDueTime <= 0)
            {
                FireEvent();
                if (_repeat)
                {
                    _currentDueTime = _setDueTime;
                }
                else
                {
                    Stop();
                }
            }
            else
            {
                _currentDueTime -= 100;

            }
        }

        private void FireEvent()
        {
            if (ExecuteEventHandler != null)
            {
                ExecuteEventHandler(_callbackObject);
            }
        }

        private static void TimedEvent100msEventCallback(object notUsed)
        {
            if (_timedEvent100msClockEventHandler != null)
            {
                _timedEvent100msClockEventHandler();
            }
        }
    }

    public class TimelineEventHandler
    {
        private const int _seconds = 1000;
        private const int _driverClockInterval = 25;

        private bool _startDelay;
        private bool _delayExecuted;
        private int _lastTick;
        private int _tick;

        public int DueAt { get; private set; }
        public int EventInterval { get; set; }
        public bool Repeat { get; set; }

        public Action EventExecute;

        public TimelineEventHandler()
        {
            EventInterval = 0;
            DueAt = 0;
            Repeat = false;
            Initialize();
        }

        public TimelineEventHandler(int eventInterval, int dueAt)
        {
            EventInterval = eventInterval;
            DueAt = CalculateDueAt(dueAt);
            Repeat = false;
            Initialize();
        }

        public TimelineEventHandler(int eventInterval, int dueAt, bool repeat)
        {
            EventInterval = eventInterval;
            DueAt = CalculateDueAt(dueAt);
            Repeat = repeat;
            Initialize();
        }

        private int CalculateDueAt(int dueAt)
        {
            if (dueAt > 0)
            {
                dueAt = dueAt * _seconds;
            }
            return dueAt;
        }

        private void Initialize()
        {
            _startDelay = false;
            _delayExecuted = true;
            _lastTick = 0;
            _tick = 0;
            DriverClock.Driver25msClockEventHandler -= HandleDevice25msTick;
            DriverClock.Driver25msClockEventHandler += HandleDevice25msTick;
        }

        private void ExecuteEvent()
        {
            if (EventExecute != null)
            {
                EventExecute();
            }
            if (!Repeat)
            {
                DriverClock.Driver25msClockEventHandler -= HandleDevice25msTick;
            }
            else
            {
                _startDelay = true;
            }
        }

        private void HandleDevice25msTick()
        {
            HandleTimelineEvent(CrestronEnvironment.TickCount);
        }

        public void Start(int dueAt)
        {
            _startDelay = true;
            Repeat = false;
            DueAt = dueAt;
            DriverClock.Driver25msClockEventHandler -= HandleDevice25msTick;
            DriverClock.Driver25msClockEventHandler += HandleDevice25msTick;
        }

        public void Start(int dueAt, bool repeat)
        {
            Repeat = repeat;
            _startDelay = true;
            DueAt = dueAt;
            DriverClock.Driver25msClockEventHandler -= HandleDevice25msTick;
            DriverClock.Driver25msClockEventHandler += HandleDevice25msTick;
        }

        public void Stop()
        {
            DriverClock.Driver25msClockEventHandler -= HandleDevice25msTick;
        }

        private void CalculateNextEventExecution(int currentTick)
        {
            _lastTick = currentTick;
            int DelayBetweenSequences = EventInterval;
            _tick = currentTick + DelayBetweenSequences;
            _startDelay = false;
            _delayExecuted = false;
        }

        private void CheckOnEvent(int currentTick)
        {
            if (_delayExecuted == false)
            {
                if (currentTick >= _tick)
                {
                    ExecuteEvent();
                    _delayExecuted = true;
                }
                else if (_tick + _driverClockInterval >= Int32.MaxValue && currentTick <= _driverClockInterval)
                {
                    ExecuteEvent();
                    _delayExecuted = true;
                }
            }
        }

        public void HandleTimelineEvent(int currentTick)
        {
            if (DueAt > 0)
            {
                DueAt -= _driverClockInterval;
                if (DueAt <= 0)
                {
                    DueAt = 0;
                }
            }
            else
            {
                if (_startDelay)
                {
                    CalculateNextEventExecution(currentTick);
                }
                CheckOnEvent(currentTick);
            }
        }
    }
}