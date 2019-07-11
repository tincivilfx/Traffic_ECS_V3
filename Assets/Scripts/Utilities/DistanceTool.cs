using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV3
{
    public class DistanceTool : MonoBehaviour
    {
        public Vector3[] nodes;
        public bool drawSphere;

        private void OnDrawGizmosSelected()
        {
            if (drawSphere)
            {
                Gizmos.DrawWireSphere(nodes[0], 5.0f);
                Gizmos.DrawWireSphere(Vector3.Lerp(nodes[0], nodes[1], 0.33f), 5.0f);
                Gizmos.DrawWireSphere(Vector3.Lerp(nodes[0], nodes[1], 0.66f), 5.0f);
                Gizmos.DrawWireSphere(nodes[1], 5.0f);
            }
        }

    }
}