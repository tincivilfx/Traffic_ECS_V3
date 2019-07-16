using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace CivilFX.TrafficV3
{
    public enum Unit
    {
        Meters,
        Feet
    }

    [System.Serializable]
    public struct CutSegment
    {
        public float u;
        public int lane;
    }

    public class TrafficPath : MonoBehaviour
    {
        public Unit unit = Unit.Meters;
        public float widthPerLane = 5.0f;
        [Tooltip("Width of all lanes; in meters")]
        public float calculatedWidth = 5.0f;

        public float pathLength; //overall arc length

        [Range(1, 10)]
        public int lanesCount = 1;
        public int splineResolution = 30;
        public List<Vector3> nodes;

        public CutSegment [] cutSegments;

        private SplineBuilder splineBuilder;

        public int GetNodesCount()
        {
            return nodes.Count;
        }

        public SplineBuilder GetSplineBuilder(bool forceRebuild=false)
        {
            if (forceRebuild || splineBuilder == null)
            {
                splineBuilder = new SplineBuilder(this);
            }
            return splineBuilder;
        }


        //Just for visualization for now
        //function to splice the path into different box segment

#if UNITY_EDITOR
        [DrawGizmo(GizmoType.Active | GizmoType.NotInSelectionHierarchy
            | GizmoType.InSelectionHierarchy | GizmoType.Pickable, typeof(TrafficPath))]
        static void DrawGizmos(TrafficPath path, GizmoType gizmoType)
        {
            if (path.GetNodesCount() < 2)
            {
                return;
            }

            SplineBuilder splineBuilder = path.GetSplineBuilder();
            var segmentation = 1.0f / path.splineResolution;
            var t = 0.0f;
            var lanesCount = path.lanesCount;
            Gizmos.color = Color.green;

            var centerStart = splineBuilder.getPoint(0);
            var centerEnd = Vector3.zero;
            var dir = Vector3.zero;
            var left = dir;
            var right = dir;
            t = segmentation;

            while (t <= 1.0f)
            {
                centerEnd = splineBuilder.getPoint(t);

                dir = math.normalize((float3)(centerEnd - centerStart));
                left = Vector3.Cross(Vector3.up, dir) * path.calculatedWidth;
                right = -left;

                //draw inner lines
                var laneSegment = 1.0f / lanesCount; // |_._._| : . = laneSegment
                var laneTime = laneSegment;
                while (laneTime < 1.0f) {
                    var laneLastPoint = Vector3.Lerp((Vector3)(centerStart + left), (Vector3)(centerStart + right), laneTime);
                    var laneCurrentPoint = Vector3.Lerp(centerEnd + left, centerEnd + right, laneTime);
                    Gizmos.DrawLine(laneLastPoint, laneCurrentPoint);
                    laneTime += laneSegment;
                }

                //draw most outter lines
                Gizmos.DrawLine((Vector3)(centerStart + left), centerEnd + left); // E |
                Gizmos.DrawLine((Vector3)(centerStart + right), centerEnd + right); // | E



                centerStart = centerEnd;
                t += segmentation;

                //draw closing line
                if (t >= 1.0f)
                {
                    Gizmos.DrawLine(centerStart, centerStart + left);  // E_
                    Gizmos.DrawLine(centerStart, centerStart + right); // _E
                }
            }

            //draw starting arrows
            centerStart = splineBuilder.getPoint(0.01f);
            centerEnd = splineBuilder.getPoint(0.015f);
            dir = math.normalize((float3)(centerEnd - centerStart));
            left = Vector3.Cross(Vector3.up, dir) * path.calculatedWidth;
            right = -left;
            var leftStart = centerStart + left;
            var leftEnd = centerEnd + left;
            var rightStart = centerStart + right;
            var rightEnd = centerEnd + right;
            var seg = 1.0f / (lanesCount * 2);
            var time = seg;
            var skip = false;
            Gizmos.color = Color.yellow;
            while (time < 1.0f) {
                if (!skip) {
                    centerStart = Vector3.Lerp(leftStart, rightStart, time);
                    centerEnd = Vector3.Lerp(leftEnd, rightEnd, time);
                    dir = math.normalize(centerEnd - centerStart);
                    left = Vector3.Cross(Vector3.up, dir) * (path.calculatedWidth / (lanesCount * 2));
                    right = -left;
                    Gizmos.DrawLine(centerStart + left, centerEnd);
                    Gizmos.DrawLine(centerStart + right, centerEnd);
                    skip = true;
                } else {
                    skip = false;
                }              
                time += seg;
            }

            /*
            //draw rough lanes
            Gizmos.color = Color.red;

            for (int i = 0; i < path.nodes.Count - 1; i++)
            {
                var centerStart = path.nodes[i];
                var centerEnd = path.nodes[i + 1];

                var dir = math.normalize(centerEnd - centerStart);
                var left = Vector3.Cross(Vector3.up, dir) * path.width;
                var right = -left;
                

                Gizmos.DrawLine(centerStart, centerStart + left);  // S_
                Gizmos.DrawLine(centerStart, centerStart + right); // _S
                Gizmos.DrawLine(centerEnd, centerEnd + left);  // E_
                Gizmos.DrawLine(centerEnd, centerEnd + right); // _E
                Gizmos.DrawLine(centerStart + left, centerEnd + left); // E |
                Gizmos.DrawLine(centerStart + right, centerEnd + right); // | E
            }
            */
            /*
            //draw center rough lines
            for (int i=0; i<path.nodes.Count-1; i++)
            {
                Gizmos.DrawLine(path.nodes[i], path.nodes[i + 1]);
            }
            */

        }
#endif
        void GetSplineSectionInternal(int index, out float3 p0, out float3 p1, out float3 p2, out float3 p3)
        {
            p1 = nodes[index];
            p2 = nodes[index + 1];

            if (index == 0)
            {
                // compute p0
                p0 = p1 + (p1 - p2);
            }
            else
            {
                p0 = nodes[index - 1];
            }

            if (index == GetNodesCount() - 2)
            {
                // compute p3
                p3 = p2 + (p2 - p1);
            }
            else
            {
                p3 = nodes[index + 2];
            }
        }
    }
}