using System;
using System.Collections.Generic;
using AutoGarden.Interfaces;

namespace AutoGarden.Bluetooth
{
    #region BLE Classes

    public class BLEDevice : JSONParsable
    {
        string m_deviceName;
        string m_mac;

        Dictionary<string, BLEService> m_services;

        public BLEDevice(string deviceName)
        {
            m_deviceName = deviceName;
            m_services = new Dictionary<string, BLEService>();
            m_mac = "";
        }

        public BLEDevice(string deviceName, string mac)
        {
            m_deviceName = deviceName;
            m_services = new Dictionary<string, BLEService>();
            m_mac = mac;
        }

        public string DeviceName
        {
            get { return m_deviceName; }
            set { m_deviceName = value; }
        }

        public string MAC
        {
            get { return m_mac; }
            set { m_mac = value; }
        }

        /// <summary>
        /// Adds the service to the BLEDevice
        /// </summary>
        /// <param name="service">Service to be added</param>
        public bool AddService(BLEService service)
        {
            if (null == service) return false;
            if (m_services.ContainsKey(service.UUID))
                return false;

            m_services.Add(service.UUID, service);
            return true;
        }

        public string CreateJSONRequest()
        {
            string servicesString = "";
            int i = 0;
            foreach (var service in m_services.Values)
            {
                servicesString += service.CreateJSONRequest();
                if (i != m_services.Count - 1) servicesString += ", ";
            }

            string jsonString =
                string.Format("{{ \"MAC\" : \"{0}\"," +
                              "\"Services\" : [{1}] }}",
                              m_mac, servicesString);
            return jsonString;
        }

        public bool ParseJSONResponse(string response)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the JSON Object String
        /// </summary>
        /// <returns>The JSON Object string</returns>
        public string ToJSON()
        {
            string servicesString = "";
            int i = 0;
            foreach (var service in m_services.Values)
            {
                servicesString += service.ToJSON();
                if (i != m_services.Count - 1) servicesString += ", ";
            }

            string jsonString =
                string.Format("{{ \"DeviceName\" : \"{0}\"," +
                              "\"Services\" : [{1}] }}",
                                  m_deviceName, servicesString);
            return jsonString;
        }
    }

    public abstract class BLEService : JSONParsable, IEquatable<BLEService>
    {
        protected const string READ = "<r>";
        protected const string WRITE_OPEN = "<w>";
        protected const string WRITE_CLOSE = "</w>";

        protected string m_serviceName;
        protected string m_uuid;

        protected List<string> m_characteristicUUIDs;

        public BLEService(string serviceName, string uuid)
        {
            m_serviceName = serviceName;
            m_uuid = uuid;

            m_characteristicUUIDs = new List<string>();
        }

        public string ServiceName { get { return m_serviceName; } }
        public string UUID { get { return m_uuid; } }

        public abstract string CreateJSONRequest();

        public bool Equals(BLEService other)
        {
            if (m_uuid.Equals(other.UUID)) return true;
            return false;
        }


        public abstract bool ParseJSONResponse(string response);
        public abstract string ToJSON();

        protected string CreateWriteCommand(string write) 
        {
            return WRITE_OPEN + write + WRITE_CLOSE;
        }
    }

    public class ClimateService : BLEService
    {
        // Define Service UUID
        public static string CLIMATE_SERVICE_UUID =
            "4fafc201-1fb5-459e-8fcc-c5c9c331914b";

        // Define Characteristics' UUIDs
        public static string TEMPERATURE_F_UUID =
            "72d7c28d-485f-4872-b7ef-35d47e3b7f9e";
        public static string TEMPERATURE_C_UUID =
            "11c92152-f0d9-4951-af84-19bb98c5fbec";
        public static string HUMIDITY_UUID =
            "a3baecbd-5db0-465a-9ea3-50924ceb5675";

        private const string SERVICE_NAME = "Climate Service";
        private const string TEMP_F_NAME = "Temperature Farenheit";
        private const string TEMP_C_NAME = "Temperature Celcius";
        private const string HUMIDITY_NAME = "Percent Humidity";

        // local variables to store sensor results
        private double m_temperatureF = double.NaN;
        private double m_temperatureC = double.NaN;
        private double m_humidity = double.NaN;

        public ClimateService() : base(SERVICE_NAME, CLIMATE_SERVICE_UUID)
        {
            // Initialize BLE service
            m_serviceName = SERVICE_NAME;
            m_uuid = CLIMATE_SERVICE_UUID;

            // Initialize BLE characteristics
            m_characteristicUUIDs.Add(TEMPERATURE_F_UUID);
            m_characteristicUUIDs.Add(TEMPERATURE_C_UUID);
            m_characteristicUUIDs.Add(HUMIDITY_UUID);
        }

        public double TemperatureF
        {
            set { m_temperatureF = value; }
            get { return m_temperatureF; }
        }

        public double TemperatureC
        {
            set { m_temperatureC = value; }
            get { return m_temperatureC; }
        }

