using System;

namespace Interfaces 
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

}