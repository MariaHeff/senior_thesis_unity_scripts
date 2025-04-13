These two scripts allow the handheld controllers to send services to the Spot ROS2 Driver and headset movement to make Spot move.

Download ROS# from https://github.com/siemens/ros-sharp and "Add package from disk..." option in Unity Package Manager.
Add these two files to the \ros-sharp-master\com.siemens.ros-sharp\Runtime\RosBridgeClient\RosCommunication folder in the ROS# library.
Create GameObject and add the SpotCommandPublisher; create GameObject and add the TwistPublisher that publishes to /spot1/cmd_vel and takes the Main Camera as the PublishedTransform
