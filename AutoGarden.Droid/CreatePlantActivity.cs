
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AutoGarden.Bluetooth;
using AutoGarden.Database;
using AutoGarden.HardwareCommunication;
using FR.Ganfra.Materialspinner;

namespace AutoGarden.Droid
{
    [Activity(Label = "CreatePlantActivity")]
    public class CreatePlantActivity : Activity
    {
        public const string TAG = "AUTOGARDEN";

        Spinner m_bleDevices;
        Spinner m_plantTypes;
        ArrayAdapter m_plantTypeAdapter;
        ArrayAdapter m_bleServiceAdapter;

		CreatePlantConnection m_connectionStatus;      
        Dictionary<string, BLEDevice> m_nearbyBleDevices;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CreatePlant);
            
            InitializeUIComponents();

			m_connectionStatus = new CreatePlantConnection(this);
            //m_nearbyBleDevices = new Dictionary<string, BLEDevice>();
            //var list = await Task.Run(() => GetBLEScannedDevList());
         
        }

		protected override void OnStart()
		{
			base.OnStart();

			Intent intent = new Intent(this, typeof(BLEScanService));
			BindService(intent, m_connectionStatus, Bind.None);
		}

		/// <summary>
		/// Initializes the UI objects for the CreatePlants page
		/// </summary>
		private void InitializeUIComponents()
        {
            RunOnUiThread(() =>
            {
                /**** Create drop down for plant types ****/
                m_plantTypes = (Spinner)FindViewById(Resource.Id.plantType);
                m_plantTypes.ItemSelected += spinner_ItemSelected;

                // Create Adapter
                m_plantTypeAdapter = ArrayAdapter.CreateFromResource(this,
                                                              Resource.Array.plant_types,
                                                              Android.Resource.Layout.SimpleSpinnerItem);

                m_plantTypeAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                m_plantTypes.Adapter = m_plantTypeAdapter;


                /**** Create drop down for ble devices ****/
                m_bleDevices = (Spinner)FindViewById(Resource.Id.bleDevices);
                m_bleDevices.ItemSelected += spinner_ItemSelected;

                // Create Adapter
                m_bleServiceAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem);
                m_bleServiceAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                m_bleDevices.Adapter = m_bleServiceAdapter;
            });
        }

        /// <summary>
        /// Scans for nearby BLE devices and returns a list of device names.
        /// </summary>
        /// <returns>A list of the scanned device names.</returns>
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

                    Log.Info(TAG, dev.DeviceName);
                }

                return list;
            }
            catch (Exception e)
            {
                string toast = string.Format("{0}", e.Message);
                return null;
            }
        }

        /// <summary>
        /// Updates the BLE Spinner adapter list.
        /// </summary>
        /// <param name="list">List of BLE device names.</param>
        void UpdateBLESpinnerAdapter(List<string> list)
        {
            RunOnUiThread(() =>
            {
                if (list == null) return;

                // Add list to BLE spinner options
                m_bleServiceAdapter.Clear();
                m_bleServiceAdapter.AddAll(list);
                m_bleServiceAdapter.NotifyDataSetChanged();
            });
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            string toast = string.Format("{0} Plant", spinner.GetItemAtPosition(e.Position));
        }

		class CreatePlantConnection : Java.Lang.Object, IServiceConnection
		{
			CreatePlantActivity m_activity;

			public bool IsConnected { get; private set; }
			public BLEScanServiceBinder Binder { get; private set; }

			public CreatePlantConnection(CreatePlantActivity activity)
			{
				IsConnected = false;
				Binder = null;
				m_activity = activity;
			}

            public void OnServiceConnected(ComponentName name, IBinder service)
            {
				Binder = service as BLEScanServiceBinder;
				IsConnected = this.Binder != null;

				if (IsConnected)
					m_activity.UpdateBLESpinnerAdapter(Binder.GetScannedDevices());
            }

            public void OnServiceDisconnected(ComponentName name)
			{
                IsConnected = false;
                Binder = null;
            }
        }
    }
   
}

