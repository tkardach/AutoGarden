
using System;

using Android.App;
using Android.Content;
using Android.OS;

namespace AutoGarden.Droid
{
    [Service(Label = "BLEDeviceService")]
    [IntentFilter(new String[] { "com.yourname.BLEDeviceService" })]
    public class BLEDeviceService : Service
    {
        IBinder binder;

        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            // start your service logic here

            // Return the correct StartCommandResult for the type of service you are building
            return StartCommandResult.NotSticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            binder = new BLEDeviceServiceBinder(this);
            return binder;
        }
    }

    public class BLEDeviceServiceBinder : Binder
    {
        readonly BLEDeviceService service;

        public BLEDeviceServiceBinder(BLEDeviceService service)
        {
            this.service = service;
        }

        public BLEDeviceService GetBLEDeviceService()
        {
            return service;
        }
    }
}
