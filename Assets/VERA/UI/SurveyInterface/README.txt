Survey Interface README

OVERVIEW:
The VERA survey interface allows you to display surveys within the study application, 
taken directly from survey data which you have uploaded to the web interface.

Once you have uploaded a survey through the web interface and determined when 
the survey will be presented, the survey interface (once properly set up) will 
automatically display the survey when the desired time comes.

SETUP AND USE:
Before adding the interface, ensure your scene is set up to be able to interact
with UI canvases.

In order to allow the interface to function properly, drag the "SurveyInterface"
prefab into your scene. Adjust the position, rotation, and scale as desired. If
you wish to preview the interface, adjust the "alpha" value of the CanvasGroup
component attached to the interface's canvas (the first child object of the interface).

The interface is equipped with the ability to be navigated accessibly using the 
VERA Locomotion Accessibility Toolkit (the VLAT). In order to allow these tools to 
function, ensure the VLAT_Menu is properly set up in your scene (see the VLAT README for
further information on setting up the VLAT_Menu). If a VLAT_Menu is somewhere in
the scene, the accessible controls will auto-activate when the survey process
is initiated.

The interface is defaultly in world space. If you wish for the interface to follow 
the camera, place the interface as a child component of your camera.

If you wish for the interface to be interactable in VR, attach the corresponding VR
component (such as TrackedDeviceGraphicsRaycaster for Unity's XR Toolkit) to the interface's 
canvas, which is the first child component of the interface.

