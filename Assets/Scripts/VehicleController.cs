using System;
using UnityEngine;

namespace CivilFX.TrafficV3
{
    public class VehicleController : MonoBehaviour, IComparable<VehicleController>
    {
        public float length;
        public float width;
        public float u;
        public int lane;
        public int v;
        public float dvdt;
        public int laneOld;
        public float speed;
        public string type;
        public bool isVirtual;

        public int id = 200;
        public bool divergeAhead = false;
        public bool toRight = false;
        public bool justMerged = false;
        public bool fromRight = false;

        public float fracLaneOptical = 1;

        public float dt_LC = 4f; //default : 4
        public float dt_afterLC = 10;
        public float dt_lastPassiveLC = 10;
        public float acc = 0;
        public int iLead = -100;
        public int iLag = -100;

        public int iLeadRight = -100;
        public int iLeadLeft = -100;
        public int iLagRight = -100;
        public int iLagLeft = -100;

        public Vector3 originalPos; //used to smoothdamp (before starting to LC)

        public CarFollowingModel longModel;
        public LaneChangingModel LCModel;


        public Transform[] wheels;

        public bool debug;

        private float rotationValue;

        public int CompareTo(VehicleController other)
        {
            return other.u.CompareTo(u);
        }

        public void Renew(float _u, int _lane, float _speed, string _type)
        {

            //length = _length; // car length[m]
            //width = _width;   // car width[m]
            u = _u;           // long coordinate=arc length [m]
            lane = _lane;     // integer-valued lane 0=leftmost
            v = lane;        // lane coordinate (lateral, units of lane width), not speed!!
            dvdt = 0;     // vehicle angle to road axis (for drawing purposes)
            laneOld = lane;  // for logging and drawing vontinuous lat coords v
            speed = _speed;
            type = _type;

            divergeAhead = false; // if true, the next diverge can/must be used
            toRight = false; // set strong urge to toRight,!toRight IF divergeAhead

            justMerged = false;
            fromRight = false;

            fracLaneOptical = 1; // slow optical LC over fracLaneOptical lanes
            originalPos = Vector3.zero;

            dt_LC = 4;
            dt_afterLC = 10;
            dt_lastPassiveLC = 10;
            acc = 0;
            iLead = -100;
            iLag = -100;
            //iLeadOld=-100; // necessary for update local environm after change
            //iLagOld=-100; // necessary for update local environm after change

            iLeadRight = -100;
            iLeadLeft = -100;
            iLagRight = -100;
            iLagLeft = -100;
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;

            if (!isVirtual) {
                foreach (var item in wheels) {
                    Vector3 rot = Vector3.zero;
                    rot.x = rotationValue;
                    item.localEulerAngles = rot;
                    rotationValue += 90.0f * (360.0f / 60.0f) * 0.002f * speed;
                }
            }
        }


        public void SetLookAt(Vector3 pos)
        {
            transform.LookAt(pos);
        }
    }
}