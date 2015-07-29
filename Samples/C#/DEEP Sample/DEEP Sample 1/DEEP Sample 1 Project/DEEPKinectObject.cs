using Microsoft.Kinect.Toolkit.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Timers;
using System.Windows.Threading;
using System.Threading;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Windows.Controls;

namespace DEEP
{
    public enum DEEPKinectObjectTypes
    {
        Gravitational,      /* This object is subject to Newton's Universal Law of Gravitation. */
        Thermal,            /* This object has a heat capacity and transfers heat to other objects. */
        Electrical,         /* This object behaves like a charged particle. */
        Kinematic,          /* This object behaves according to the rules of kinematics. It can be subject
                             * to friction, bounce off other things and has momentum/kinetic energy. */
        Button              /* Only accepts presses. Can be used to trigger other actions. */
    };

    /// <summary>
    /// A helper class to take care of a lot of what's required when creating GUI objects that
    /// interact with the Kinect hand pointer. This class attaches to a Shape object, and provides
    /// it with the means to become Kinect-enabled, physics enabled and whatever else you can
    /// think of!
    /// </summary>
    public abstract partial class DEEPKinectObjectBaseClass
    {
        /// <summary>
        /// Identifies this object as being of a particular type.
        /// </summary>
        public DEEPKinectObjectTypes DEEPObjectType;

        #region Common physics-related variables.

        /// <summary>
        /// Represents the object's velocity in 2-D space. Units in pixels per second.
        /// </summary>
        public DenseVector velocity = new DenseVector(2);

        /// <summary>
        /// The location of the on-screen circle on the UI, as an [x,y] coordinate.
        /// </summary>
        public Point onScreenShapePosition;

        /// <summary>
        /// The object's mass, in kilograms.
        /// </summary>
        public double mass = 1d;

        /// <summary>
        /// Sets the rate at which the internal timer goes off to take care of things
        /// like updating position, physics, etc. The rate is expressed in seconds, or
        /// 1/Hz.
        /// </summary>
        public const double internalRefreshRate = (1d / 100d);

        /// <summary>
        /// Used to enforce the ratio between calculation updates and UI refreshes.
        /// </summary>
        public int screenUpdateSkipCounter;


        

        #endregion

        /// <summary>
        /// This is a reference to the object that is on the UI. We have to associate the
        /// DEEPKinectObject with a shape we put on the UI. Note that this can only be
        /// an ellipse that happens to be circular -- the height and width should be
        /// equal.
        /// </summary>
        public Ellipse onScreenShape;

        /// <summary>
        /// This state variable keeps track of whether the object is currently grabbed by
        /// the user.
        /// </summary>
        protected bool isGripped = false;
        /// <summary>
        /// This state variable keeps track of whether the object is currently pressed by
        /// the user.
        /// </summary>
        protected bool isPressed = false;

        /// <summary>
        /// This timer is used to execute any pending animations, kinematics etc.
        /// </summary>
        protected System.Timers.Timer internalTimer;

        /// <summary>
        /// Constructor. Creates a new DEEPKinectObject and binds it to an existing
        /// Shape object that is on the GUI.
        /// </summary>
        /// <param name="onScreenShape">The Shape that is on the GUI. We need this to bind the
        /// DEEPKinectObject to the shape on the screen.</param>
        /// <param name="isGrippable">Indicates whether the user can grip this object using a Kinect hand pointer.</param>
        /// <param name="isPressable">Indicates whether the user can press this object using a Kinect hand pointer.</param>
        public DEEPKinectObjectBaseClass(System.Windows.Shapes.Ellipse onScreenShape,
                                UIElement backGroundRectangle,
                                bool isGrippable,
                                bool isPressable)
        {
            //Get a reference to the on-screen shape, so we can manipulate it later.
            this.onScreenShape = onScreenShape;

            /* Also get a starting location for the physical simulation. */
            this.onScreenShapePosition.X = onScreenShape.Margin.Left;
            this.onScreenShapePosition.Y = onScreenShape.Margin.Top;

            //Make sure the onscreen shape is grippable and touchable if the caller desires it.
            KinectRegion.SetIsGripTarget(onScreenShape, isGrippable);
            KinectRegion.SetIsPressTarget(onScreenShape, isPressable);

            //Add handlers so that we can do things when the shape is touched or gripped on screen.
            if (isGrippable)
            {
                KinectRegion.AddHandPointerGripHandler(onScreenShape, OnGripHandler);
                KinectRegion.AddHandPointerGripReleaseHandler(onScreenShape, OnGripReleaseHandler);
                KinectRegion.AddHandPointerGripReleaseHandler(backGroundRectangle, OnGripReleaseHandler);
            }
            if (isPressable)
            {
                KinectRegion.AddHandPointerPressHandler(onScreenShape, OnPressHandler);
                KinectRegion.AddHandPointerPressReleaseHandler(onScreenShape, OnPressReleaseHandler);
            }

            KinectRegion.AddHandPointerMoveHandler(backGroundRectangle, OnHandPointerMoveHandler);
            KinectRegion.AddHandPointerMoveHandler(onScreenShape, OnHandPointerMoveHandler);

            //Here, we initialize the animationTimer, which we will later use for effects.
            internalTimer = new System.Timers.Timer(internalRefreshRate * 1000d);
            internalTimer.Elapsed += InternalTimer_Elapsed;
            internalTimer.Start();
        }

