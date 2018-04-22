using System;
using System.Threading;

namespace AutoGarden
{
    public class ScheduleEventArgs : EventArgs
    {
        private readonly string m_eventMsg;

        public ScheduleEventArgs(DateTime time)
        {
            m_eventMsg = string.Format("{0} : Scehdule Triggered", time);
        }

        public string EventMessage
        {
            get { return m_eventMsg; }
        }
    }

    /// <summary>
    /// Handles a Watering event.
    /// </summary>
    public delegate void ScheduleEventHandler(object sender, ScheduleEventArgs args);

    /// <summary>
    /// Watering schedule for plants. Specifies a schedule and a duration
    /// when the plant will be watered.
    /// </summary>
    public class WateringSchedule : Schedule 
    {
        public WateringSchedule()
        {
        }

        public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(5);
    }

    public class Schedule
    {
        private bool m_monday = false;
        private bool m_tuesday = false;
        private bool m_wednesday = false;
        private bool m_thursday = false;
        private bool m_friday = false;
        private bool m_saturday = false;
        private bool m_sunday = false;

        private bool m_daySet = false;

        private DateTime m_date;
        private TimeSpan m_time = TimeSpan.Zero;

        public Schedule()
        {
            m_date = DateTime.Now;
        }

        public string Name { get; set; } = "New Schedule";

        public TimeSpan Time
        {
            get { return m_time; }
            set
            {
                // If no day is set, it is a one-time event at the next matching time.
                
            }
        }

        private void SetTrueDays()
        {
            m_daySet = m_monday & m_tuesday & m_wednesday & 
                m_thursday & m_friday & m_saturday & m_sunday;
        }

        public bool Monday 
        { 
            get { return m_monday; }
            set 
            { 
                m_monday = value;
                SetTrueDays();
            }
        }

        public bool Tuesday
        {
            get { return m_tuesday; }
            set 
            { 
                m_tuesday = value;
                SetTrueDays();
            }
        }

        public bool Wednesday
        {
            get { return m_wednesday; }
            set
            {
                m_wednesday = value;
                SetTrueDays();
            }
        }

        public bool Thursday
        {
            get { return m_thursday; }
            set
            {
                m_thursday = value;
                SetTrueDays();
            }
        }

        public bool Friday
        {
            get { return m_friday; }
            set
            {
                m_friday = value;
                SetTrueDays();
            }
        }

        public bool Saturday
        {
            get { return m_saturday; }
            set
            {
                m_saturday = value;
                SetTrueDays();
            }
        }

        public bool Sunday
        {
            get { return m_sunday; }
            set
            {
                m_sunday = value;
                SetTrueDays();
            }
        }
    }
}
