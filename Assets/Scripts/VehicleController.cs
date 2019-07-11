using System;
using UnityEngine;

namespace CivilFX.TrafficV3 {
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

        public int id = 200;
        public bool divergeAhead = false;
        public bool toRight = false;

        public float fracLaneOptical = 1;

        public float dt_LC = 4;
        public float dt_afterLC = 10;
        public float dt_lastPassiveLC = 10;
        public float acc = 0;
        public int iLead = -100;
        public int iLag = -100;

        public int iLeadRight = -100;
        public int iLeadLeft = -100;
        public int iLagRight = -100;
        public int iLagLeft = -100;

        public CarFollowingModel longModel;
        public LaneChangingModel LCModel;

        public int CompareTo(VehicleController other)
        {
            return other.u.CompareTo(u);
        }

        public void Init()
        {

        }
        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public void SetLookAt(Vector3 pos)
        {
            transform.LookAt(pos);
        }
    }
}