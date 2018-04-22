using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AutoGarden.HardwareCommunication
{

    public class RPiCommLink
    {
        private const string URI =
            "http://10.0.0.139/cgi-bin/hwctrl.py";
        private const string ID_CMD = "ID=Tombo";

        private TcpClient m_clientSocket;

        private static RPiCommLink m_rpiCommLink;

        private RPiCommLink() 
        {
            m_clientSocket = new TcpClient();
            ServicePointManager.ServerCertificateValidationCallback =
                                   TrustAllCertificatesCallback;
        }

        public static bool TrustAllCertificatesCallback(
            object sender, X509Certificate cert,
            X509Chain chain, System.Net.Security.SslPolicyErrors errors)
        {
            return true;
        }

        public static RPiCommLink GetInstance()
        {
            if (m_rpiCommLink == null)
                m_rpiCommLink = new RPiCommLink();
            return m_rpiCommLink;
        }

        public string SendCommand(string cmd)
        {
            try
            {
                string myParameters = string.Format("{0}&{1}", cmd, ID_CMD);

                string HtmlResult = "Something went wrong...";
                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    HtmlResult = wc.UploadString(URI, myParameters);
                }

                return HtmlResult;
            }
            catch (SocketException ex)
            {
                return "SocketException : " + ex.Message;
            }
            catch (Exception ex)
            {
                return "Exception : " + ex.Message;
            }
            finally
            {
                m_clientSocket.Close();
            }
        }


        public async Task<string> SendCommandAsync(string cmd)
        {
            try
            {
                string myParameters = string.Format("{0}&{1}", cmd, ID_CMD);

                string HtmlResult = "Something went wrong...";
                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    HtmlResult = await wc.UploadStringTaskAsync(URI, myParameters);
                }

                return HtmlResult;
            }
            catch (SocketException ex)
            {
                return "SocketException : " + ex.Message;
            }
            catch (Exception ex)
            {
                return "Exception : " + ex.Message;
            }
            finally
            {
                m_clientSocket.Close();
            }
        }
    }

    public interface IBLEService
    {
        string ServiceName { get; }
        string ServiceUUID { get; }
    }

    public abstract class BLEDevice : IBLEService
    {
        protected string m_deviceName;

        protected string m_serviceName;
        protected string m_uuid;

        private string m_deviceNameCmd = "HW=";
        private string m_serviceUUIDCmd = "SUUID=";

        protected string m_commandString;

        protected List<string> m_characteristicUUIDs;

        public BLEDevice(string deviceName, string serviceName, string uuid)
        {
            m_deviceName = deviceName;
            m_serviceName = serviceName;

            m_deviceNameCmd += deviceName;
            m_serviceUUIDCmd += uuid;

            m_uuid = uuid;
            m_characteristicUUIDs = new List<string>();
        }

        public string DeviceName => m_deviceName;

        public string ServiceName => m_serviceName;

        public string ServiceUUID => m_uuid;

        /// <summary>
        /// Creates the HTTP command
        /// </summary>
        /// <returns>The HTTP Response</returns>
        /// <param name="args">A list of all the GET attributes being sent</param>
        protected string CreateCommand()
        {
            string cmdResult = m_deviceNameCmd + "&" + m_serviceUUIDCmd + "&";
            string cmdString = "CMD";

            if (m_characteristicUUIDs.Count == 0) return cmdResult;

            if (m_characteristicUUIDs.Count == 1) 
                return string.Format("{0}={1}", cmdString, m_characteristicUUIDs[0]);

            for (int i = 0; i < m_characteristicUUIDs.Count; i++)
            {
                cmdResult += string.Format("{0}{1}={2}", cmdString, i, m_characteristicUUIDs[i]);
                if (i + 1 != m_characteristicUUIDs.Count)
                    cmdResult += "&";
            }

            return cmdResult;
        }
    }

    public class ClimateDevice : BLEDevice
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
        private double m_temperatureF   = double.NaN;
        private double m_temperatureC   = double.NaN;
        private double m_humidity       = double.NaN;

        public ClimateDevice(string deviceName) : base(deviceName, 
                                                       SERVICE_NAME, 
                                                       CLIMATE_SERVICE_UUID)
        {
            // Initialize BLE service
            m_serviceName = SERVICE_NAME;
            m_uuid = CLIMATE_SERVICE_UUID;

            // Initialize BLE characteristics
            m_characteristicUUIDs.Add(TEMPERATURE_F_UUID);
            m_characteristicUUIDs.Add(TEMPERATURE_C_UUID);
            m_characteristicUUIDs.Add(HUMIDITY_UUID);

            // Initialize Command string
            m_commandString = CreateCommand();
        }

        public double TemperatureF { get { return m_temperatureF; } }

        public double TemperatureC { get { return m_temperatureC; } }

        public double Humidity { get { return m_humidity; } }

        public string CommandString { get { return m_commandString; } }

        /// <summary>
        /// Update all values from the BLE device
        /// </summary>
        /// <returns>The response message.</returns>
        public string Update()
        {
            return RPiCommLink.GetInstance().SendCommand(m_commandString);
        }

        /// <summary>
        /// Update all values asynchronously from the BLE device
        /// </summary>
        /// <returns>The response message.</returns>
        public async Task<string> UpdateAsync()
        {
            return await RPiCommLink.GetInstance().SendCommandAsync(m_commandString);
        }
    }

    public class WaterDevice : BLEDevice
    {
        public WaterDevice() : base("Blah", "", "") {}
    }

}
