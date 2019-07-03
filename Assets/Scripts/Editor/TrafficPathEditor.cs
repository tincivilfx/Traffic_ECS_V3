﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CivilFX.TrafficV3
{
    [CustomEditor(typeof(TrafficPath))]
    public class TrafficPathEditor : Editor
    {
        private TrafficPath _target;
        private SerializedObject so;
        private GUIStyle labelStyle;

        private void OnEnable()
        {
            _target = (TrafficPath)target;
            so = serializedObject;

            so.Update();
            var nodesProp = so.FindProperty("nodes");
            while (nodesProp.arraySize < 2)
            {
                nodesProp.InsertArrayElementAtIndex(nodesProp.arraySize == 0 ? 0 : nodesProp.arraySize - 1);
                nodesProp.GetArrayElementAtIndex(nodesProp.arraySize - 1).vector3Value = _target.transform.position + Vector3.one;
            }
            so.ApplyModifiedProperties();

            labelStyle = new GUIStyle();
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 16;
        }

        public override void OnInspectorGUI()
        {
            so.Update();

            //show script name
            SerializedProperty currentProp = so.FindProperty("m_Script");
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(currentProp);
            }

            //path width
            currentProp = so.FindProperty("width");
            EditorGUILayout.PropertyField(currentProp);

            //lanes count
            currentProp = so.FindProperty("lanesCount");
            EditorGUILayout.PropertyField(currentProp);

            //spline resolution
            currentProp = so.FindProperty("splineResolution");
            EditorGUILayout.PropertyField(currentProp);

            //nodes
            currentProp = so.FindProperty("nodes");
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(currentProp, new GUIContent(""), false, GUILayout.MaxWidth(1));
            EditorGUILayout.LabelField("Nodes:", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
            if (GUILayout.Button("Reverse", GUILayout.MaxWidth(100))) {
                SerializedProperty nodesProp = so.FindProperty("nodes");
                List<Vector3> nodes = new List<Vector3>(nodesProp.arraySize);
                var iterator = nodesProp.GetEnumerator();
                while (iterator.MoveNext()) {
                    nodes.Add(((SerializedProperty)iterator.Current).vector3Value);
                }
                iterator = nodesProp.GetEnumerator();
                for (int i = nodes.Count - 1; i >= 0; i--) {
                    iterator.MoveNext();
                    ((SerializedProperty)iterator.Current).vector3Value = nodes[i];
                }
            }
            GUILayout.EndHorizontal();
            if (currentProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < currentProp.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();
                    if (i == 0)
                    {
                        EditorGUILayout.LabelField("Begin", GUILayout.MaxWidth(50));
                    }
                    else if (i == currentProp.arraySize - 1)
                    {
                        EditorGUILayout.LabelField("End", GUILayout.MaxWidth(50));
                    }
                    else
                    {
                        EditorGUILayout.LabelField(i.ToString(), GUILayout.MaxWidth(50));
                    }
                    SerializedProperty nodeProp = currentProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(nodeProp, new GUIContent(""));

                    //delete button
                    if (GUILayout.Button(new GUIContent("X", "Delete this node"), GUILayout.MaxWidth(50)))
                    {
                        currentProp.MoveArrayElement(i, currentProp.arraySize - 1);
                        currentProp.arraySize -= 1;
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
            so.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            //short cut to project nodes
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
            {
                ProjectNodes(_target.nodes);
                /*
                if (e.control)
                {
                    drawBorder = !drawBorder;
                }
                else
                {
                    projectNodesCount = ProjectNodes(_target.nodes);
                }
                */
            }

            //double click to add
            if (e.type == EventType.MouseDown && e.clickCount > 1)
            {
                int index = LocateNearestNode(_target.nodes, e.mousePosition);
                var direction = Vector3.zero;
                var newNode = Vector3.zero;
                var length = 100.0f;
                var nodesCount = _target.GetNodesCount();

                if (index == nodesCount - 1)
                {
                    direction = (_target.nodes[nodesCount - 1] - _target.nodes[nodesCount - 2]).normalized;
                    newNode = _target.nodes[nodesCount - 1] + (direction * length);
                } else if (index == 0)
                {
                    direction = (_target.nodes[1] - _target.nodes[0]).normalized;
                    newNode = _target.nodes[0] + (-direction * length);
                } else
                {
                    direction = (_target.nodes[index + 1] - _target.nodes[index]).normalized;
                    length = Vector3.Distance(_target.nodes[index], _target.nodes[index + 1]) / 2;
                    newNode = _target.nodes[index] + (direction * length);
                }
                Undo.RecordObject(target, "AddNodeSingle");
                _target.nodes.Insert(index == 0 ? index : index + 1, newNode);
            }


            //draw nodes handle
            for (int i = 0; i < _target.nodes.Count; i++)
            {

                Vector3 currentPos = _target.nodes[i];

                //draw label
                if (i == 0)
                {
                    Handles.Label(currentPos, "Begin", labelStyle);
                }
                else if (i == _target.nodes.Count - 1)
                {
                    Handles.Label(currentPos, "End", labelStyle);
                }
                else
                {
                    Handles.Label(currentPos, i.ToString(), labelStyle);
                }

                //draw handle
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(currentPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "MoveSingleNode");
                    if (e.control)
                    {
                        MoveAllNodes(_target.nodes, newPos - currentPos);
                    }
                    else
                    {
                        _target.nodes[i] = newPos;
                        //auto-adjust start controlled point (Begin)
                        if (i == 1) {
                            _target.nodes[0] = newPos + (newPos - _target.nodes[2]);
                        } else if (i == _target.nodes.Count - 2) {
                            //auto adjust end controlled point (End)
                            _target.nodes[_target.nodes.Count - 1] = newPos + (newPos - _target.nodes[_target.nodes.Count - 3]);
                        }
                    }
                }
            }
        }

        private int ProjectNodes(List<Vector3> nodes)
        {
            Undo.RecordObject(target, "ProjectNodes");
            Vector3 currentPos;
            RaycastHit hit;
            int count = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                currentPos = nodes[i];
                if (Physics.Raycast(currentPos + Vector3.up, Vector3.down, out hit, 10000f))
                {
                    //cast down
                    nodes[i] = hit.point;
                }
                else if (Physics.Raycast(currentPos + Vector3.down, Vector3.up, out hit, 10000f))
                {
                    nodes[i] = hit.point;
                }
                else
                {
                    count++;
                    Debug.Log("Not Hit");
                }
            }
            return count;
        }

        private void MoveAllNodes(List<Vector3> nodes, Vector3 delta)
        {
            Undo.RecordObject(target, "MoveAllNodes");
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i] += delta;
            }
        }

        private int LocateNearestNode(List<Vector3> nodes, Vector2 mousePos)
        {
            int index = -1;
            float minDistance = float.MaxValue;

            for (int i = 0; i < nodes.Count; i++)
            {
                var nodeToGUI = HandleUtility.WorldToGUIPoint(nodes[i]);
                var dis = Vector2.Distance(nodeToGUI, mousePos);
                if (dis < minDistance)
                {
                    minDistance = dis;
                    index = i;
                }
            }
            return index;
        }

    }


}