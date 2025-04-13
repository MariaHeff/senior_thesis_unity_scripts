using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Std;


public class SpotCommandPublisher : MonoBehaviour
{
    public RosConnector rosConnector;

    // Used in TwistPublisher.cs, keeps track of Spot's state/ordered movement
    public static bool robotSit;
    public static bool robotTurnRight;
    public static bool robotTurnLeft;
    public static bool pauseMovement;
   
    // Keep track of Spot's state, allows one button to have toggling between opposite commands
    private bool gripperOpen;
    private bool armOut;

    // Tracks whether current button press has been registered already
    private bool prevAButtonPressed;
    private bool prevBButtonPressed;
    private bool prevRightTrigPressed;

    
    // Called once, before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
	robotSit = false;
	gripperOpen = false;
	armOut = false;
	pauseMovement = false;
	
	prevAButtonPressed = false;
	prevBButtonPressed = false;
	prevRightTrigPressed = false;
    }

    // Called once per frame
    void Update()
    {
	// Registered right and left hand controllers
        List<InputDevice> rightDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, rightDevices);
	List<InputDevice> leftDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left, leftDevices);

	// Setting button states
        bool aButtonPressed = false;
        bool bButtonPressed = false;
	bool rightTrigger = false;
	bool xButtonPressed = false;
        bool yButtonPressed = false;
	bool leftTrigger = false;
        
        foreach (var device in rightDevices)
        {
		device.TryGetFeatureValue(CommonUsages.primaryButton, out aButtonPressed);
		device.TryGetFeatureValue(CommonUsages.secondaryButton, out bButtonPressed);
		device.TryGetFeatureValue(CommonUsages.triggerButton, out rightTrigger);


        }
	foreach (var device in leftDevices)
        {
		device.TryGetFeatureValue(CommonUsages.primaryButton, out xButtonPressed);
		device.TryGetFeatureValue(CommonUsages.secondaryButton, out yButtonPressed);
		device.TryGetFeatureValue(CommonUsages.triggerButton, out leftTrigger);

        }

	// if A button is pressed, Spot sits/stands (registers once per press)       
        if (aButtonPressed && !prevAButtonPressed)
        {
		if (robotSit)
		{
			CallService("/spot1/stand");
			robotSit = false;
		}
		else 
		{
			CallService("/spot1/sit");
			robotSit = true;
		}
		
        }

	// If B button is pressed, Spot puts arm out/pulls arm in        
        if (bButtonPressed && !prevBButtonPressed)
        {
		if (armOut) 
		{
			CallService("/spot1/arm_stow");
			armOut = false;
		}
		else
		{
			CallService("/spot1/arm_unstow");
			armOut = true;
		}

        }

	// If right trigger pressed, opens/closes gripper
	if (rightTrigger && !prevRightTrigPressed)
        {
		if (gripperOpen)
		{
			CallService("/spot1/close_gripper");
			gripperOpen = false;
		}
		else 
		{
			CallService("/spot1/open_gripper");
			gripperOpen = true;
		}
        }

	// For duration of X press, turn Spot left (command sent in TwistPublisher)
	if (xButtonPressed)
        {
		robotTurnLeft = true;
        }
	else
	{
		robotTurnLeft = false;
	}

        // For duration of Y press, turn Spot right (command sent in TwistPublisher)
        if (yButtonPressed)
        {
		robotTurnRight = true;
        }
	else
	{
		robotTurnRight = false;
	}

	// For duration of left trigger press, Spot does not move (effect in TwistPublisher)
	if (leftTrigger)
        {
		pauseMovement = true;
        }
	else 
	{
		pauseMovement = false;
	}

	// Sets the "button previously pressed" booleans for right hand buttons
	prevAButtonPressed = aButtonPressed;
	prevBButtonPressed = bButtonPressed;
	prevRightTrigPressed = rightTrigger;

    }

    private void CallService(string serviceName)
    {
	// TriggerRequest in RosSharp.RosBridgeClient.MessageTypes.ROS2.Std.srv, which uses namespace RosSharp.RosBridgeClient.MessageTypes.Std
        var serviceRequest = new TriggerRequest();
	
	// Calling CallService from Libraries/RosBridge/Client/RosSocket.cs
	rosConnector.RosSocket.CallService<TriggerRequest, TriggerResponse>(serviceName, response => Debug.Log(serviceName + " Response: " + response.success), serviceRequest);
        
    }
}

