
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AutoGarden.Bluetooth;
using AutoGarden.Database;
using AutoGarden.HardwareCommunication;

namespace AutoGarden.Droid
{
    [Service(Label = "BLEScanService")]
    [IntentFilter(new String[] { "com.yourname.BLEScanService" })]
    public class BLEScanService : Service
    {
		IBinder binder;
		Dictionary<string, BLEDevice> m_nearbyBleDevices;
		List<string> m_scannedDevices;

		public bool Scanning { get; private set; }
		public bool ScanComplete { get; private set; }

		public override void OnCreate()
		{
			base.OnCreate();

			Scanning = false;
			ScanComplete = false;

			m_nearbyBleDevices = new Dictionary<string, BLEDevice>();
		}

		public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
		{
            Scanning = true;

			// start your service logic here
			m_scannedDevices = GetBLEScannedDevList();

			ScanComplete = true;

            // Return the correct StartCommandResult for the type of service you are building
            return StartCommandResult.NotSticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            binder = new BLEScanServiceBinder(this);
            return binder;
        }
        
		public Dictionary<string, BLEDevice> GetNearbyDeviceDictionary()
        {
            return m_nearbyBleDevices;
        }

        public List<string> GetScannedDevices()
        {
            return m_scannedDevices;
        }

        private List<string> GetBLEScannedDevList()
        {
            /*** Create drop down for know BLE devices ***/
            try
            {
                var serviceList = DatabaseConnection.GetBLEServices();
                var deviceList = RPiCommLink.ScanCommand();

                if (deviceList == null) return null;

                // Get device names
                var list = new List<string>();
                m_nearbyBleDevices.Clear();

                // Fill up the list of device names, and nearby devices
                foreach (var dev in deviceList)
                {
                    // Add the device to scan list and nearby device list
                    if (!m_nearbyBleDevices.ContainsKey(dev.DeviceName) &&
                        !dev.DeviceName.Equals("No Name"))
                    {
                        list.Add(dev.DeviceName);
                        m_nearbyBleDevices.Add(dev.DeviceName, dev);
                    }
                    else
                    {
                        if (!m_nearbyBleDevices.ContainsKey(dev.MAC))
                        {
                            list.Add(dev.MAC);
                            m_nearbyBleDevices.Add(dev.MAC, dev);
                        }
                    }
                }

                return list;
            }
            catch (Exception e)
            {
                string toast = string.Format("{0}", e.Message);
                return null;
            }
        }
    }

    public class BLEScanServiceBinder : Binder
    {
        readonly BLEScanService service;

        public BLEScanServiceBinder(BLEScanService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Gets a list of all the nearby BLE device names.
        /// </summary>
        /// <returns>The names of the nearby BLE devices.</returns>
        public BLEScanService GetBLEScanService()
        {
            return service;
        }

		public Dictionary<string, BLEDevice> GetNearbyDeviceDictionary()
        {
            return service.GetNearbyDeviceDictionary();
        }

        public List<string> GetScannedDevices()
        {
			return service.GetScannedDevices();
        }
    }
}
