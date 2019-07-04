using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV3
{
    public class SplineBuilder
    {
        public struct Segment
        {
            public float time;
            public float distance;
            public Segment(float time, float distance)
            {
                this.time = time;
                this.distance = distance;
            }
        }

        public List<Vector3> _nodes;
        public List<Segment> segments;

        public int segmentResolution = 5;

        public float pathLength;

        //constructor
        public SplineBuilder(TrafficPath path)
        {
            _nodes = path.nodes;
            BuildPath();
        }

        public void BuildPath()
        {
            var totalSubdivisions = _nodes.Count * segmentResolution;
            segments = new List<Segment>(totalSubdivisions);
            pathLength = 0;
            var segmentLength = 1f / totalSubdivisions;
            var lastPoint = getPoint(0);

            for (int i=1; i<totalSubdivisions+1; i++)
            {
                var currentSegment = segmentLength * i;
                var currentPoint = getPoint(currentSegment);
                pathLength += Vector3.Distance(currentPoint, lastPoint);
                lastPoint = currentPoint;
                segments.Add(new Segment(currentSegment, pathLength));
            }


        }

        public Vector3 getPoint(float t)
        {
            int numSections = _nodes.Count - 3;
            int currentNode = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
            float u = t * (float)numSections - (float)currentNode;

            Vector3 a = _nodes[currentNode];
            Vector3 b = _nodes[currentNode + 1];
            Vector3 c = _nodes[currentNode + 2];
            Vector3 d = _nodes[currentNode + 3];

            return .5f *
            (
                (-a + 3f * b - 3f * c + d) * (u * u * u)
                + (2f * a - 5f * b + 4f * c - d) * (u * u)
                + (-a + c) * u
                + 2f * b
            );
        }

        // gets the point taking in to account constant speed. the default implementation approximates the length of the spline
        // by walking it and calculating the distance between each node
        public virtual Vector3 getPointOnPath(float t)
        {
            // we know exactly how far along the path we want to be from the passed in t
            float targetDistance = pathLength * t;

            // loop through all the values in our lookup table and find the two nodes our targetDistance falls between
            // translate the values from the lookup table estimating the arc length between our known nodes from the lookup table
            int nextSegmentIndex;
            for (nextSegmentIndex = 0; nextSegmentIndex < segments.Count; nextSegmentIndex++) {
                if (segments[nextSegmentIndex].distance >= targetDistance)
                    break;
            }

            Segment nextSegment = segments[nextSegmentIndex];

            if (nextSegmentIndex == 0) {
                // t within first segment
                t = (targetDistance / nextSegment.distance) * nextSegment.time;
            } else {
                // t within prev..next segment
                Segment previousSegment = segments[nextSegmentIndex - 1];

                float segmentTime = nextSegment.time - previousSegment.time;
                float segmentLength = nextSegment.distance - previousSegment.distance;

                t = previousSegment.time + ((targetDistance - previousSegment.distance) / segmentLength) * segmentTime;
            }
            return getPoint(t);
        }
    }
}