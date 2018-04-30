using System;
using System.Collections.Generic;
using AutoGarden.Bluetooth;

namespace AutoGarden.Database
{
    #region Database Connection

    public static class DatabaseConnection
    {
        public static List<BLEService> GetBLEServices()
        {
            var list = new List<BLEService>();

            list.Add(new ClimateService());
            list.Add(new WaterService());

            return list;
        }
    }

    #endregion
}
