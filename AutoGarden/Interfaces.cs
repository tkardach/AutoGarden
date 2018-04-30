using System;

namespace AutoGarden.Interfaces 
{
    public interface IWaterControl
    {
        void WaterOn(int seconds);
        void WaterOn();
        void WaterOff();
    }

    public interface IClimate
    {
        void ReadTemperatureFareinheit();
        void ReadTemperatureCelcius();
        void ReadHumidity();
    }

    /// <summary>
    /// Creates a JSON object string representation of the object
    /// </summary>
    public interface JSONParsable
    {
        string CreateJSONRequest();
        bool ParseJSONResponse(string response);
        string ToJSON();
    }

}