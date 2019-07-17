using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CivilFX.TrafficV3
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TrafficPath))]
    public class TrafficPathEditor : Editor
    {
        private TrafficPath _target;
        private SerializedObject so;
        private GUIStyle labelStyle;
        private bool recalculateWidth;
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

            //unit
            currentProp = so.FindProperty("unit");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(currentProp);
            recalculateWidth |= EditorGUI.EndChangeCheck();

            //path width
            currentProp = so.FindProperty("widthPerLane");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(currentProp);
            recalculateWidth |= EditorGUI.EndChangeCheck();

            //calculated width
            using (new EditorGUI.DisabledGroupScope(true)) {
                currentProp = so.FindProperty("calculatedWidth");
                if (recalculateWidth) {
                    if (so.FindProperty("unit").enumValueIndex == 0) {
                        //meters
                        currentProp.floatValue = so.FindProperty("widthPerLane").floatValue * so.FindProperty("lanesCount").intValue;
                    } else {
                        //feet
                        currentProp.floatValue = so.FindProperty("widthPerLane").floatValue / 3.2808f * so.FindProperty("lanesCount").intValue;
                    }
                    recalculateWidth = false;
                }               
                EditorGUILayout.PropertyField(currentProp);

                //path length
                currentProp = so.FindProperty("pathLength");
                currentProp.floatValue = _target.GetSplineBuilder(true).pathLength;
                EditorGUILayout.PropertyField(currentProp);
            }

            //lanes count
            currentProp = so.FindProperty("lanesCount");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(currentProp);
            recalculateWidth |= EditorGUI.EndChangeCheck();

            //spline resolution
            currentProp = so.FindProperty("splineResolution");
            EditorGUILayout.PropertyField(currentProp);

            //spline color
            currentProp = so.FindProperty("splineColor");
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

            //cut segments;
            currentProp = so.FindProperty("cutSegments");
            EditorGUILayout.PropertyField(currentProp, true);

            so.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            //short cut to project nodes
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
            {
                ProjectNodes(_target.nodes);
            }

            //short cut to delete node
            if (e.type == EventType.KeyUp && e.keyCode == KeyCode.X && e.control) {
                int index = LocateNearestNode(_target.nodes, e.mousePosition);
                Undo.RecordObject(target, "DeleteNode");
                _target.nodes.RemoveAt(index);
            }

            //move scene camera to begin of node
            if (e.control && e.type == EventType.KeyDown && e.keyCode == KeyCode.F) {
                MoveSceneView(_target.nodes[0]);
            }

            //move scene camera to selected node
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.D) {
                var index = LocateNearestNode(_target.nodes, e.mousePosition);
                if (index > 0) {

                    Vector3 pos = Vector3.Lerp(_target.nodes[index - 1], _target.nodes[index], 0.5f);
                    MoveSceneView(pos);
                }
                /*
                Vector3 pos = _target.nodes[(LocateNearestNode(_target.nodes, e.mousePosition))];
                MoveSceneView(pos);
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
                            _target.nodes[0] = newPos + (newPos - _target.nodes[2]).normalized * 20f;
                        } else if (i == _target.nodes.Count - 2) {
                            //auto adjust end controlled point (End)
                            _target.nodes[_target.nodes.Count - 1] = newPos + (newPos - _target.nodes[_target.nodes.Count - 3]).normalized * 20f;
                        }
                    }
                }
            }

            //show length (if control is held) 
            if (e.control) {
                foreach (var item in Selection.gameObjects) {
                    var script = item.GetComponent<TrafficPath>();
                    if (script != null) {
                        List<Vector3> nodes = new List<Vector3>();
                        SplineBuilder splineBuilder = script.GetSplineBuilder();
                        var segmentation = 1.0f / 1000f;
                        var t = segmentation;
                        while (t < 1.0f) {
                            nodes.Add(splineBuilder.getPoint(t));
                            t += segmentation;
                        }
                        var index = LocateNearestNode(nodes, e.mousePosition);
                        var dis = 0f;
                        for (int i = 1; i < index; i++) {
                            dis += Vector3.Distance(nodes[i], nodes[i + 1]);
                        }
                        Handles.Label(nodes[index], dis.ToString(), labelStyle);
                        Handles.ArrowHandleCap(0, nodes[index], Quaternion.LookRotation(-Vector3.up), 5.0f, EventType.Repaint);
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
        private void MoveSceneView(Vector3 pos)
        {
            var view = SceneView.currentDrawingSceneView;
            if (view != null) {
                var target = new GameObject();
                var y = Camera.current.transform.position.y;
                var rot = Camera.current.transform.rotation;
                target.transform.rotation = rot;
                target.transform.position = pos + new Vector3(0, 50, 0);
                //target.transform.LookAt(pos);
                view.AlignViewToObject(target.transform);
                GameObject.DestroyImmediate(target);
            }
        }
    }


}