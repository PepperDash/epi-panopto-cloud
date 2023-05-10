using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Enums;


namespace Crestron.Panopto.Common
{
    /// <summary>
    /// A collection of alarms.
    /// </summary>
    public class AlarmState
    {
        /// <summary>
        /// Indicates the state of the burglary alarm.
        /// </summary>
        public StateType Burglary;
        /// <summary>
        /// Indicates the state of the fire alarm.
        /// </summary>
        public StateType Fire;
        /// <summary>
        /// Indicates the state of the medical alarm.
        /// </summary>
        public StateType Medical;
        /// <summary>
        /// Indicates the state of the tamper alarm.
        /// </summary>
        public StateType Tamper;

        /// <summary>
        /// When an AlarmState object is initialized the states default to the unknown state.
        /// </summary>
        public AlarmState()
        {
            Burglary = StateType.Unknown;
            Medical = StateType.Unknown;
            Fire = StateType.Unknown;
            Tamper = StateType.Unknown;
        }
    }
}