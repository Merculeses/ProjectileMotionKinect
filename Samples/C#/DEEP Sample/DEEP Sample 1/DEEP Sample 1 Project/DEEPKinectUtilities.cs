using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Kinect.Toolkit.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DEEP
{
    /// <summary>
    /// This class contains a lot of helper utilities to do various things.
    /// 
    /// THEY ARE HERE BECAUSE YOU SHOULDN'T CHANGE THEM! LEAVE THESE ALONE!
    /// 
    /// Seriously. Changing these will make lots of problems.
    /// </summary>
    public abstract partial class DEEPKinectObjectBaseClass
    {

        #region Helper Methods. These methods do useful things. DO NOT CHANGE THEM.

        /// <summary>
        /// Calculates whether this object is colliding with another one or not.
        /// </summary>
        /// <param name="otherObject">The other object to look for a collision with.</param>
        /// <returns>True if colliding, false if not.</returns>
        protected bool IsCollidingWith(DEEPKinectObjectBaseClass otherObject)
        {
            /* First, we calculate the positions of the objects on the screen. 
             * Note that because of the way things are shown on-screen, we have
             * to calculate the center location in a bit of a strange way. */

            DenseVector thisPosition = this.GetPosition() + this.velocity * internalRefreshRate;

            DenseVector otherPosition = otherObject.GetPosition() + otherObject.velocity * internalRefreshRate;

            /* Calculate the Cartesian distance between the two points. */
            double distance = (otherPosition - thisPosition).L2Norm();
            /* Here we add the velocity to the distance as well. This is a bit of a hack, 
             * but it will prevent two shapes from passing through each other if they are moving really fast. */
            //distance -= this.velocity.L2Norm() + otherObject.velocity.L2Norm();

            /* Calculate the sum of the radii. Assume that height is equal to width here. */
            double radiiSum = (this.onScreenShape.Height + otherObject.onScreenShape.Width) / 2d;

            /* Check to see if they are close enough to collide. Since we've
             * restricted objects to be round, we can do a simple check: 
             * there is a collision if the distance between the two objects is
             * less than the sum of their radii. */
            if (distance < radiiSum)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks for collisions with the edges of the UI. Bounces off if a collision
        /// is detected.
        /// </summary>
        /// <param name="kinectUIElement">The UIElement that the object is encapsulated in.</param>
        protected void BounceOffWalls(UIElement kinectUIElement)
        {
            /* Left wall. */
            if (this.GetPosition()[0] - (this.onScreenShape.Width / 2d) + (this.velocity[0] * internalRefreshRate) <= 0d)
            {
                this.velocity[0] = -this.velocity[0];
            }
            /* Right wall. */
            if (this.GetPosition()[0] + (this.onScreenShape.Width / 2d) + (this.velocity[0] * internalRefreshRate) >= kinectUIElement.RenderSize.Width)
            {
                this.velocity[0] = -this.velocity[0];
            }
            /* Top wall. */
            if (this.GetPosition()[1] - this.onScreenShape.Height / 2d + (this.velocity[1] * internalRefreshRate) <= 0d)
            {
                this.velocity[1] = -this.velocity[1];
            }
            /* Bottom wall. */
            if (this.GetPosition()[1] + this.onScreenShape.Height / 2d + (this.velocity[1] * internalRefreshRate) >= kinectUIElement.RenderSize.Height)
            {
                this.velocity[1] = -this.velocity[1];
            }
        }

        /// <summary>
        /// Checks for collisions with the edges of the UI. Sticks to the wall if a collision is
        /// detected.
        /// </summary>
        /// <param name="kinectUIElement">The UIElement that the object is encapsulated in.</param>
        protected void StickToWalls(UIElement kinectUIElement)
        {
            /* Left wall. */
            if (this.GetPosition()[0] - (this.onScreenShape.Width / 2d) + (this.velocity[0] * internalRefreshRate) <= 0d)
            {
                this.velocity[0] = 0d;
                this.velocity[1] = 0d;
            }
            /* Right wall. */
            if (this.GetPosition()[0] + (this.onScreenShape.Width / 2d) + (this.velocity[0] * internalRefreshRate) >= kinectUIElement.RenderSize.Width)
            {
                this.velocity[0] = 0d;
                this.velocity[1] = 0d;
            }
            /* Top wall. */
            if (this.GetPosition()[1] - this.onScreenShape.Height / 2d + (this.velocity[1] * internalRefreshRate) <= 0d)
            {
                this.velocity[0] = 0d;
                this.velocity[1] = 0d;
            }
            /* Bottom wall. */
            if (this.GetPosition()[1] + this.onScreenShape.Height / 2d + (this.velocity[1] * internalRefreshRate) >= kinectUIElement.RenderSize.Height)
            {
                this.velocity[0] = 0d;
                this.velocity[1] = 0d;
            }
        }

        /// <summary>
        /// Calculates the resulting velocities when colliding in a perfectly
        /// elastic manner with another object.
        /// </summary>
        /// <param name="otherObject">The other object that this one is colliding with.</param>
        protected void ProcessElasticCollisionWith(DEEPKinectObjectBaseClass otherObject)
        {
            /* We use the definition from Wikipedia for how to handle the collision of
             * two moving objects in vector form. See: en.wikipedia.org/wiki/Elastic_collision
             * 
             * To be specific, we use instructions from www.vobarian.com/collisions/2dcollisions2.pdf */

            /* Here are the known quanitities of the situation. */
            DenseVector p1 = this.GetPosition();
            DenseVector p2 = otherObject.GetPosition();

            DenseVector v1 = this.velocity;
            DenseVector v2 = otherObject.velocity;

            double m1 = this.mass;
            double m2 = otherObject.mass;

            /* First we create a unit normal and tangent vector. */
            DenseVector n = p2 - p1;
            DenseVector un = n / n.Norm(2d);
            DenseVector ut = new DenseVector(new double[] { -un[1], un[0] });

            /* Here we find the normal and tangential components of the velocities. */
            double v1n = un.DotProduct(v1);
            double v1t = ut.DotProduct(v1);

            double v2n = un.DotProduct(v2);
            double v2t = ut.DotProduct(v2);

            /* We then apply 1-D elastic collision dynamics in the normal direction to the
             * line of collision.
             * Note that there is NO CHANGE in the tangential components of the velocity. */
            double post_v1n = (v1n * (m1 - m2) + 2 * m2 * v2n) / (m1 + m2);
            double post_v2n = (v2n * (m2 - m1) + 2 * m1 * v1n) / (m1 + m2);

            /* Now we convert the scalar normal/tangential velocities into vectors pointing
             * in the appropriate directions. */
            DenseVector vPost_v1n = post_v1n * un;
            DenseVector vPost_v1t = v1t * ut;

            DenseVector vPost_v2n = post_v2n * un;
            DenseVector vPost_v2t = v2t * ut;

            /* Calculate the post-collision velocity by adding the normal/tangential velocities
             * together. */
            DenseVector v1FinalVelocity = vPost_v1n + vPost_v1t;
            DenseVector v2FinalVelocity = vPost_v2n + vPost_v2t;

            /* Set the object's velocity to the post-collision velocity. */
            this.velocity = v1FinalVelocity;
            otherObject.velocity = v2FinalVelocity;
        }

        /// <summary>
        /// Calculates the resulting velocities of both objects when undergoing a perfectly
        /// inelastic collision with another object.
        /// </summary>
        /// <param name="otherObject"></param>
        protected void ProcessInelasticCollisionWith(DEEPKinectObjectBaseClass otherObject)
        {
            /* Here are the known quanitities of the situation. */
            DenseVector p1 = this.GetPosition();
            DenseVector p2 = otherObject.GetPosition();

            DenseVector v1 = this.velocity;
            DenseVector v2 = otherObject.velocity;

            double m1 = this.mass;
            double m2 = otherObject.mass;

            DenseVector vFinal = new DenseVector(2);
            vFinal[0] = (m1 * v1[0] + m2 * v2[0]) / (m1 + m2);
            vFinal[1] = (m1 * v2[1] + m2 * v2[1]) / (m1 + m2);

            this.velocity = vFinal;
            otherObject.velocity = vFinal;
        }

        /// <summary>
        /// Provides the [X,Y] coordinate of this object on the screen in DenseVector form.
        /// </summary>
        /// <returns>A 2-element vector containing the [X,Y] position of this object.</returns>
        protected DenseVector GetPosition()
        {
            DenseVector location = new DenseVector(2);

            /* X coordinate */
            location[0] = this.onScreenShapePosition.X + onScreenShape.Width / 2d;
            location[1] = this.onScreenShapePosition.Y + onScreenShape.Height / 2d;

            return location;
        }

        /// <summary>
        /// Set the position of the on-screen shape in the window by adjusting the margin Property.
        /// </summary>
        /// <param name="setPoint">The point (referenced from the top left corner of the parent element) 
        /// at which the on-screen shape should be placed.</param>
        protected void SetShapeScreenPosition(Point setPoint)
        {
            try
            {
                Thickness margin = new Thickness();
                margin.Left = setPoint.X;
                margin.Top = setPoint.Y;

                onScreenShape.Dispatcher.Invoke(new Action(delegate() { onScreenShape.Margin = margin; }));
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Moves the object to the place under the current position of the hand pointer.
        /// This function is useful when moving an object that has been gripped.
        /// </summary>
        /// <param name="senderEllipse"></param>
        /// <param name="args"></param>
        private void PullGrippedObject(HandPointerEventArgs args)
        {
            /* Find out where the hand is. */
            Point handPoint = args.HandPointer.GetPosition(null);
            DenseVector handPosition = new DenseVector(new double[] { handPoint.X, handPoint.Y });

            /* Find out where the object is on the screen. */
            DenseVector objectPosition = this.GetPosition();

            /* "Pull" the object toward the grip handle. */
            DenseVector pullDirection = (handPosition - objectPosition);
            pullDirection = pullDirection * 0.0001 * pullDirection.Norm(2d);

            this.velocity = this.velocity + pullDirection;
        }

        #endregion

    }
}
