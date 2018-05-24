using System;
using System.Collections.Generic;
using AutoGarden.Interfaces;

namespace AutoGarden
{
    public class Gardener
    {
        public Gardener()
        {
        }
    }

    public class WateringEventArgs : EventArgs
    {
        private readonly string m_eventMsg;

        public WateringEventArgs(DateTime time, TimeSpan duration, string plantId)
        {
            m_eventMsg = string.Format("{0} : {1} watered for {2}",
                                       time, plantId, duration);
        }

        public string EventMessage
        {
            get { return m_eventMsg; }
        }
    }

    /// <summary>
    /// Handles a Watering event.
    /// </summary>
    public delegate void WateringEventHandler(object sender, WateringEventArgs args);

    /// <summary>
    /// A Waterable object informs other objects when it is due for watering.
    /// </summary>
    public interface IWaterable
    {
        event WateringEventHandler OnWater;
    }

    public class Plant : IWaterable
    {
        protected List<WateringSchedule> m_wateringSchedule;
        protected string m_plantId;
		protected string m_plantName;
		protected DateTime m_plantDOB;

        public event WateringEventHandler OnWater;

        public Plant() 
        {
			Initialize();
			m_plantName = "No Name";
			m_plantDOB = DateTime.Now;
        }

		public Plant(string name)
		{
			Initialize();
			m_plantName = name;
            m_plantDOB = DateTime.Now;
		}

        public Plant(string name, string dob)
		{         
            Initialize();
            m_plantName = name;
			DateTime.TryParse(dob, out m_plantDOB);
		}

        private void Initialize()
		{         
            m_plantId = Guid.NewGuid().ToString("N");
            m_wateringSchedule = new List<WateringSchedule>();
		}

        public string Name { get { return m_plantName; } set { m_plantName = value; } }

		public DateTime DOB { get { return m_plantDOB; } set { m_plantDOB = value; }}
    }

    public class PottedPlant : Plant, IWaterControl
    {
        public PottedPlant() : base()
        {
            
        }

        public void WaterOff()
        {
            throw new NotImplementedException();
        }

        public void WaterOn(int seconds)
        {
            throw new NotImplementedException();
        }

        public void WaterOn()
        {
            throw new NotImplementedException();
        }
    }

    public class HydroponicPlant : Plant, IWaterControl
    {
        string m_plantName;

        public HydroponicPlant() : base()
        {
        }

        public void WaterOff()
        {
            throw new NotImplementedException();
        }

        public void WaterOn(int seconds)
        {
            throw new NotImplementedException();
        }

        public void WaterOn()
        {
            throw new NotImplementedException();
        }
    }
}