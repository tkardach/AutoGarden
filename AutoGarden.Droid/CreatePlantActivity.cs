
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

        Spinner bleDevices;
        Spinner plantTypes;
        ArrayAdapter plantTypeAdapter;
        ArrayAdapter bleServiceAdapter;
        List<string> displayList;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CreatePlant);

            InitPlantTypes();

            displayList = new List<string>();

            InitBLESpinner();

            var list = await Task.Run(() => GetBLEScannedDevList());

            UpdateBLESpinnerAdapter(list);
        }

        private void InitPlantTypes()
        {
            RunOnUiThread(() =>
            {
                /**** Create drop down for plant types ****/
                plantTypes = (Spinner)FindViewById(Resource.Id.plantType);
                plantTypes.ItemSelected += spinner_ItemSelected;

                // Create Adapter
                plantTypeAdapter = ArrayAdapter.CreateFromResource(this,
                                                              Resource.Array.plant_types,
                                                              Android.Resource.Layout.SimpleSpinnerItem);

                plantTypeAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                plantTypes.Adapter = plantTypeAdapter;
            });
        }

        private void InitBLESpinner()
        {
            RunOnUiThread(() => {
                /**** Create drop down for ble devices ****/
                bleDevices = (Spinner)FindViewById(Resource.Id.bleDevices);
                bleDevices.ItemSelected += spinner_ItemSelected;

                // Create Adapter
                bleServiceAdapter = new ArrayAdapter(this,
                                                     Android.Resource.Layout.SimpleSpinnerItem,
                                                     displayList);
                bleServiceAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                bleDevices.Adapter = bleServiceAdapter;
            });
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
                foreach (var dev in deviceList)
                {
                    list.Add(dev.DeviceName);
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

        void UpdateBLESpinnerAdapter(List<string> list)
        {
            RunOnUiThread(() =>
            {
                if (list == null) return;
                bleServiceAdapter.Clear();
                bleServiceAdapter.AddAll(list);
                bleServiceAdapter.NotifyDataSetChanged();
            });
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            string toast = string.Format("{0} Plant", spinner.GetItemAtPosition(e.Position));
        }
    }
}