        public double Humidity
        {
            set { m_humidity = value; }
            get { return m_humidity; }
        }

        /// <summary>
        /// Creates a JSON object for network protocols
        /// </summary>
        /// <returns>The json string</returns>
        public override string CreateJSONRequest()
        {
            string characteristicString =
                string.Format("{{ \"UUID\" : \"{0}\" ," +
                                 "\"CMD\" : \"{1}\"}}," +
                              "{{ \"UUID\" : \"{2}\"," +
                                 "\"CMD\" : \"{3}\"}}" +
                              "{{ \"UUID\" : \"{4}\"," +
                                 "\"CMD\" : \"{5}\"}}",
                              TEMPERATURE_F_UUID, READ,
                              TEMPERATURE_C_UUID, READ,
                              HUMIDITY_UUID, READ);

            string jsonString =
                string.Format("{{ \"UUID\" : \"{0}\"," +
                              "\"Characteristics\" : [{1}] }}",
                                m_uuid, characteristicString);

            return jsonString;
        }

        public override bool ParseJSONResponse(string response)
        {
            throw new NotImplementedException();
        }

        public override string ToJSON()
        {
            string characteristicString =
                string.Format("{{ \"{0}\" : \"{1}\" }}," +
                              "{{ \"{2}\" : \"{3}\" }}," +
                              "{{ \"{4}\" : \"{5}\" }}",
                              TEMP_F_NAME, TEMPERATURE_F_UUID,
                              TEMP_C_NAME, TEMPERATURE_C_UUID,
                              HUMIDITY_NAME, HUMIDITY_UUID);

            string jsonString =
                string.Format("{{ \"UUID\" : \"{0}\"," +
                              "\"Characteristics\" : [{1}] }}",
                                m_uuid, characteristicString);

            return jsonString;
        }
    }

    public class WaterService : BLEService
    {
        // Define Service UUID
        public static string WATER_SERVICE_UUID =
            "e3952659-639c-4908-add4-962008d48067";

        // Define Characteristics' UUIDs
        public static string WATER_STATUS_UUID =
            "986fccef-17e9-4668-819c-0fc43b93b1c5";
        public static string WATER_TIMER_UUID =
            "a4d167fd-ebf3-4e11-976f-cad6be4d52c6";

        public enum WaterStatusValue : int { WATER_ON = 1, WATER_OFF = 0 };

        private const string SERVICE_NAME = "Watering Service";
        private const string WATER_STATUS_NAME = "Watering Status";
        private const string WATER_TIMER_NAME = "Watering Timer";


        // local variables to store sensor results
        private int m_waterStatus;
        private int m_waterTimer;

        public WaterService() : base(SERVICE_NAME, WATER_SERVICE_UUID)
        {
            // Initialize BLE service
            m_serviceName = SERVICE_NAME;
            m_uuid = WATER_SERVICE_UUID;

            // Initialize BLE characteristics
            m_characteristicUUIDs.Add(WATER_STATUS_UUID);
            m_characteristicUUIDs.Add(WATER_TIMER_UUID);

            // Intialize characteristic values
            m_waterStatus = 0;
            m_waterTimer = 10000;
        }

        public int WaterStatus
        {
            set { m_waterStatus = value; }
            get { return m_waterStatus; }
        }

        public int WaterTimer
        {
            set { m_waterTimer = value; }
            get { return m_waterTimer; }
        }

        /// <summary>
        /// Creates a JSON object for network protocols
        /// </summary>
        /// <returns>The json string</returns>
        public override string CreateJSONRequest()
        {
            string characteristicString =
                string.Format("{{ \"UUID\" : \"{0}\" ," +
                                 "\"CMD\" : \"{1}\"}}," +
                              "{{ \"UUID\" : \"{2}\"," +
                                 "\"CMD\" : \"{3}\"}}",
                              WATER_TIMER_UUID, CreateWriteCommand(m_waterTimer.ToString()),
                              WATER_STATUS_UUID, CreateWriteCommand(m_waterStatus.ToString()));

            string jsonString =
                string.Format("{{ \"UUID\" : \"{0}\"," +
                              "\"Characteristics\" : [{1}] }}",
                                m_uuid, characteristicString);

            return jsonString;
        }

        public override bool ParseJSONResponse(string response)
        {
            throw new NotImplementedException();
        }

        public override string ToJSON()
        {
            string characteristicString =
                string.Format("{{ \"{0}\" : \"{1}\" }}," +
                              "{{ \"{2}\" : \"{3}\" }}",
                              WATER_TIMER_NAME, WATER_TIMER_UUID,
                              WATER_STATUS_NAME, WATER_STATUS_UUID);

            string jsonString =
                string.Format("{{ \"UUID\" : \"{0}\"," +
                              "\"Characteristics\" : [{1}] }}",
                                m_uuid, characteristicString);

            return jsonString;
        }
    }

    #endregion
}