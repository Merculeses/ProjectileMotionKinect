using Microsoft.Kinect.Toolkit.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DEEP
{
    class DEEPButton : DEEPKinectObjectBaseClass
    {
        /// <summary>
        /// Delegate method as a prototype for pressed/released events.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="args">Kinect hand pointer parameters (location, etc.)</param>
        public delegate void ButtonEvent(object sender, HandPointerEventArgs args);

        /// <summary>
        /// An event that is broadcast by this object to indicate the button was
        /// just pressed.
        /// </summary>
        public event ButtonEvent ButtonPressed;

        /// <summary>
        /// An event that is broadcast by this object to indicate the button was
        /// just released.
        /// </summary>
        public event ButtonEvent ButtonReleased;

        /// <summary>
        /// Constructor. This sets up all the object-specific properties, and
        /// is run at the beginning of the code.
        /// </summary>
        /// <param name="onScreenShape"></param>
        /// <param name="backGroundRectangle"></param>
        /// <param name="isGrippable"></param>
        /// <param name="isPressable"></param>
        public DEEPButton(Ellipse onScreenShape,
                          UIElement backGroundRectangle) :
            base(onScreenShape, backGroundRectangle, false, true)
        {
            this.DEEPObjectType = DEEPKinectObjectTypes.Button;

            /* Set the button appearance. */
            SetVisibleButtonUp();
        }

        /// <summary>
        /// Handle when the object is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnPressHandler(object sender, HandPointerEventArgs args)
        {
            this.isPressed = true;

            /* Change the button colour! */
            SetVisibleButtonDown();
            //this.onScreenShape.Fill = new SolidColorBrush(Colors.Blue);

            /* Notify on the appropriate event. */
            ButtonPressed(sender, args);
        }

        /// <summary>
        /// Handles when the user releases the press on the on-screen shape.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnPressReleaseHandler(object sender, HandPointerEventArgs args)
        {
            this.isPressed = false;

            /* Change the button colour! */
            SetVisibleButtonUp();
            //this.onScreenShape.Fill = new SolidColorBrush(Colors.Green);

            /* Notify on the appropriate event. */
            ButtonReleased(sender, args);
        }

        #region Helper Functions.

        /// <summary>
        /// Makes the button look big and blue, and up.
        /// </summary>
        private void SetVisibleButtonUp()
        {
            ScaleTransform buttonScaleXform = new ScaleTransform();
            buttonScaleXform.ScaleX = 2.4;
            buttonScaleXform.ScaleY = 2.4;

            TranslateTransform buttonTranslateXForm = new TranslateTransform();
            buttonTranslateXForm.X = -0.1;
            buttonTranslateXForm.Y = -0.7;

            TransformGroup buttonTransformGroup = new TransformGroup();
            buttonTransformGroup.Children.Add(buttonScaleXform);
            buttonTransformGroup.Children.Add(buttonTranslateXForm);

            this.onScreenShape.Fill.RelativeTransform = buttonTransformGroup; 
        }

        /// <summary>
        /// Makes the button look big and blue, and down.
        /// </summary>
        private void SetVisibleButtonDown()
        {
            ScaleTransform buttonScaleXform = new ScaleTransform();
            buttonScaleXform.ScaleX = 2.4;
            buttonScaleXform.ScaleY = 2.4;

            TranslateTransform buttonTranslateXForm = new TranslateTransform();
            buttonTranslateXForm.X = -1.305;
            buttonTranslateXForm.Y = -0.7;

            TransformGroup buttonTransformGroup = new TransformGroup();
            buttonTransformGroup.Children.Add(buttonScaleXform);
            buttonTransformGroup.Children.Add(buttonTranslateXForm);

            this.onScreenShape.Fill.RelativeTransform = buttonTransformGroup; 
        }

        #endregion

        #region For a button, all these other things are kind of useless. Ignore these!

        /// <summary>
        /// This method implements all interactions with other objects. It gets
        /// called often. For example, if you wanted to handle collisions, this
        /// would be the place to do it.
        /// </summary>
        /// <param name="anotherObject"></param>
        public override void InteractWith(DEEPKinectObjectBaseClass anotherObject)
        {
            /* No interactions! It's a button! */
        }

        /// <summary>
        /// This implements any special interactions with the hand pointer.
        /// For example, if the hand pointer was to repel or attract this object,
        /// the code to implement that would go here.
        /// </summary>
        /// <param name="pointer"></param>
        public override void InteractWithHandPointer(HandPointer pointer)
        {
            /* Also no interactions with a hand pointer. We'll handle the
             * button press separately. */
        }

        /// <summary>
        /// Here we do things if we want to interact with the UI window border. For
        /// example, if we'd like to bounce off the border, that code would go here.
        /// </summary>
        /// <param name="kinectRegionGrid"></param>
        public override void InteractWithWindowBorder(UIElement kinectRegionGrid)
        {
            /* No interaction with window border, because it can't move. */
        }

        /// <summary>
        /// Implements all kinematic behaviour. This method is called many times per second to
        /// make updates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void ExecuteInternalBehaviours(object sender, ElapsedEventArgs e)
        {
            /* No internal behaviours. */
        }

        #endregion
    }
}
