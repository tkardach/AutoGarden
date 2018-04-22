using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Android.App;
using Android.Widget;
using Android.OS;
using System.IO;

namespace AutoGarden.Droid
{
    [Activity(Label = "AutoGarden", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var httpRequest = FindViewById<Button>(Resource.Id.sendResponse);
            httpRequest.Click += Button_Click;

            var addPlant = FindViewById<Button>(Resource.Id.addPlant);
            addPlant.Click += OnAddPlantButtonPressed;

        }


        void OnAddPlantButtonPressed(object sender, EventArgs e)
        {
            SetContentView(Resource.Layout.CreatePlant);
        }

        void Button_Click(object sender, EventArgs e)
        {
            var tb = FindViewById<TextView>(Resource.Id.responseInfo);

            try
            {
                string URI = "http://10.0.0.139/cgi-bin/hwctrl.py";
                string myParameters = "HW=ClimateServiceBLE1&CMD=ReadF&ID=Tombo";

                string HtmlResult = "Oh well...";
                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    HtmlResult = wc.UploadString(URI, myParameters);
                }

                tb.Text = HtmlResult;
            }
            catch (SocketException ex)
            {
                tb.Text = ex.Message;
                return;
            }
            catch (Exception ex)
            {
                tb.Text = ex.Message;
                return;
            }
            finally
            {
                clientSocket.Close();
            }
        }
    }
}

