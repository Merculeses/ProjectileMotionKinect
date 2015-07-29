using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Kinect.Toolkit.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace DEEP
{
    /// <summary>
    /// This class defines an object that behaves as if subject to
    /// Newton's Universal Law of Gravitation.
    /// </summary>
    class DEEPGravitationalObject : DEEPKinectObjectBaseClass
    {        
        /// <summary>
        /// The gravitational constant G. This is the real deal, though it might make more sense
        /// to make it a fake number for the sake of illustrating the effect to scale.
        /// </summary>
        public const double G = 6.673e-11;

        /// <summary>
        /// Constructor. This sets up all the object-specific properties, and
        /// is run at the beginning of the code.
        /// </summary>
        /// <param name="mass">The mass of the object, in kg.</param>
        /// <param name="onScreenShape">The UI object to pair this object with, so the user can see it.</param>
        /// <param name="backGroundRectangle"></param>
        /// <param name="isGrippable"></param>
        /// <param name="isPressable"></param>
        public DEEPGravitationalObject( double mass,
                                        DenseVector startingVelocity,
                                        Ellipse onScreenShape,
                                        UIElement backGroundRectangle,
                                        bool isGrippable,
                                        bool isPressable) :
            base(onScreenShape, backGroundRectangle, isGrippable, isPressable)
        {
            this.DEEPObjectType = DEEPKinectObjectTypes.Gravitational;

            this.mass = mass;
            this.velocity = startingVelocity;
        }

        /// <summary>
        /// This method implements all interactions with other objects. It gets
        /// called often. For example, if you wanted to handle collisions, this
        /// would be the place to do it.
        /// </summary>
        /// <param name="otherObject"></param>
        public override void InteractWith(DEEPKinectObjectBaseClass otherObject)
        {
            /* We only interact with other Gravitational objects. */
            if (otherObject.DEEPObjectType == DEEPKinectObjectTypes.Gravitational)
            {
                /* Check for collisions. */
                if (this.IsCollidingWith(otherObject))
                {
                    /* If collided, we do so inelastically. */
                    this.ProcessInelasticCollisionWith(otherObject);
                }
                else
                {
                    /* Otherwise, apply gravity. */
                    this.ProcessGravitationalInteraction((DEEPGravitationalObject)otherObject);
                }
            }
        }

        /// <summary>
        /// This implements any special interactions with the hand pointer.
        /// For example, if the hand pointer was to repel or attract this object,
        /// the code to implement that would go here.
        /// </summary>
        /// <param name="pointer"></param>
        public override void InteractWithHandPointer(HandPointer pointer)
        {
            /* For now, we won't have the hand pointer do anything cool, so we'll
             * leave this blank. */
        }

        /// <summary>
        /// Here we do things if we want to interact with the UI window border. For
        /// example, if we'd like to bounce off the border, that code would go here.
        /// </summary>
        /// <param name="kinectRegionGrid"></param>
        public override void InteractWithWindowBorder(UIElement kinectUIElement)
        {
            /* Just for the sake of the demo, we'll have the planets bounce off the UI border.
             * Watch out! This system is likely to accumulate energy. O.o */
            BounceOffWalls(kinectUIElement);
        }

        /// <summary>
        /// Implements all kinematic behaviour. This method is called many times per second to
        /// make updates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void ExecuteInternalBehaviours(object sender, ElapsedEventArgs e)
        {
            /* There are no state changes like friction, or mass loss, so we will leave this
             * method empty. */
        }

        /// <summary>
        /// Processes the interaction of two bodies due to Newton's Laws of 
        /// Universal Gravitation. Adjusts both bodies' velocities.
        /// </summary>
        /// <param name="otherObject">The other object to interact with.</param>
        private void ProcessGravitationalInteraction(DEEPGravitationalObject otherObject)
        {
            /* Calculate the gravitational force. */

            double m1 = this.mass;
            double m2 = otherObject.mass;

            DenseVector p1 = this.GetPosition();
            DenseVector p2 = otherObject.GetPosition();

            /* Calculate the Cartesian distance between the two points. */
            double r = (p2 - p1).L2Norm();

            double F = (G * m1 * m2) / Math.Pow(r, 2d);

            /* Calculate the acceleration magnitude. */
            double a1 = F / m1;
            double a2 = F / m2;

            /* Multiply by direction unit vectors in the respective directions. */
            DenseVector n1 = p2 - p1;
            DenseVector un1 = n1 / n1.Norm(2d);
            DenseVector accel1 = a1 * un1;

            DenseVector n2 = p1 - p2;
            DenseVector un2 = n2 / n2.Norm(2d);
            DenseVector accel2 = a2 * un2;

            /* Apply the acceleration over one timestep to get the change in velocity. */
            DenseVector deltaV1 = accel1 * internalRefreshRate;
            DenseVector deltaV2 = accel2 * internalRefreshRate;

            /* Apply the velocity change to the objects. */
            this.velocity += deltaV1;
            otherObject.velocity += deltaV2;
        }
    }
}
