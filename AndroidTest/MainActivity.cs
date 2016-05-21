using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Widget;
using Android.OS;
using DSLink;
using DSLink.Nodes;
using Environment = Android.OS.Environment;

namespace AndroidTest
{
    [Activity(Label = "AndroidTest", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private DSLinkContainer _dslink;

        protected override void OnCreate(Bundle bundle)
        {
            DSLink.Android.AndroidPlatform.Initialize();
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);
            Button connect = FindViewById<Button>(Resource.Id.connect_button);
            EditText url = FindViewById<EditText>(Resource.Id.broker_url);
            connect.Click += delegate
            {
                _dslink = new AndroidDSLink(new Configuration(new List<string>(), "androidtest", true, true, Environment.ExternalStorageDirectory.Path + "/.keys", brokerUrl: url.Text));
                connect.Enabled = false;
            };
        }
    }

    public class AndroidDSLink : DSLinkContainer
    {
        private Timer timer;
        private Node number;
        private Random random;

        public AndroidDSLink(Configuration config) : base(config)
        {
            random = new Random();
            timer = new Timer(0.01);
            timer.Elapsed += Elapsed;

            number = Responder.SuperRoot.CreateChild("MyNum")
                .SetDisplayName("My Number")
                .SetType("int")
                .SetValue(0)
                .BuildNode();

            timer.Start();
        }

        private void Elapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            number.Value.Set(random.Next());
        }
    }
}

