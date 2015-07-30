using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Globalization;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using System.Collections.ObjectModel;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DEEP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensorChooser sensorChooser;

        /// <summary>
        /// A list of all the interactive objects in the program.
        /// </summary>
        private List<DEEPKinectObjectBaseClass> InteractiveObjects;

        /// <summary>
        /// A list of all the interactive elements on the UI.
        /// </summary>
        private List<Ellipse> InteractiveUIElements;

        /// <summary>
        /// This timer goes off often to advance interactions between 
        /// all the parts involved.
        /// </summary>
        private System.Timers.Timer simulationTick;

        /// <summary>
        /// This is the first method that is run. Start here!
        /// </summary>
        public MainWindow()
        {
            //Don't put anything before this method! It starts the program.
            InitializeComponent();

            //Set up all Kinect-related stuff. Don't change this!
            InitializeKinect();

            /* Make a list of all the Kinect-enabled objects in the app, and
             * add them to the list. */
            InteractiveObjects = new List<DEEPKinectObjectBaseClass>();

            /* Make the circles on the screen interactive. */
            DEEPKinectObjectBaseClass kinectObject = new DEEPKinematicObject(10d, this.ball, this.backgroundRectangle, true, true);

            /* Add all of the interactive objects to the list, so we can keep track of them for later. */
            InteractiveObjects.Add(kinectObject);

            /* Also add the circles on the UI to the list. */
            InteractiveUIElements = new List<Ellipse>();
            InteractiveUIElements.Add(ball);

            /* Make it so we can handle presses on the background to make new circles. */
            KinectRegion.SetIsPressTarget(this.backgroundRectangle, true);
            KinectRegion.AddHandPointerPressHandler(this.backgroundRectangle, OnBackgroundPressHandler);

            //Here, we initialize the simulationTick, which we will later use for effects.
            simulationTick = new System.Timers.Timer(DEEPKinectObjectBaseClass.internalRefreshRate * 1000d);
            simulationTick.Elapsed += simulationTick_Elapsed;
            simulationTick.Start();
        }

        /// <summary>
        /// This timer goes off very quickly to keep all the objects
        /// moving. Put anything that requires interaction between many objects here.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void simulationTick_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            /* Here we make calculations about how all the objects interact. */

            /* Find out if there are hand cursors to be had. */

            ReadOnlyObservableCollection<HandPointer> handPointers = kinectRegion.HandPointers;

            /* First we look at the interaction of the hand with the other objects. */
            foreach (HandPointer hand in handPointers)
            {
                foreach (DEEPKinectObjectBaseClass appObject in InteractiveObjects)
                {
                    appObject.InteractWithHandPointer(hand);
                }
            }

            /* Before we interact, we have to make sure there are at least two objects to 
             * have interactions. The loops fail in a colourful way if this is not so. */
            if (InteractiveObjects.Count > 1)
            {
                /* Now we look at all the interactions between the objects. One InteractWith()
                 * call processes both of the objects involved, so we just have to make sure
                 * every pair of objects is covered once. */
                for (int i = 0; i < InteractiveObjects.Count - 1; i++)
                {
                    for (int j = i + 1; j < InteractiveObjects.Count; j++)
                    {
                        /* This magic incantation makes sure the code is executed in a 
                        * thread-safe way. */
                        try
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                InteractiveObjects[i].InteractWith(InteractiveObjects[j]);
                            }));
                        }
                        catch (Exception ex)
                        {
                            /* Catch end-of-program exceptions to stop it from complaining. */
                        }
                    }
                }
            }

            /* Finally, we look at any interactions of the objects and the walls. We want to make
             * sure they don't fly away. */
            for (int i = 0; i < InteractiveObjects.Count; i++)
            {
                try
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        InteractiveObjects[i].InteractWithWindowBorder(kinectRegionGrid);
                    }));
                }
                catch (Exception ex)
                {
                    /* Don't catch anything here yet. */
                }
            }
        }

        /// <summary>
        /// This method runs every time the user presses on the background.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void OnBackgroundPressHandler(object sender, HandPointerEventArgs args)
        {
            /* Create a new circle on the screen where the background was pressed. */
            DenseVector position = new DenseVector(new double[] { args.HandPointer.GetPosition(kinectRegion).X, 
                                                                  args.HandPointer.GetPosition(kinectRegion).Y });
            Ellipse genEllipse = CreateEllipse(160d, position);

            /* Tie a DEEPKinectObject to the ellipse on screen. */
            DEEPKinematicObject newGravObject = new DEEPKinematicObject(10d, genEllipse, this.backgroundRectangle,
                                                                                true, true);
            /* Add it to the list of items in the app. */
            InteractiveObjects.Add(newGravObject);

            /* Also add the circle on the UI to the list, so we can keep track of it. */
            InteractiveUIElements.Add(genEllipse);
        }

        #region Don't modify these. They're necessary to make the program work, but you won't need to change them.

        /// <summary>
        /// Creates a circle on the UI at a particular point.
        /// </summary>
        /// <param name="diameter">The diameter of the circle, in pixels.</param>
        /// <param name="position">The position of the circle, as a distance from the
        /// top and left sides of the UI window.</param>
        /// <returns></returns>
        private Ellipse CreateEllipse(double diameter, DenseVector position)
        {
            Ellipse genEllipse = new Ellipse();
            
            genEllipse.Height = diameter;
            genEllipse.Width = diameter;

            genEllipse.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            genEllipse.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            double left = position[0] - diameter / 2;
            double top = position[1] - diameter / 2;
            genEllipse.Margin = new Thickness(left, top, 0, 0);

            genEllipse.StrokeThickness = 5;
            genEllipse.Stroke = new SolidColorBrush(Colors.Black);
            genEllipse.Fill = new SolidColorBrush(Colors.Red);

            this.kinectRegionGrid.Children.Add(genEllipse);

            return genEllipse;
        }

        /// <summary>
        /// Initialize Kinect-related stuff.
        /// </summary>
        private void InitializeKinect()
        {
            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);
        }

        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event arguments</param>
        private static void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser.Stop();
        }

        #endregion
    }
}
