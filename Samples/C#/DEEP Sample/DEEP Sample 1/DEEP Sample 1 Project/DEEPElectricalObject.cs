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
    class DEEPElectricalObject : DEEPKinectObjectBaseClass
    {
        /// <summary>
        /// Electrical charge. Units are elementary charge units (1.6 x 10^-19 Coulomb.)
        /// </summary>
        double charge = 0;

        /// <summary>
        /// Coloumb's constant.
        /// </summary>
        const double k_e = 8.987551787e6;
        //const double k_e = 8.987551787e9;

        /// <summary>
        /// Constructor. This sets up all the object-specific properties, and
        /// is run at the beginning of the code.
        /// </summary>
        /// <param name="onScreenShape"></param>
        /// <param name="backGroundRectangle"></param>
        /// <param name="isGrippable"></param>
        /// <param name="isPressable"></param>
        public DEEPElectricalObject( double charge,
                                     double mass,
                                     Ellipse onScreenShape,
                                     UIElement backGroundRectangle,
                                     bool isGrippable,
                                     bool isPressable) :
            base(onScreenShape, backGroundRectangle, isGrippable, isPressable)
        {
            this.DEEPObjectType = DEEPKinectObjectTypes.Electrical;

            this.charge = charge;
            this.mass = mass;
        }

        /// <summary>
        /// This method implements all interactions with other objects. It gets
        /// called often. For example, if you wanted to handle collisions, this
        /// would be the place to do it.
        /// </summary>
        /// <param name="anotherObject"></param>
        public override void InteractWith(DEEPKinectObjectBaseClass otherObject)
        {
            /* We only interact with other Electrical objects. */
            if (otherObject.DEEPObjectType == DEEPKinectObjectTypes.Electrical)
            {
                /* Check for collisions. */
                if (this.IsCollidingWith(otherObject))
                {
                    /* If collided, we do so inelastically. */
                    this.ProcessInelasticCollisionWith(otherObject);
                }
                else
                {
                    /* Otherwise, apply electrostatics. */
                    this.ProcessElectricalInteraction((DEEPElectricalObject)otherObject);
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

        }

        /// <summary>
        /// Here we do things if we want to interact with the UI window border. For
        /// example, if we'd like to bounce off the border, that code would go here.
        /// </summary>
        /// <param name="kinectRegionGrid"></param>
        public override void InteractWithWindowBorder(UIElement kinectUIElement)
        {
            /* Just for the sake of the demo, we'll have charges stick to the outside
             * walls if they get flung about. */
            StickToWalls(kinectUIElement);
        }

        /// <summary>
        /// Implements all kinematic behaviour. This method is called many times per second to
        /// make updates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void ExecuteInternalBehaviours(object sender, ElapsedEventArgs e)
        {

        }

        /// <summary>
        /// Handle when the object is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnPressHandler(object sender, HandPointerEventArgs args)
        {

        }

        /// <summary>
        /// Handle the interaction of charged particles.
        /// </summary>
        /// <param name="dEEPGravitationalObject"></param>
        private void ProcessElectricalInteraction(DEEPElectricalObject otherObject)
        {
            /* Calculate the gravitational force. */

            double m1 = this.mass;
            double m2 = otherObject.mass;

            double q1 = this.charge;
            double q2 = otherObject.charge;

            DenseVector p1 = this.GetPosition();
            DenseVector p2 = otherObject.GetPosition();

            /* Calculate the Cartesian distance between the two points. */
            double r = (p2 - p1).L2Norm();

            double F = (k_e * q1 * q2) / Math.Pow(r, 2d);

            /* Calculate the acceleration magnitude. */
            double a1 = F / m1;
            double a2 = F / m2;

            /* Multiply by direction unit vectors in the respective directions. */
            DenseVector n1 = p1 - p2;
            DenseVector un1 = n1 / n1.Norm(2d);
            DenseVector accel1 = a1 * un1;

            DenseVector n2 = p2 - p1;
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
