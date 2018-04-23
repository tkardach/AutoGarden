using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AutoGarden.HardwareCommunication
{

    public class RPiCommLink
    {
        private const string URI =
            "http://10.0.0.139/cgi-bin/hwctrl.py";
        private const string ID_CMD = "ID=Tombo";

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

        public static string RunClient(string machineName, string serverName)
        {
            // Create a TCP/IP client socket.
            // machineName is the host running the server application.
            TcpClient client = new TcpClient(machineName, 4443);
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
                sslStream.AuthenticateAsClient(serverName);
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

            // Encode a test message into a byte array.
            // Signal the end of the message using the "<EOF>".
            byte[] messsage = Encoding.UTF8.GetBytes("Hello from the client.<EOF>");

            // Send message to the server. 
            sslStream.Write(messsage);
            sslStream.Flush();

            // Read message from the server.
            string serverMessage = ReadMessage(sslStream);
            Console.WriteLine("Server says: {0}", serverMessage);

            // Close the client connection.
            client.Close();

            Console.WriteLine("Client closed.");

            return serverMessage;
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
