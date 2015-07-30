using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Kinect.Toolkit.Controls;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DEEP
{
    class DEEPKinematicObject : DEEPKinectObjectBaseClass
    {
        #region State variables that describe the object, and its' properties as a kinematic thing.

        /// <summary>
        /// Indicates the coefficient of friction when moving. Value should be in [0,1].
        /// If you make this number negative, the system will spontaneously gain energy
        /// and explode. Could be kind of fun...? :D
        /// </summary>
        private double coefficientOfFriction = 0.010d;

        #endregion

        /// <summary>
        /// Constructor. This is automatically run when you make a new
        /// DEEPKinematicObject(). You should use this method to do any
        /// setup. For example, you should set the object's mass here.
        /// </summary>
        /// <param name="onScreenShape"></param>
        /// <param name="isGrippable"></param>
        /// <param name="isPressable"></param>
        public DEEPKinematicObject( double setMassTo,
                                    Ellipse onScreenShape,
                                    UIElement backGroundRectangle,
                                    bool isGrippable,
                                    bool isPressable) :
            base(onScreenShape, backGroundRectangle, isGrippable, isPressable)
        {
            /* First, we declare this to be a Kinematic object, so other objects
             * will know if they check. */
            this.DEEPObjectType = DEEPKinectObjectTypes.Kinematic;

            /* Next, we set the mass. */
            this.mass = setMassTo;
        }

        /// <summary>
        /// Because we're kinematic, we'll bounce off the walls like an air hockey table.
        /// </summary>
        /// <param name="kinectUIElement"></param>
        public override void InteractWithWindowBorder(UIElement kinectUIElement)
        {
            /* Check to see if we've collided with each of the 4 walls. */
            BounceOffWalls(kinectUIElement);
        }

        /// <summary>
        /// Defines non-click/grip interactions with any hand pointers on the screen.
        /// </summary>
        /// <param name="pointer"></param>
        public override void InteractWithHandPointer(HandPointer pointer)
        {
            /* For now, we don't do anything here. */
        }

        public override void InteractWith(DEEPKinectObjectBaseClass otherObject)
        {
            /* The only interaction that a Kinematic object has with other objects
             * is collision with other kinematic objects. So we will first check
             * if the other object is also kinematic. */
            if (otherObject.DEEPObjectType == DEEPKinectObjectTypes.Kinematic)
            {
                if (this.IsCollidingWith(otherObject) )
                {
                    /* A collision affects both objects that collide. So this interaction
                     * will adjust the velocities of both of the involved objects. */
                    ProcessElasticCollisionWith((DEEPKinematicObject)otherObject);
                }
            }
        }

        /// <summary>
        /// Implements all kinematic behaviour. This method is called many times per second to
        /// make updates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void ExecuteInternalBehaviours(object sender, ElapsedEventArgs e)
        {
            /* Since this object is subject to sliding friction, we slow it down here. */
            
            /* Dear DEEP Student: This model of friction is WRONG WRONG WRONG. But it
             * seems to simulate friction quite well, doesn't it? If you'd like, you
             * can improve it to be accurate from a scientific standpoint. :) */
            this.velocity[0] *= (1 - coefficientOfFriction);
            this.velocity[1] *= (1 - coefficientOfFriction);
            //distanceX, distanceY, VelocityX, VelocityY, acceleration, time
            if (!isGripped)
            {
                this.velocity[0] += 0;
                this.velocity[1] += 9.81d * 1;
            }
            //Console.WriteLine(isGripped);

            //this.onScreenShapePosition.X += this.velocity[0] * 1;
            //this.onScreenShapePosition.Y += this.velocity[1] * 1;
        }
    }
}
