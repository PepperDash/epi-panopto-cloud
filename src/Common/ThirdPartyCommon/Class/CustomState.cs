using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common
{
    public class CustomState
    {
        string Label;
        StateSeverity Severity;
        StateType State;

        public CustomState()
        {
            Label = string.Empty;
            Severity = StateSeverity.Unknown;
            State = StateType.Unknown;
        }

        public CustomState(string label, StateSeverity severity, StateType state)
        {
            Label = label;
            Severity = severity;
            State = state;
        }
    }
}