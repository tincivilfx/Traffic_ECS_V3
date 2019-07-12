using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CivilFX.TrafficV3
{
    public class PathDriver : MonoBehaviour
    {
        public GameObject [] prefabs;
        public int vehiclesCount;
        public TrafficPath path;
        [SerializeField]
        private List<VehicleController> vehicles;
        [SerializeField]
        private List<VehicleController> vehiclesWaiting;
        private VehicleController strayVehicle;
        private float pathLength;
        private SplineBuilder pathSpline;
        // Start is called before the first frame update
        private float timeScale;
        void Awake()
        {
            vehiclesWaiting = new List<VehicleController>(vehiclesCount / 2);
            vehicles = new List<VehicleController>(vehiclesCount);
            while (vehiclesCount > 0) {
                var go = GameObject.Instantiate(prefabs[Random.Range(0, prefabs.Length)]);
                go.transform.SetParent(transform);
                vehicles.Add(go.GetComponent<VehicleController>());
                vehiclesCount--;
            }

            pathSpline = path.GetSplineBuilder();
            pathLength = pathSpline.pathLength;
            Debug.Log(pathLength);

            var uSegment = pathLength / vehicles.Count;

            var i = 0;
            foreach (var item in vehicles) {
                item.u = uSegment * i;
                item.lane = 0;
                ++i;
            }
            vehicles.Sort();
            foreach (var item in vehicles) {
                item.SetPosition(GetPositionFromLongitute(item.u));
            }
            timeScale = Time.timeScale;
            //EditorApplication.isPaused = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                timeScale += 0.5f;
                Time.timeScale = timeScale;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                timeScale -= 0.5f;
                Time.timeScale = timeScale;
            }

        }

        private void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;
            UpdateLastLCTimes(dt);
            CalcAccelerations();
            ChangeLanes();
            UpdateSpeedPositions(dt);
            UpdateBCDown();
            UpdateBCUp();
            UpdateFinalPositions();
        }

        private Vector3 GetPositionFromLongitute(float u)
        {
            if (u < pathLength) {
                return pathSpline.getPointOnPath(u / pathLength);
            } else {
                return pathSpline.getPointOnPath(1f);
            }
        }

        private void UpdateLastLCTimes(float dt)
        {
            foreach (var item in vehicles) {
                item.dt_afterLC += dt;
                item.dt_lastPassiveLC += dt;
            }
        }

        private void CalcAccelerations()
        {
            UpdateEnvironment();
            int i = 0;
            foreach (var item in vehicles) {
                var speed = item.speed;
                var iLead = item.iLead;

                var s = vehicles[iLead].u - vehicles[iLead].length - item.u;
                var speedLead = vehicles[iLead].speed;
                var accLead = vehicles[iLead].acc;
                if (iLead >= i) {
                    s = 10000;
                    accLead = 0;
                }
                //Debug.Log(s + ":" + speed + ":" + speedLead + ":" + accLead);
                //Debug.Log("Acc Before: " + item.acc);
                item.acc = item.longModel.CalculateAcceleration(s, speed, speedLead, accLead);
                //Debug.Log("Acc After: " + item.acc.ToString("###############.################"));
                ++i;
            }
        }

        #region Update Environment
        private void UpdateEnvironment()
        {
            for (int i=0; i<vehicles.Count; i++) {
                UpdateILead(i);
                UpdateILag(i);
                UpdateILeadRight(i);
                UpdateILagRight(i);
                UpdateILeadLeft(i);
                UpdateILagLeft(i);
            }
        }

        private void UpdateILead(int i)
        {
            var n = vehicles.Count;
            var iLead = (i == 0) ? n - 1 : i - 1;  //!! also for non periodic BC
            var success = (vehicles[iLead].lane == vehicles[i].lane);
            while (!success) {
                iLead = (iLead == 0) ? n - 1 : iLead - 1;
                success = ((i == iLead) || (vehicles[iLead].lane == vehicles[i].lane));
            }
            vehicles[i].iLead = iLead;
        }
        private void UpdateILag(int i)
        {
            var n = vehicles.Count;
            var iLag = (i == n - 1) ? 0 : i + 1;
            var success = (vehicles[iLag].lane == vehicles[i].lane);
            while (!success) {
                iLag = (iLag == n - 1) ? 0 : iLag + 1;
                success = ((i == iLag) || (vehicles[iLag].lane == vehicles[i].lane));
            }
            vehicles[i].iLag = iLag;
        }

        private void UpdateILeadRight(int i)
        {
            var n = vehicles.Count;
            int iLeadRight;
            if (vehicles[i].lane < path.lanesCount - 1) {
                iLeadRight = (i == 0) ? n - 1 : i - 1;
                var success = ((i == iLeadRight) || (vehicles[iLeadRight].lane == vehicles[i].lane + 1));
                while (!success) {
                    iLeadRight = (iLeadRight == 0) ? n - 1 : iLeadRight - 1;
                    success = ((i == iLeadRight) || (vehicles[iLeadRight].lane == vehicles[i].lane + 1));
                }
            } else { iLeadRight = -10; }
            vehicles[i].iLeadRight = iLeadRight;
        }

        private void UpdateILagRight(int i)
        {
            var n = vehicles.Count;
            int iLagRight;
            if (vehicles[i].lane < path.lanesCount - 1) {
                iLagRight = (i == n - 1) ? 0 : i + 1;
                var success = ((i == iLagRight) || (vehicles[iLagRight].lane == vehicles[i].lane + 1));
                while (!success) {
                    iLagRight = (iLagRight == n - 1) ? 0 : iLagRight + 1;
                    success = ((i == iLagRight) || (vehicles[iLagRight].lane == vehicles[i].lane + 1));
                }
            } else { iLagRight = -10; }
            vehicles[i].iLagRight = iLagRight;
        }

        private void UpdateILeadLeft(int i)
        {
            var n = vehicles.Count;

            int iLeadLeft;
            if (vehicles[i].lane > 0) {
                iLeadLeft = (i == 0) ? n - 1 : i - 1;
                var success = ((i == iLeadLeft) || (vehicles[iLeadLeft].lane == vehicles[i].lane - 1));
                while (!success) {
                    iLeadLeft = (iLeadLeft == 0) ? n - 1 : iLeadLeft - 1;
                    success = ((i == iLeadLeft) || (vehicles[iLeadLeft].lane == vehicles[i].lane - 1));
                }
            } else { iLeadLeft = -10; }
            vehicles[i].iLeadLeft = iLeadLeft;
        }

        private void UpdateILagLeft(int i)
        {
            var n = vehicles.Count;
            int iLagLeft;

            if (vehicles[i].lane > 0) {
                iLagLeft = (i == n - 1) ? 0 : i + 1;
                var success = ((i == iLagLeft) || (vehicles[iLagLeft].lane == vehicles[i].lane - 1));
                while (!success) {
                    iLagLeft = (iLagLeft == n - 1) ? 0 : iLagLeft + 1;
                    success = ((i == iLagLeft) || (vehicles[iLagLeft].lane == vehicles[i].lane - 1));
                }
            } else { iLagLeft = -10; }
            vehicles[i].iLagLeft = iLagLeft;
        }
        #endregion

        #region Change Lanes
        private void ChangeLanes()
        {
            DoChangesInDirection(true); //changes to right
            DoChangesInDirection(false); //changes to left
        }

        private void DoChangesInDirection(bool toRight)
        {
            //Debug.Log("DoChangesInDirection");
            var uminLC = 20f;
            var waitTime = 4f;
            for (int i=0; i<vehicles.Count; i++) {
                if (vehicles[i].u > uminLC) {
                    // test if there is a target lane 
                    // and if last change is sufficiently long ago

                    var newLane = (toRight) ? vehicles[i].lane + 1 : vehicles[i].lane - 1;
                    var targetLaneExists = (newLane >= 0) && (newLane < path.lanesCount);
                    var lastChangeSufficTimeAgo = (vehicles[i].dt_afterLC > waitTime)
                    && (vehicles[i].dt_lastPassiveLC > 0.2f * waitTime);

                    if (targetLaneExists && lastChangeSufficTimeAgo) {

                        var iLead = vehicles[i].iLead;
                        var iLag = vehicles[i].iLag; // actually not used
                        var iLeadNew = (toRight) ? vehicles[i].iLeadRight : vehicles[i].iLeadLeft;
                        var iLagNew = (toRight) ? vehicles[i].iLagRight : vehicles[i].iLagLeft; ;

                        // check if also the new leader/follower did not change recently

                        //Debug.Log("iLeadNew="+iLeadNew+" dt_afterLC_iLeadNew="+vehicles[iLeadNew].dt_afterLC+" dt_afterLC_iLagNew="+vehicles[iLag].dt_afterLC); 

                        if ((vehicles[i].id != 1) // not an ego-vehicle
                       && (iLeadNew >= 0)       // target lane allowed (otherwise iLeadNew=-10)
                       && (vehicles[iLeadNew].dt_afterLC > waitTime)  // lower time limit
                       && (vehicles[iLagNew].dt_afterLC > waitTime)) { // for serial LC

                            //console.log("changeLanes: i=",i," cond 2 passed");
                            var acc = vehicles[i].acc;
                            var accLead = vehicles[iLead].acc;
                            var accLeadNew = vehicles[iLeadNew].acc; // leaders: exogen. for MOBIL
                            var speed = vehicles[i].speed;
                            var speedLeadNew = vehicles[iLeadNew].speed;
                            var sNew = vehicles[iLeadNew].u - vehicles[iLeadNew].length - vehicles[i].u;
                            var sLagNew = vehicles[i].u - vehicles[i].length - vehicles[iLagNew].u;

                            // treat case that no leader/no veh at all on target lane
                            // notice: if no target vehicle iLagNew=i set in updateEnvironment()
                            //    => update_iLagLeft, update_iLagRight

                            if (iLeadNew >= i) { // if iLeadNew=i => laneNew is empty
                                sNew = 10000;
                            }
                            // treat case that no follower/no veh at all on target lane
                            if (iLagNew <= i) { // if iLagNew=i => laneNew is empty
                                sLagNew = 10000;
                            }


                            // calculate MOBIL input

                            var vrel = vehicles[i].speed / vehicles[i].longModel.v0;
                            var accNew = vehicles[i].longModel.CalculateAcceleration(sNew, speed, speedLeadNew, accLeadNew);

                            // reactions of new follower if LC performed
                            // it assumes new acceleration of changing veh

                            var speedLagNew = vehicles[iLagNew].speed;
                            var accLagNew
                                = vehicles[iLagNew].longModel.CalculateAcceleration(sLagNew, speedLagNew, speed, accNew);

                            // final MOBIL incentive/safety test before actual lane change
                            // (regular lane changes; for merges, see below)


                            //var log=(item.type==="truck");
                            var log = false;
                            //var log=true;

                            var MOBILOK = vehicles[i].LCModel.RealizeLaneChange(vrel, acc, accNew, accLagNew, toRight);


                            var changeSuccessful =(sNew > 0) && (sLagNew > 0) && MOBILOK;
                            if (changeSuccessful) {

                                // do lane change in the direction toRight (left if toRight=0)
                                //!! only regular lane changes within road; merging/diverging separately!

                                vehicles[i].dt_afterLC = 0;                // active LC
                                vehicles[iLagNew].dt_lastPassiveLC = 0;   // passive LC
                                vehicles[iLeadNew].dt_lastPassiveLC = 0;
                                vehicles[iLead].dt_lastPassiveLC = 0;
                                vehicles[iLag].dt_lastPassiveLC = 0;

                                vehicles[i].laneOld = vehicles[i].lane;
                                vehicles[i].lane = newLane;
                                vehicles[i].acc = accNew;
                                vehicles[iLagNew].acc = accLagNew;

                                // update the local envionment implies 12 updates, 
                                // better simply to update all ...

                            }
                        }
                    }
                }
            }
            SortVehicles();
            UpdateEnvironment();
        }
        #endregion

        private void UpdateSpeedPositions(float dt)
        {
            foreach (var item in vehicles) {
                //Debug.Log("U before:" + item.u);
                item.u += Mathf.Max(0, item.speed * dt + 0.5f * item.acc * dt * dt);
                //Debug.Log("U After:" + item.u);
                item.speed = Mathf.Max(item.speed + item.acc * dt, 0);
            }

            SortVehicles();
            UpdateEnvironment();
        }

        private void UpdateBCDown()
        {
            //outflow
            if (vehicles[0].u > pathLength) {
                var vehicle = vehicles[0];
                vehicles.RemoveAt(0);
                vehicle.gameObject.SetActive(false);
                vehiclesWaiting.Add(vehicle);
                SortVehicles();
                UpdateEnvironment();
            }
        }

        private void UpdateBCUp()
        {
            if (vehiclesWaiting.Count > 0) {
                var newLane = vehicles[vehicles.Count - 1].lane + 1;
                newLane %= path.lanesCount;
                for (int i = vehicles.Count -2; i>=0; i--) {
                    if (vehicles[i].lane == newLane) {
                        if (vehicles[i].u > 10f) {
                            var vehicle = vehiclesWaiting[0];
                            vehiclesWaiting.RemoveAt(0);
                            vehicle.gameObject.SetActive(true);
                            var newSpeed = Utilities.Map(vehicles[i].u - vehicles[i].length, 0, 10, 0, 30);
                            newSpeed = Mathf.Clamp(newSpeed, 0, 30f);
                            vehicle.Init(newLane, newSpeed);
                            vehicles.Add(vehicle);
                            SortVehicles();
                            UpdateEnvironment();
                        }
                        return;
                    }
                }

            }
        }

        private void UpdateFinalPositions()
        {
            var numLanes = path.lanesCount;

            foreach (var item in vehicles) {
                var centerStart = GetPositionFromLongitute(item.u);
                var centerEnd = GetPositionFromLongitute(item.u + 0.01f);
                var dir = (centerEnd - centerStart).normalized;
                var right = Vector3.Cross(Vector3.up, dir) * path.calculatedWidth;
                var left = -right;
                var seg = 1.0f / (path.lanesCount * 2);
                seg = item.lane * (1f / path.lanesCount) + seg;
                var pos = Vector3.Lerp(centerStart + left, centerStart + right, seg);
                var lookAt = Vector3.Lerp(centerEnd + left, centerEnd + right, seg);
                item.SetPosition(pos);
                item.SetLookAt(lookAt);
                
                //item.SetPosition(GetPositionFromLongitute(item.u));
                //item.SetLookAt(GetPositionFromLongitute(item.u + 0.1f));
            }
        }

        private void SortVehicles()
        {
            vehicles.Sort();
        }
    }
}