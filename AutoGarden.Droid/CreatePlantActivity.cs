
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
using FR.Ganfra.Materialspinner;

namespace AutoGarden.Droid
{
    [Activity(Label = "CreatePlantActivity")]
    public class CreatePlantActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CreatePlant);

            var plantTypes = (MaterialSpinner) FindViewById(Resource.Id.plantType);

            plantTypes.ItemSelected += spinner_ItemSelected;
            var adapter = ArrayAdapter.CreateFromResource(this,
                                                          Android.Resource.Layout.SimpleSpinnerItem,
                                                          Resource.Array.plant_types);
            
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            plantTypes.Adapter = adapter;
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            string toast = string.Format("{0} Plant", spinner.GetItemAtPosition(e.Position));
        }
    }
}

