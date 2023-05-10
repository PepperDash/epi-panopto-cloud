namespace Crestron.RAD.Common.BasicDriver
{
    public abstract partial class ABaseDriverProtocol
    {
        internal class CommandQueueObject
        {
            // Unique identifier of the object. This Id could be used when a command response is received and the command object needs to be removed from the queue.
            internal ulong Id;
            
            // Actual CommandSet object to use when sending to the device
            internal CommandSet CommandSet;

            // Timestamp when the command was sent to the device. This will be used to determine if the command timeout value has been reached.
            internal int TransmissionTickCount;

            // Command supports feedback from the device.
            internal bool SupportsFeedback;

            // Command is waiting for feedback response. This will be set by the Process Command Queue logic after the command is sent and "SupportsFeedback" is True.
            internal bool WaitingForFeedback;

            // Maximum number of retries before logging an error and/or removing it from the queue. 0 = No maximum.
            internal int MaxRetries;

            // Current number of command retries.
            internal int RetryCount;

            // Command is persistent and should not be removed from the queue. This is commonly used for "keep-alive" commands.
            internal bool Persistent;

            // "TimeBteweenCommands" override value.
            internal uint TBCOverride;
        }
    }
}