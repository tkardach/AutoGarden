
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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CreatePlant);

            InitPlantTypes();

            Task.Run(() => InitBLEScan()).Wait();
        }

        private void InitPlantTypes()
        {
            /**** Create drop down for plant types ****/
            plantTypes = (Spinner)FindViewById(Resource.Id.plantType);

            plantTypes.ItemSelected += spinner_ItemSelected;
            plantTypeAdapter = ArrayAdapter.CreateFromResource(this,
                                                          Android.Resource.Layout.SimpleSpinnerItem,
                                                          Resource.Array.plant_types);

            plantTypeAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            plantTypes.Adapter = plantTypeAdapter;
        }

        private void InitBLEScan()
        {
            /*** Create drop down for know BLE devices ***/
            try
            {
                var serviceList = DatabaseConnection.GetBLEServices();
                var deviceList = RPiCommLink.ScanCommand();

                bleDevices = (Spinner)FindViewById(Resource.Id.bleDevices);

                var displayList = new List<string>();
                if (deviceList != null)
                {
                    foreach (var dev in deviceList)
                    {
                        displayList.Add(dev.DeviceName);
                        Log.Info(TAG, dev.DeviceName);
                    }
                }

                bleDevices.ItemSelected += spinner_ItemSelected;

                bleServiceAdapter = new ArrayAdapter(this,
                                                     Android.Resource.Layout.SimpleSpinnerItem,
                                                     displayList);

                bleServiceAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                bleDevices.Adapter = bleServiceAdapter;
            }
            catch (Exception e)
            {
                string toast = string.Format("{0}", e.Message);
            }
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            string toast = string.Format("{0} Plant", spinner.GetItemAtPosition(e.Position));
        }
    }
}

