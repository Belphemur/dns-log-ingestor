using System;

namespace LogIngester.Configuration
{
    public class WorkerConfig
    {
        /// <summary>
        /// Check for work every <see cref="Interval"/>
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// How many worker to run concurrently
        /// </summary>
        public int Workers { get; set; } = 3;
    }
}