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
        public Vector3 currentVelocity;
        public CarFollowingModel longModel;
        public LaneChangingModel LCModel;

        public bool debug;

        public VehicleController()
        {

        }

        public VehicleController(float _length, float _width, float _u, int _lane, float _speed, string _type)
        {
            length = _length; // car length[m]
            width = _width;   // car width[m]
            u = _u;           // long coordinate=arc length [m]
            lane = _lane;     // integer-valued lane 0=leftmost
            v = lane;        // lane coordinate (lateral, units of lane width), not speed!!
            dvdt = 0;     // vehicle angle to road axis (for drawing purposes)
            laneOld = lane;  // for logging and drawing vontinuous lat coords v
            speed = _speed;
            type = _type;

            id = 200; // 


            divergeAhead = false; // if true, the next diverge can/must be used
            toRight = false; // set strong urge to toRight,!toRight IF divergeAhead

            fracLaneOptical = 1; // slow optical LC over fracLaneOptical lanes
 

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
            //iLeadRightOld=-100;
            //iLeadLeftOld=-100;
            //iLagRightOld=-100;
            //iLagLeftOld=-100;

            // just start values used for virtual vehicles
            longModel = new ACC(20, 1.3f, 2, 1, 2);//IDM_v0,IDM_T,IDM_s0,IDM_a,IDM_b);
            LCModel = new MOBIL(4, 20, 0.1f, 0.2f, 0.3f); //bSafe, bSafeMax, p, bThr, biasRight)
        }

        public int CompareTo(VehicleController other)
        {
            return other.u.CompareTo(u);
        }

        public void Init(int newLane, float newSpeed)
        {
            Reset();
            lane = newLane;
            speed = newSpeed;
        }

        public void Reset()
        {
            u = 0;
            lane = 0;
            laneOld = 0;
            speed = 30;

            dt_LC = 4;
            dt_afterLC = 10;
            dt_lastPassiveLC = 10;

            acc = 0;


            iLead = -100;
            iLag = -100;
            iLeadRight = -100;
            iLeadLeft = -100;
            iLagRight = -100;
            iLagLeft = -100;
        }
        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public void SetPositionForward(float f)
        {
            transform.position += transform.forward * f;
        }

        public void SetLookAt(Vector3 pos)
        {
            transform.LookAt(pos);
        }
    }
}