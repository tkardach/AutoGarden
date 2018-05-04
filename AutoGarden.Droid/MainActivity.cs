using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Android.App;
using Android.Widget;
using Android.OS;
using System.IO;
using AutoGarden.HardwareCommunication;
using Android.Content;
using System.Threading.Tasks;

namespace AutoGarden.Droid
{
    [Activity(Label = "AutoGarden", MainLauncher = true)]
    public class MainActivity : Activity
    {
        TextView responseText;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var httpRequest = FindViewById<Button>(Resource.Id.sendResponse);
            httpRequest.Click += Button_Click;

            var addPlant = FindViewById<Button>(Resource.Id.addPlant);

            responseText = FindViewById<TextView>(Resource.Id.responseInfo);

            addPlant.Click += OnAddPlantButtonPressed;

        }

        void OnAddPlantButtonPressed(object sender, EventArgs e)
        {
            StartActivity(typeof(CreatePlantActivity));
        }

        async void Button_Click(object sender, EventArgs e)
        {
            responseText.Text = await Task.Run(() => RPiCommLink.GenericCommand("Farts"));
        }
    }
}

