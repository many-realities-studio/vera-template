In-Game Debug Log README

OVERVIEW:
The in-game debug log provides an in-game display which mimics the behavior 
of Unity's built in debug console, but is visible in-game during runtime. 
The log provides an easy way to view any console logs, warnings, and errors 
which occur during runtime, with the option of viewing the stack trace.

SETUP AND USE:
- Drag the "InGameDebugLog" prefab into your scene at a desired world location.
- During runtime, the log will now display all debug logs, warnings, and errors.

ADDITIONAL USAGE:
- If you wish for the log to follow the camera, place the object as a child 
	component of your camera.
- If you wish for the log to be interactable in VR, attach the corresponding VR
	component to the log (such as TrackedDeviceGraphicsRaycaster for Unity's XR
	Toolkit).
- If you wish to show or hide the log via an external script, there is an "InGameDebugLog.cs" 
	script which is attached to the log game object; call the "ShowWindow()" and "HideWindow()" 
	functions of this script to show or hide the log.