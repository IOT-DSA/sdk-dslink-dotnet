using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Request;
using DSLink.Util.Logger;
using Foundation;
using Newtonsoft.Json.Linq;
using UIKit;
using Action = DSLink.Nodes.Actions.ActionHandler;

namespace DSLink.iOS.Example
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

            iOSPlatform.Initialize();
            var dslink =
                new ExampleDSLink(new Configuration(new List<string>(), "sdk-dotnet",
                                                    responder: true,
                                                    requester: true,
                                                    logLevel: LogLevel.Debug,
                                                    communicationFormat: "json",
                                                    keysLocation: Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/dsa_mobile.keys",
                                                    connectionAttemptLimit: -1));
            
            dslink.Connect();
            dslink.Subscribe();

            return true;
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }

    public class ExampleDSLink : DSLinkContainer
    {
        public ExampleDSLink(Configuration config) : base(config)
        {
            var testAction = Responder.SuperRoot.CreateChild("Test")
                                      .AddColumn(new Column("Column", "number"))
                                      .AddColumn(new Column("Column2", "string"))
                                      .SetResult(ResultType.Stream)
                                      .SetAction(new ActionHandler(Permission.Write, Test))
                                      .SetInvokable(Permission.Read)
                                      .BuildNode();
        }

        public async void Test(InvokeRequest request)
        {
            //request.Mode = Table.Mode.Append;
            for (int i = 0; i < 999; i++)
            {
                await request.UpdateTable(new Table
                {
                    new Row
                    {
                        true,
                        "test" + i,
                    }
                });
                await Task.Delay(1000);
            }
            await request.Close();
        }

        public async void Subscribe()
        {
            /*await Requester.Subscribe("/sys/dataInPerSecond", (response) =>
            {
                Console.WriteLine(response.Value);
            });*/
        }
    }
}
