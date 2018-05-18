
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
using AutoGarden.Droid.GardenGridView;

namespace AutoGarden.Droid
{
    [Activity(Label = "PlantListActivity")]
    public class PlantListActivity : Activity
    {
		GridView m_plantGrid;
		ImageButton m_addPlantButton;

		public Plant[] m_plants = {
		    new Plant("Plant McPlantface"),
            new Plant("Spud McKenzie"),
			new Plant("Bubba Sparks"),
            new Plant("Planty McPlanterson"),
            new Plant("Water Udoin"),
			new Plant("Dirty Sanchez"),
            new Plant("Mr. Herb"),
            new Plant("Bob Parsley"),
			new Plant("Willow Glen"),
            new Plant("I like the plantz"),
            new Plant("Mrs. Herb"),
            new Plant("Doge"),
			new Plant("{Plant pun}"),
			new Plant("Tree McGee"),
            new Plant("Rad Raddish"),
            new Plant("Carrie Otter"),
            new Plant("Broc Olee"),
            new Plant("Spin Itch")
		};

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.GardenGridViewLayout);

			m_plantGrid = FindViewById<GridView>(Resource.Id.gardenGridView);
			m_plantGrid.Adapter = new GardenGridAdapter(this, m_plants);

            // Get the add button and attach a click listener         
			m_addPlantButton = 
				((GardenGridAdapter)m_plantGrid.Adapter).AddPlantView.FindViewById<ImageButton>(Resource.Id.gridAddButton);
			m_addPlantButton.Click += OnAddPlantButtonPressed;
        }

		void OnAddPlantButtonPressed(object sender, EventArgs e)
		{
			StartActivity(new Intent(Application.Context, typeof(CreatePlantActivity)));
		}
    }
}
