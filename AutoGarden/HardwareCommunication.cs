using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoGarden.Bluetooth;
using AutoGarden.Database;
using AutoGarden.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoGarden.HardwareCommunication
{
    #region RaspberryPi Comm class

    public static class RPiCommLink
    {
        // SSL Stream constants for RPi connection
        private const string URL = "10.0.0.139";
        private const string SERVERNAME = "10.0.0.139";
        private const int PORT = 4443;

        // Command Strings
        private const string COMMAND_STR = "Command";
        private const string SCAN_CMD = "Scan";
        private const string GENERIC_CMD = "Generic";
        private const string ADD_CMD = "Add";
        private const string REMOVE_CMD = "Remove";
        private const string MAC_STR = "MAC";
        private const string BLEDEVICE_STR = "BLEDevice";


        #region Protocol Commands

        public static string GenericCommand(string command)
        {
            command = string.Format("{{\"{0}\" : \"{1}\", " +
                                    "\"{2}\" : \"{3}\" }}",
                                    COMMAND_STR, GENERIC_CMD,
                                    BLEDEVICE_STR, command);

            return RunClient(command, 5000);
        }

        public static async Task<string> GenericCommandAsync(string command)
        {
            command = string.Format("{{\"{0}\" : \"{1}\", " +
                                    "\"{2}\" : \"{3}\" }}",
                                    COMMAND_STR, GENERIC_CMD,
                                    BLEDEVICE_STR, command);

            return await RunClientAsync(command, 5000);
        }

        public static List<BLEDevice> ScanCommand()
        {
            // Create command string to send to server
            var command = string.Format("{{ \"{0}\" : \"{1}\" }}",
                                        COMMAND_STR, SCAN_CMD);

            // Get return value as JSON Object string
            var jsonArray = RunClient(command, 60000);

            JObject devices = null;

            try 
            {
                devices = JObject.Parse(jsonArray);
                if (devices == null) return null;
            }
            catch (JsonException e)
            {
                Console.WriteLine("JSON Error: " + e.Message);
                return null;
            }

            return GenerateScannedDeviceList(devices);
        }

        public static async Task<List<BLEDevice>> ScanCommandAsync()
        {
            // Create command string to send to server
            var command = string.Format("{{ \"{0}\" : \"{1}\" }}",
                                        COMMAND_STR, SCAN_CMD);

            // Get return value as JSON Object string
            var jsonArray = await RunClientAsync(command, 60000);

            JObject devices = null;

            try
            {
                devices = JObject.Parse(jsonArray);
                if (devices == null) return null;
            }
            catch (JsonException e)
            {
                Console.WriteLine("JSON Error: " + e.Message);
                return null;
            }

            return await Task.Run(() => GenerateScannedDeviceList(devices));
        }

        private static List<BLEDevice> GenerateScannedDeviceList(JObject devices)
        {
            var scannedDevices = new List<BLEDevice>();

            var recognizedServices = DatabaseConnection.GetBLEServices();

            // Itterate through each object
            foreach (var dev in devices)
            {
                // Initialize a Scanned Device
                var scannedDev = new BLEDevice("No Name", "No MAC");

                // Set ScannedDevice attributes
                scannedDev.MAC = (string)dev.Value["MAC"];

                // Parse the device content for wanted values
                var content = (JObject)dev.Value["Content"];
                foreach (var data in content)
                {
                    switch (data.Key)
                    {
                        case "Complete Local Name":
                            scannedDev.DeviceName = (string)data.Value;
                            break;
                        case "Complete 128b Services":
                            var service =
                                recognizedServices.Single(o => o.UUID.Equals((string)data.Value));
                            scannedDev.AddService(service);
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

        public static async Task<string> AddDeviceCommandAsync(string mac)
        {
            var command = string.Format("{{\"{0}\" : \"{1}\", " +
                                        "\"{2}\" : \"{3}\" }}",
                                        COMMAND_STR, ADD_CMD,
                                        MAC_STR, mac);

            // Get return value as JSON Object string
            return await RunClientAsync(command, 10000);
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

        public static async Task<string> RemoveDeviceCommandAsync(string mac)
        {
            var command = string.Format("{{\"{0}\" : \"{1}\", " +
                                        "\"{2}\" : \"{3}\" }}",
                                        COMMAND_STR, REMOVE_CMD,
                                        MAC_STR, mac);

            // Get return value as JSON Object string
            return await RunClientAsync(command, 10000);
        }

        #endregion

        #region Client/Server Communication

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

        public static string RunClient(string commands)
        {
            return RunClient(commands, 5000);
        }

        public static string RunClient(string commands, int timeout)
        {
            using (var client = new TcpClient())
            {
                // Create a TCP/IP client socket.
                // machineName is the host running the server application.
                var result = client.BeginConnect(SERVERNAME, PORT, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

                // If TCP Connection was a failure, report it
                if (!success)
                {
                    var error = string.Format("Failed to connect to {0}:{1}",
                                                SERVERNAME, PORT);
                    Console.WriteLine(error);
                    return error;
                }

                client.EndConnect(result);

                Console.WriteLine("Client connected.");

                using (var sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                null
                ))
                {
                    // Begin ssl authentication and create the SslStream
                    result = sslStream.BeginAuthenticateAsClient(SERVERNAME, null, null);
                    success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

                    if (!success)
                    {
                        var error = string.Format("Failed to authenticate with {0}", SERVERNAME);
                        Console.WriteLine(error);
                        return error;
                    }

                    sslStream.EndAuthenticateAsClient(result);

                    // Set timeout values
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


                    client.GetStream().Close();
                    // Close the connection.
                    client.Close();

                    return serverMessage;
                }
            }
        }// end RunClient

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

            var resultString = messageData.ToString().Replace("<EOF>", "");

            return resultString;
        }

        public static async Task<string> RunClientAsync(string commands)
        {
            return await RunClientAsync(commands, 5000);
        }

        public static async Task<string> RunClientAsync(string commands, int timeout)
        {
            using (var client = new TcpClient())
            {
                // Create a TCP/IP client socket.
                // machineName is the host running the server application.
                var result = client.BeginConnect(SERVERNAME, PORT, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

                // If TCP Connection was a failure, report it
                if (!success)
                {
                    var error = string.Format("Failed to connect to {0}:{1}",
                                                SERVERNAME, PORT);
                    Console.WriteLine(error);
                    return error;
                }

                client.EndConnect(result);

                Console.WriteLine("Client connected.");

                using (var sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                null
                ))
                {
                    // Begin ssl authentication and create the SslStream
                    result = sslStream.BeginAuthenticateAsClient(SERVERNAME, null, null);
                    success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

                    if (!success)
                    {
                        var error = string.Format("Failed to authenticate with {0}", SERVERNAME);
                        Console.WriteLine(error);
                        return error;
                    }

                    sslStream.EndAuthenticateAsClient(result);

                    // Set timeout values
                    sslStream.ReadTimeout = sslStream.WriteTimeout = timeout;

                    // Encode a test message into a byte array.
                    // Signal the end of the message using the "<EOF>".
                    byte[] messsage = Encoding.UTF8.GetBytes(commands);


                    // Send message to the server. 
                    sslStream.Write(messsage);
                    sslStream.Flush();

                    // Read message from the server.
                    string serverMessage = await ReadMessageAsync(sslStream);
                    Console.WriteLine("Server says: {0}", serverMessage);


                    client.GetStream().Close();
                    // Close the connection.
                    client.Close();

                    return serverMessage;
                }
            }
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


            var resultString = messageData.ToString().Replace("<EOF>", "");

            return resultString;
        }

        #endregion
    }

    #endregion
}
