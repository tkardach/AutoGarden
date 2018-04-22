using System;
using System.Collections.Generic;
using Interfaces;

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

    public abstract class Plant : IWaterable
    {
        protected List<WateringSchedule> m_wateringSchedule;
        protected string m_plantId;

        public event WateringEventHandler OnWater;

        public Plant() 
        {
            m_plantId = Guid.NewGuid().ToString("N");
            m_wateringSchedule = new List<WateringSchedule>();
        }

        public string Name { get; set; } = "New Plant";
        public DateTime DOB { get; set; } = DateTime.Now;
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