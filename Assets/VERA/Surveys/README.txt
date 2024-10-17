Survey Interface README

OVERVIEW:
The VERA survey interface allows you to display surveys within the study application, 
taken directly from survey data which you have uploaded to the web interface.

Once you have uploaded a survey through the web interface and determined when 
the survey will be presented, the survey interface (once properly set up) will 
automatically display the survey when the desired time comes.

SETUP AND USE:
- Before adding the interface, ensure your scene and XR rig are set up to allow
  interaction with UI canvases. The interface is set up to be interacted with
  via ray interactors.
- Add the "SurveyInterface" prefab into your scene. Adjust the position, rotation,
  and scale as desired.
- If you wish to preview the interface, adjust the "alpha" value of the CanvasGroup
  component, attached to the interface's canvas (the first child object of the interface).
- If you wish to allow the survey to use accessible controls, ensure the VLAT_Menu is
  properly set up for your scene (see the VLAT README for more details). If a VLAT_Menu
  is somewhere in your scene, the accessible controls will auto-activate when the survey
  process begins.
- The interface is defaultly in world space. If you wish for the interface to follow 
  the camera, place the interface as a child component of your camera.

For example usage of the survey interface, see the associated demo scenes.
