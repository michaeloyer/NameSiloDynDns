using System;

namespace NameSiloDynDns
{
    public class HostToUpdate
    {
        public string Host { get; private set; }
        public string Domain { get; set; }
        public int Hours { get; private set; }
        public int Minutes { get; private set; }
        public int Seconds { get; private set; }
        public int RetryAttempts { get; private set; }
        public TimeSpan UpdateTimeSpan => new TimeSpan(Hours, Minutes, Seconds);
        public TimeSpan RetryTimeSpan =>
            RetryAttempts < 0
            ? TimeSpan.Zero
            : TimeSpan.FromTicks(UpdateTimeSpan.Ticks / (RetryAttempts + 1));
    }
}
