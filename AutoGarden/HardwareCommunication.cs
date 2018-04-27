﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AutoGarden.HardwareCommunication
{

    public class RPiCommLink
    {
        // HTTP constants for RPi connection
        private const string URI    = "http://10.0.0.139/cgi-bin/hwctrl.py";
        private const string ID_CMD = "ID=Tombo";

        // SSL Stream constants for RPi connection
        private const string    URL         = "10.0.0.139";
        private const string    SERVERNAME  = "10.0.0.139";
        private const int       PORT        = 4443;

        // Command Strings
        private const string COMMAND_STR    = "Command";
        private const string SCAN_CMD       = "Scan";
        private const string GENERIC_CMD    = "Generic";
        private const string ADD_CMD        = "Add";
        private const string REMOVE_CMD     = "Remove";
        private const string MAC_STR        = "MAC";
        private const string BLEDEVICE_STR  = "BLEDevice";


        private static RPiCommLink m_rpiCommLink;

        private RPiCommLink()
        {
            // Disable Certificate Verification. We are only connecting to a 
            // trusted source so we are not worried about the server-side cert.
            ServicePointManager.ServerCertificateValidationCallback += 
                (sender, cert, chain, sslPolicyErrors) => true;
        }

        public static RPiCommLink GetInstance()
        {
            if (m_rpiCommLink == null)
                m_rpiCommLink = new RPiCommLink();
            return m_rpiCommLink;
        }

        public static string SendCommand(string cmd)
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
        }

        public static async Task<string> SendCommandAsync(string cmd)
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
        }

        private static Hashtable certificateErrors = new Hashtable();

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static string GenericCommand(string command)
        {
            command = string.Format("{{\"{0}\" : \"{1}\", " +
                                    "\"{2}\" : {3} }}", 
                                    COMMAND_STR, GENERIC_CMD,
                                    BLEDEVICE_STR, command);
            
            return RunClient(command, 5000);
        }

        public struct ScannedDevice
        {
            public string DeviceName;
            public string MAC;
            public List<string> ServiceUUID;
        }

        public static List<ScannedDevice> ScanCommand()
        {
            // Create command string to send to server
            var command = string.Format("{{ \"{0}\" : \"{1}\" }}",
                                        COMMAND_STR, SCAN_CMD);

            // Get return value as JSON Object string
            var jsonArray = RunClient(command, 10000);

            var devices = JObject.Parse(jsonArray);

            var scannedDevices = new List<ScannedDevice>();

            // Itterate through each object
            foreach (var dev in devices)
            {
                // Initialize a Scanned Device
                var scannedDev = new ScannedDevice();
                scannedDev.DeviceName = "No Name";
                scannedDev.MAC = "No MAC";

                // Set ScannedDevice attributes
                scannedDev.MAC = (string)dev.Value["MAC"];

                // Parse the device content for wanted values
                var content = (JObject)dev.Value["Content"];
                foreach (var data in content)
                {
                    switch(data.Key)
                    {
                        case "Complete Local Name":
                            scannedDev.DeviceName = (string)data.Value;
                            break;
                        case "Complete 128b Services":
                            if (scannedDev.ServiceUUID == null)
                                scannedDev.ServiceUUID = new List<string>();
                            scannedDev.ServiceUUID.Add((string)data.Value);
                            break;
                        default:
                            break;
                    }
                }

                scannedDevices.Add(scannedDev);
            }

            return scannedDevices;
        }

        public static string AddDeviceCommand(string mac)
        {
            var command = string.Format("{{\"{0}\" : \"{1}\", " +
                                        "\"{2}\" : \"{3}\" }}",
                                        COMMAND_STR, ADD_CMD,
                                        MAC_STR, mac);
            
            // Get return value as JSON Object string
            return RunClient(command, 10000);
        }

        public static string RemoveDeviceCommand(string mac)
        {
            var command = string.Format("{{\"{0}\" : \"{1}\", " +
                                        "\"{2}\" : \"{3}\" }}",
                                        COMMAND_STR, REMOVE_CMD,
                                        MAC_STR, mac);

            // Get return value as JSON Object string
            return RunClient(command, 10000);
        }

        public static string RunClient(string commands)
        {
            return RunClient(commands, 5000);
        }

        public static string RunClient(string commands, int timeout)
        {
            // Create a TCP/IP client socket.
            // machineName is the host running the server application.
            TcpClient client = new TcpClient(URL, PORT);
            Console.WriteLine("Client connected.");

            // Create an SSL stream that will close the client's stream.
            SslStream sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                null
                );

            // The server name must match the name on the server certificate.
            try
            {
                sslStream.AuthenticateAsClient(SERVERNAME);

                sslStream.ReadTimeout = sslStream.WriteTimeout = timeout;

                // Encode a test message into a byte array.
                // Signal the end of the message using the "<EOF>".
                byte[] messsage = Encoding.UTF8.GetBytes(commands);


                // Send message to the server. 
                sslStream.Write(messsage);
                sslStream.Flush();

                // Read message from the server.
                string serverMessage = ReadMessage(sslStream);
                Console.WriteLine("Server says: {0}", serverMessage);

                Console.WriteLine("Client closed.");

                return serverMessage;
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                client.Close();
                return "Authentication Failed";
            }
            finally
            {
                // Close the connection.
                client.Close();
                sslStream.Close();
            }
        }

        static string ReadMessage(SslStream sslStream)
        {
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.

            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                bytes = sslStream.Read(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF.
                if (messageData.ToString().IndexOf("<EOF>", StringComparison.CurrentCulture) != -1)
                {
                    break;
                } 

                if (messageData.Length == bytes) break;
            } while (bytes != 0);

            return messageData.ToString();
        }

        public static async Task<string> RunClientAsync(string commands)
        {
            return await RunClientAsync(commands, 5000);
        }

        public static async Task<string> RunClientAsync(string commands, int timeout)
        {
            // Create a TCP/IP client socket.
            // machineName is the host running the server application.
            TcpClient client = new TcpClient(URL, PORT);
            Console.WriteLine("Client connected.");

            // Create an SSL stream that will close the client's stream.
            SslStream sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                null
                );

            // The server name must match the name on the server certificate.
            try
            {
                await sslStream.AuthenticateAsClientAsync(SERVERNAME);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                client.Close();
                return "Authentication Failed";
            }

            sslStream.ReadTimeout = sslStream.WriteTimeout = timeout;

            // Encode a test message into a byte array.
            // Signal the end of the message using the "<EOF>".
            byte[] messsage = Encoding.UTF8.GetBytes(commands + "<EOF>");

            // Send message to the server. 
            await sslStream.WriteAsync(messsage, 0, messsage.Length);
            await sslStream.FlushAsync();

            // Read message from the server.
            string serverMessage = await ReadMessageAsync(sslStream);
            Console.WriteLine("Server says: {0}", serverMessage);

            // Close the client connection.
            client.Close();

            Console.WriteLine("Client closed.");

            return serverMessage;
        }

        static async Task<string> ReadMessageAsync(SslStream sslStream)
        {
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                bytes = await sslStream.ReadAsync(buffer, 0, buffer.Length);

                if (bytes == 0) return "";

                // Use Decoder class to convert from bytes to UTF8
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF.
                if (messageData.ToString().IndexOf("<EOF>", StringComparison.CurrentCulture) != -1)
                {
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }

    }

    /// <summary>
    /// Creates a JSON object string representation of the object
    /// </summary>
    public interface JSONParsable
    {
        string CreateJSONRequest();
        string ParseJSONResponse();
        string ToJSON();
    }

    public class BLEDevice : JSONParsable
    {
        string m_deviceName;
        string m_commandString;

        Dictionary<string, BLEService> m_services;

        public BLEDevice(string deviceName)
        {
            m_deviceName = deviceName;
            m_services = new Dictionary<string, BLEService>();
        }

        public string DeviceName => m_deviceName;

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
                string.Format("{{ \"DeviceName\" : \"{0}\"," +
                              "\"Services\" : [{1}] }}",
                                  m_deviceName, servicesString);
            return jsonString;
        }

        public string ParseJSONResponse()
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

    public abstract class BLEService : JSONParsable
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
        public abstract string ParseJSONResponse();
        public abstract string ToJSON();
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
        private double m_temperatureF   = double.NaN;
        private double m_temperatureC   = double.NaN;
        private double m_humidity       = double.NaN;

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
                string.Format("{{ \"{0}\" : \"{1}\" }}," +
                              "{{ \"{2}\" : \"{3}\" }}," +
                              "{{ \"{4}\" : \"{5}\" }}",
                              TEMPERATURE_F_UUID, READ,
                              TEMPERATURE_C_UUID, READ,
                              HUMIDITY_UUID, READ);

            string jsonString = 
                string.Format("{{ \"UUID\" : \"{0}\"," +
                              "\"Characteristics\" : [{1}] }}",
                                m_uuid, characteristicString);

            return jsonString;
        }

        public override string ParseJSONResponse()
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
        public WaterService() : base("Blah", "") {}

        public override string CreateJSONRequest()
        {
            throw new NotImplementedException();
        }

        public override string ParseJSONResponse()
        {
            throw new NotImplementedException();
        }

        public override string ToJSON()
        {
            throw new NotImplementedException();
        }
    }

}
