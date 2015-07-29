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
using System.Windows.Shapes;

namespace DEEP
{
    class DEEPThermalObject : DEEPKinectObjectBaseClass
    {


        /// <summary>
        /// A constant that determines how quickly heat flows in and out of it.
        /// The thermal resistance has units of K/W, or Watts [Joules/second] 
        /// per degree Kelvin.
        /// </summary>
        public double thermalResistance = 1d;

        /// <summary>
        /// The temperature, in degrees Kelvin. Initialized to zero degrees
        /// Celsius.
        /// </summary>
        public double temperature = 273.15d;

        /// <summary>
        /// The heat capacity, measured in Joules/gram x Kelvin. Initialized
        /// to the specific heat capacity of water.
        /// </summary>
        public double heatCapacity = 4.186d;

        /// <summary>
        /// Constructor. This sets up all the object-specific properties, and
        /// is run at the beginning of the code.
        /// </summary>
        /// <param name="insulationFactor"></param>
        /// <param name="temperature"></param>
        /// <param name="heatCapacity"></param>
        /// <param name="onScreenShape"></param>
        /// <param name="backGroundRectangle"></param>
        /// <param name="isGrippable"></param>
        /// <param name="isPressable"></param>
        public DEEPThermalObject( double insulationFactor, double temperature, double heatCapacity,
                                  Ellipse onScreenShape,
                                  UIElement backGroundRectangle,
                                  bool isGrippable,
                                  bool isPressable) :
            base(onScreenShape, backGroundRectangle, isGrippable, isPressable)
        {
            this.DEEPObjectType = DEEPKinectObjectTypes.Thermal;

            this.thermalResistance = insulationFactor;
            this.temperature = temperature;
            this.heatCapacity = heatCapacity;
        }

        /// <summary>
        /// This method implements all interactions with other objects. It gets
        /// called often. For example, if you wanted to handle collisions, this
        /// would be the place to do it.
        /// </summary>
        /// <param name="anotherObject"></param>
        public override void InteractWith(DEEPKinectObjectBaseClass otherObject)
        {
            /* We only interact with other Gravitational objects. */
            if (otherObject.DEEPObjectType == DEEPKinectObjectTypes.Thermal)
            {
                /* Check for collisions. */
                if (this.IsCollidingWith(otherObject))
                {
                    /* Since the objects are touching, there is heat transfer
                     * happening. We model that here. */
                    this.ProcessHeatTransfer((DEEPThermalObject)otherObject);

                    /* Apply friction to slow parts down here. */
                    this.velocity *= 0.95;
                    otherObject.velocity *= 0.95;
                }
                else
                {
                    /* If the objects aren't touching, we assume that they are
                     * not interacting. */
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
        /// <param name="kinectUIElement"></param>
        public override void InteractWithWindowBorder(UIElement kinectUIElement)
        {
            /* If we reach a wall, we stick to it so the object doesn't disappear or fly around
             * a lot. */
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
            /* Make the colour reflect the temperature. To do so, we will
             * go between red and blue, passing through purple (blue being cold.) 
             * We will use the Kelvin scale, with 0K being the coldest, and 510K being
             * the hottest. */

            byte r = (byte)(this.temperature / 2d);
            byte g = 0;
            byte b = (byte)(255 - (this.temperature / 2d));

            /* If we try to set the colour too often, the program freezes up
             * because it is very expensive. So we only do it once in a while. */
            coloringCounter = (coloringCounter + 1) % 10;
            if (coloringCounter == 0)
            {
                onScreenShape.Dispatcher.Invoke(new Action(delegate() 
                { 
                    this.onScreenShape.Fill = new SolidColorBrush(Color.FromRgb(r, g, b)); 
                }));
            }
        }

        private int coloringCounter = 0;

        /// <summary>
        /// This method calculates how much heat energy moves from one object to the other
        /// when they are touching.
        /// </summary>
        /// <param name="otherObject"></param>
        private void ProcessHeatTransfer(DEEPThermalObject otherObject)
        {
            /* The thermal resistance is the sum of both objects' thermal resistances. */
            double r = this.thermalResistance + otherObject.thermalResistance;

            /* Calculate the temperature difference. */
            double dT = otherObject.temperature - this.temperature;

            /* Calculate the energy transferred. */
            double dE = (dT * internalRefreshRate) / r;

            /* Calculate resulting temperature changes using the specific heat capacities. */
            double dT1 = dE/this.heatCapacity;
            double dT2 = -dE/otherObject.heatCapacity;

            /* Apply the changes to the interacting objects. */
            this.temperature += dT1;
            otherObject.temperature += dT2;
        }
    }
}
