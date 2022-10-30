using System;

namespace Herbstregatta.Data.Run
{
    /// <summary>
    /// Descibes a time that was recorded
    /// </summary>
    public class TimeRecord
    {
        /// <summary>
        /// Internal Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the person that recorded the time
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The time that was recorded on the device
        /// </summary>
        public DateTimeOffset RecordedDeviceTime { get; set; }

        /// <summary>
        /// The time that was recorded when message was received
        /// </summary>
        public DateTimeOffset ReceivedServerTime { get; set; }
    }
}