        /// <summary>
        /// Destructor. We clean up here.
        /// </summary>
        ~DEEPKinectObjectBaseClass()
        {
            internalTimer.Stop();
        }

        /// <summary>
        /// This template governs how this object interacts with another object. In particular,
        /// it governs how this object's internal states are modified by the presence of another
        /// object. For example, if this object has mass and is placed in the presence of another
        /// object with mass, it will be attracted to the other object.
        /// 
        /// Note that this interaction is one-way only: the interaction shows how this object is
        /// changed by the presence of the other, but not the other way around. To also change
        /// the other object, it is necessary to call its' InteractWith() method.
        /// </summary>
        /// <param name="anotherObject">The other object that this object is being exposed to.</param>
        public abstract void InteractWith(DEEPKinectObjectBaseClass anotherObject);

        /// <summary>
        /// Defines how the object interacts with a hand. This interaction does not include gripping
        /// and clicking, which are handled separately. However, things like how this object behaves
        /// in proximity with the hand pointer, for example, are implemented here.
        /// </summary>
        /// <param name="pointer">Hand pointer information.</param>
        public abstract void InteractWithHandPointer(HandPointer pointer);

        /// <summary>
        /// This method handles what happens when the object touches one of the borders of the 
        /// UI window.
        /// </summary>
        /// <param name="kinectRegionGrid"></param>
        public abstract void InteractWithWindowBorder(UIElement kinectRegionGrid);

        /// <summary>
        /// This method is automatically called many times per second, as part of the DEEPKinectObject's
        /// internal updating routine. You can put behaviours in here, like friction, gravity or other
        /// effects that are specific to your sub-type of DEEPKinectObject().
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected abstract void ExecuteInternalBehaviours(object sender, ElapsedEventArgs e);

        /// <summary>
        /// This method goes off many times per second when the
        /// animationTimer is enabled. You can use this to implement things like inertia,
        /// gravity or other effects.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void InternalTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
           //Calculate the new position of the object on the screen based on the velocity.
           try
           {
               /* Update the position of the circle on-screen with the velocity. */
               onScreenShapePosition.X += velocity[0] * internalRefreshRate;
               onScreenShapePosition.Y += velocity[1] * internalRefreshRate;

               /* Now we call the abstract method that the programmer will implement
                * for each different type of sub-class. This function will implement
                * all dynamic behaviour that is type-specific. For example, friction
                * may affect a kinematic object type. */
               this.ExecuteInternalBehaviours(sender, e);

               /* We don't really need to run this very fast at all. So let's skip it now
                * and again. */
               screenUpdateSkipCounter = (screenUpdateSkipCounter + 1) % 3;
               if (screenUpdateSkipCounter == 0)
               {
                    SetShapeScreenPosition(onScreenShapePosition);
               }
           }
           catch (Exception ex)
           {
               /* Don't do anything about errors. */
           }
        }

        #region Event Handlers. These methods are called when stuff happens to your object (touched, gripped, etc.)

        /// <summary>
        /// Handles when the hand pointer moves.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void OnHandPointerMoveHandler(object sender, HandPointerEventArgs args)
        {
            //If the object is grabbed, move it with the hand pointer.
            if (this.isGripped == true)
            {
                PullGrippedObject(args);
            }

            //Here, we can do things if the object is pressed.
            if (this.isPressed == true)
            {

            }
        }

        /// <summary>
        /// Handles when the shape on the screen is gripped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void OnGripHandler(object sender, HandPointerEventArgs args)
        {
            this.isGripped = true;
        }

        /// <summary>
        /// Handles when the user releases his/her grip on the on-screen shape.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void OnGripReleaseHandler(object sender, HandPointerEventArgs args)
        {
            this.isGripped = false;
        }

        /// <summary>
        /// Handles when the user presses the on-screen shape.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void OnPressHandler(object sender, HandPointerEventArgs args)
        {
            this.isPressed = true;
        }

        /// <summary>
        /// Handles when the user releases the press on the on-screen shape.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void OnPressReleaseHandler(object sender, HandPointerEventArgs args)
        {
            this.isPressed = false;
        }

        #endregion
    }
}
