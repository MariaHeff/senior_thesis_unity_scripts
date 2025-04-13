/*
Adapted from TwistPublisher.cs in ROS# package, by Dr. Martin Bischoff
*/

using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class TwistPublisher : UnityPublisher<MessageTypes.Geometry.Twist>
    {
	// Unity environment object that Twist motion follows
        public Transform PublishedTransform;
	
	// Twist message that is published to Spot ROS Driver
        private MessageTypes.Geometry.Twist message;

	// Tracks previous linear position of tracked Unity object      
        private Vector3 previousPosition = Vector3.zero;

	// Counter to keep track of UpdateMessage calls before publishing message 
	private int numUpdateCalls;

	// Holds linear velocity value, between calls to UpdateMessage
	private Vector3 linearVelocity = new Vector3(0.0f, 0.0f, 0.0f);

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
	    numUpdateCalls = 0;
        }

        private void FixedUpdate()
        {
            UpdateMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Geometry.Twist();
            message.linear = new MessageTypes.Geometry.Vector3();
            message.angular = new MessageTypes.Geometry.Vector3();
        }

        private void UpdateMessage()
        {
	    // Resets linear velocity and angular components of message every 10 UpdateMessage calls
	    if (numUpdateCalls == 0)
	    {
		linearVelocity = new Vector3(0.0f, 0.0f, 0.0f);
		message.angular = new MessageTypes.Geometry.Vector3();
	    }
	    numUpdateCalls++;


	    // Update linear component of message, based on PublishedTransform position
	    linearVelocity = linearVelocity + ((PublishedTransform.localPosition - previousPosition) / Time.fixedDeltaTime);	   
            previousPosition = PublishedTransform.localPosition;
	    
	    // Updates angular component of message, based on controller buttons
	    if (SpotCommandPublisher.robotTurnRight) 
	    {
		message.angular.z = -1.5;
	    }
	    if (SpotCommandPublisher.robotTurnLeft) 
	    {
		message.angular.z = 1.5;

	    }
	    
	    // Send complete Twist message every ten calls (reduced linear component frequency improves Spot jerkiness)
	    if (numUpdateCalls == 10) {
		// Scaling linear velocity (adjust as wanted)
		linearVelocity = linearVelocity / 5;
	        
		// Round (reduces Spot jerkiness) and cap (Spot can not take > 2.0) linear values
		message.linear.x = System.Math.Clamp(System.Math.Round(linearVelocity.Unity2Ros().x, 1), -1.0, 1.0);
		message.linear.y = System.Math.Clamp(System.Math.Round(linearVelocity.Unity2Ros().y, 1), -1.0, 1.0);
		message.linear.z = 0;

		// Publish as long as Spot is standing and movement should not be paused
		if (!SpotCommandPublisher.robotSit & !SpotCommandPublisher.pauseMovement)
		{
           		Publish(message);
		}
	    }
	    // Every five calls, send Twist message with only angular component
	    // Spot turns better with higher frequency of angular, but jerky when linear messages sent at this rate
	    else if (numUpdateCalls % 5 == 0) {
		// Zeroes out linear component
		message.linear = new MessageTypes.Geometry.Vector3();

		// Publish as long as Spot is standing and movement should not be paused
		if (!SpotCommandPublisher.robotSit & !SpotCommandPublisher.pauseMovement)
		{
			Publish(message);
		}
	    }
	    
	    // Cycle through every 10 calls
	    if (numUpdateCalls == 10) {
		numUpdateCalls = 0;
	    }

	    

        }

    }
}
