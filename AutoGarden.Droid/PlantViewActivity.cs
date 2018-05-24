
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AutoGarden.Droid
{
	[Activity(Label = "PlantViewActivity")]
    public class PlantViewActivity : Activity
    {
		TextView m_plantName, m_plantDOB, m_devices;

		Plant m_plant;

        protected override void OnCreate(Bundle savedInstanceState)
        {
			base.OnCreate(savedInstanceState);
         
			SetContentView(Resource.Layout.PlantView);
            
			// Get information from intent
			var name = Intent.GetStringExtra("PLANT_NAME");
			var dob = Intent.GetStringExtra("PLANT_DOB");

			m_plant = new Plant(name, dob);

			m_plantName = FindViewById<TextView>(Resource.Id.plantViewPlantName);
			m_plantName.Text = name;

			m_devices = FindViewById<TextView>(Resource.Id.plantViewPlantBLE);
			m_devices.Text = "Rando ass device mofoooo";
        }
    }
}
