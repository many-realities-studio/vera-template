using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VLAT_DoorInteractable))]
public class VLAT_DoorInteractableEditor : Editor
{

    // Editor for VLAT_DoorInteractable


    #region VARIABLES


    SerializedProperty twoWayDoor;

    SerializedProperty pushedOpenRotation; // Only when two way
    SerializedProperty pulledOpenRotation; // Only when two way
    SerializedProperty openedRotation; // Only when not two way
    SerializedProperty closedRotation; // Always

    SerializedProperty doorHingeJoint;
    SerializedProperty pushedOpenHingeAngle; // Only when two way
    SerializedProperty pulledOpenHingeAngle; // Only when two way
    SerializedProperty openHingeAngle; // Only when not two way
    SerializedProperty closedHingeAngle; // Always


    #endregion


    #region ON ENABLE


    // OnEnable, set up properties
    //--------------------------------------//
    private void OnEnable()
    //--------------------------------------//
    {
        twoWayDoor = serializedObject.FindProperty("twoWayDoor");

        doorHingeJoint = serializedObject.FindProperty("doorHingeJoint");
        pushedOpenHingeAngle = serializedObject.FindProperty("pushedOpenHingeAngle");
        pulledOpenHingeAngle = serializedObject.FindProperty("pulledOpenHingeAngle");
        openHingeAngle = serializedObject.FindProperty("openHingeAngle");
        closedHingeAngle = serializedObject.FindProperty("closedHingeAngle");

    } // END OnEnable


    // OnInspectorGUI, draw items
    //--------------------------------------//
    public override void OnInspectorGUI()
    //--------------------------------------//
    {
        serializedObject.Update();

        // Always display doorType and twoWayDoor
        EditorGUILayout.PropertyField(twoWayDoor);

        // Conditionally display fields based on the value of doorType and twoWayDoor
        bool actualTwoWayDoor = (bool)twoWayDoor.boolValue;

        EditorGUILayout.PropertyField(doorHingeJoint);
        if (actualTwoWayDoor)
        {
            EditorGUILayout.PropertyField(pushedOpenHingeAngle);
            EditorGUILayout.PropertyField(pulledOpenHingeAngle);
        }
        else
        {
            EditorGUILayout.PropertyField(openHingeAngle);
        }
        EditorGUILayout.PropertyField(closedHingeAngle);

        serializedObject.ApplyModifiedProperties();
    
    } // END OnInspectorGUI


    #endregion


} // END VLAT_DoorInteractableEditor.cs