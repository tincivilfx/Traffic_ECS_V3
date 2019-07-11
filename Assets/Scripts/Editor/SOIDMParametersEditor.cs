using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CivilFX.TrafficV3 {

    [CustomEditor(typeof(SOIDMParameters))]
    public class SOIDMParametersEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            var sp = serializedObject.FindProperty("type");
            if (sp.enumValueIndex == 0) {
                //car
                serializedObject.FindProperty("desiredSpeed").floatValue = 120f;
                serializedObject.FindProperty("safetyTime").floatValue = 1.5f;
                serializedObject.FindProperty("minGap").floatValue = 2f;
                serializedObject.FindProperty("acceleration").floatValue = 0.3f;
                serializedObject.FindProperty("deceleration").floatValue = 3f;
                serializedObject.FindProperty("accelerationExponent").intValue = 4;
            } else if (sp.enumValueIndex == 1) {
                //truck
                serializedObject.FindProperty("desiredSpeed").floatValue = 80f;
                serializedObject.FindProperty("safetyTime").floatValue = 1.7f;
                serializedObject.FindProperty("minGap").floatValue = 2f;
                serializedObject.FindProperty("acceleration").floatValue = 0.3f;
                serializedObject.FindProperty("deceleration").floatValue = 2f;
                serializedObject.FindProperty("accelerationExponent").intValue = 4;
            }
            serializedObject.ApplyModifiedProperties();
            
        }
    }
}