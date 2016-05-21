using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using DSLink;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using Action = System.Action;

namespace UniversalTest
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private DSLinkContainer _dslink;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof (MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

            DSLink.Universal.UniversalPlatform.Initiailize();

            _dslink =
                new ExampleDSLink(new Configuration(new List<string>(), "sdk-dotnet", responder: true,
                    brokerUrl: "http://octocat.local:8080/conn"));
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }

    class ExampleDSLink : DSLinkContainer
    {
        private Timer timer;
        private int counter;

        public ExampleDSLink(Configuration config) : base(config)
        {
            var myNum = Responder.SuperRoot.CreateChild("MyNum")
                .SetDisplayName("My Number")
                .SetType("int")
                .SetValue(0)
                .BuildNode();

            var addNum = Responder.SuperRoot.CreateChild("AddNum")
                .SetDisplayName("Add Number")
                .AddParameter(new Parameter("Number", "int"))
                .SetAction(new DSLink.Nodes.Actions.Action(Permission.Write, parameters =>
                {
                    myNum.Value.Set(parameters["Number"].Get());
                    return new List<dynamic>();
                }))
                .BuildNode();

            /*
                        Responder.SuperRoot.CreateChild("TestAction")
                            .AddParameter(new Parameter("Test", "string"))
                            .AddColumn(new Column("Status", "bool"))
                            .SetAction(new Action(Permission.Write, parameters =>
                            {
                                Console.WriteLine("ran!");
                                Console.WriteLine(parameters.Count);
                                return new List<dynamic>
                                {
                                    true
                                };
                            }));
            */

            /*
                        var testValue = Responder.SuperRoot.CreateChild("testnode")
                            .SetConfig("type", new Value("number")).BuildNode();
                        testValue.Value.Set(5);

                        testValue.Value.Set(1);
                        Random random = new Random();
                        timer = new Timer(obj =>
                        {
                            if (testValue.Subscribed)
                            {
                                testValue.Value.Set(random.Next(0, 1000));
                            }
                            Responder.SuperRoot.RemoveChild("Test" + counter);
                            Responder.SuperRoot.CreateChild("Test" + ++counter);
                        }, null, 1000, 1);
            */
        }
    }
}
