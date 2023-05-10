using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto.Common.Transports
{
    internal class DelayedIrCommand
    {
        public IrFunction Function;
        public uint DelayForFutureCommands;

        public DelayedIrCommand(IrFunction function, uint delayForFutureCommands)
        {
            Function = function;
            DelayForFutureCommands = delayForFutureCommands;
        }
    }
}