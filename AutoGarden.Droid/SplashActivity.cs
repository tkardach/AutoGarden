
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace AutoGarden.Droid
{
	[Activity(Theme = "@style/MyTheme.Splash",Label="AutoGarden", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        protected override void OnResume()
		{
			base.OnResume();
			Task startupWork = new Task(() => { StartupWork(); });
			startupWork.Start();
		}

        void StartupWork()
		{         
            // Start BLE Scan Service
            Intent bleServiceIntent = new Intent(this, typeof(BLEScanService));
            StartService(bleServiceIntent);         
			StartActivity(new Intent(Application.Context, typeof(PlantListActivity)));
		}
    }
}
