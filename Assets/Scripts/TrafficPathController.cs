using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV3
{
    public class TrafficPathController : MonoBehaviour
    {
        public TrafficPath path;

        public List<VehicleController> vehicles;
        private float pathLength;
        private SplineBuilder pathSpline;
        private int iTargetFirst;
        private int vehiclesCount;

        private CarFollowingModel longModelCar;
        private CarFollowingModel longModelTruck;
        private LaneChangingModel LCModelCar;
        private LaneChangingModel LCModelTruck;
        private LaneChangingModel LCModelMandatoryRight;
        private LaneChangingModel LCModelMandatoryLeft;

        public void Init(GameObject[] prefabs, int _vehiclesCount)
        {
            pathSpline = path.GetSplineBuilder(true);
            pathLength = pathSpline.pathLength;
            vehiclesCount = _vehiclesCount;
            vehicles = new List<VehicleController>(_vehiclesCount);
            while (_vehiclesCount > 0) {
                var go = GameObject.Instantiate(prefabs[Random.Range(0, prefabs.Length)]);
                go.transform.SetParent(transform);
                vehicles.Add(go.GetComponent<VehicleController>());
                _vehiclesCount--;
            }
            Debug.Log(pathLength);
            var uSegment = (pathLength - (pathLength * 0.1f)) / vehicles.Count;

            var i = 0;
            foreach (var item in vehicles) {
                item.u = uSegment * i;
                item.lane = Random.Range(0, path.lanesCount);
                item.id = 200 + i;
                ++i;
            }
            
        }

        public void Init(GameObject[] prefabs, int vehiclesCount, VehicleController[] obstacles)
        {
            Init(prefabs, vehiclesCount);
            AddObstacles(obstacles);
        }

        public void SetModels(CarFollowingModel _longModelCar, CarFollowingModel _longModelTruck, LaneChangingModel _LCModelCar, LaneChangingModel _LCModelTruck,
            LaneChangingModel _LCModelMandatoryRight, LaneChangingModel _LCModelMandatoryLeft)
        {
            longModelCar = _longModelCar;
            longModelTruck = _longModelTruck;
            LCModelCar = _LCModelCar;
            LCModelTruck = _LCModelTruck;
            LCModelMandatoryRight = _LCModelMandatoryRight;
            LCModelMandatoryLeft = _LCModelMandatoryLeft;
        }

        public void AddObstacles(VehicleController[] obstacles)
        {
            if (obstacles != null) {
                vehicles.AddRange(obstacles);
            }
        }

        private Vector3 GetPositionFromLongitute(float u)
        {
            float t = u / pathLength;
            t = Mathf.Clamp01(t);
            return pathSpline.getPointOnPath(t);
        }

        public void UpdateVehiclesModels(LaneChangingModel LCModel)
        {
            foreach (var item in vehicles) {
                if (!item.isVirtual) {
                    item.LCModel = LCModel;
                }
            }
        }

        public void SetLCMandatory(float umin, float umax, bool toRight)
        {
            foreach (var item in vehicles) {
                if (!item.isVirtual) {
                    var u = item.u;
                    if ((u > umin) && (u < umax)) {
                        item.toRight = toRight;
                        item.LCModel = toRight ? LCModelMandatoryRight : LCModelMandatoryLeft;
                    }
                }
            }
        }


        public void UpdateLastLCTimes(float dt)
        {
            foreach (var item in vehicles) {
                item.dt_afterLC += dt;
                item.dt_lastPassiveLC += dt;
            }
        }

        public void CalcAccelerations()
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
                item.acc = item.isVirtual ? 0 : item.longModel.CalculateAcceleration(s, speed, speedLead, accLead);
                //Debug.Log("Acc After: " + item.acc.ToString("###############.################"));
                ++i;
            }
        }

        #region Update Environment
        private void UpdateEnvironment()
        {
            for (int i = 0; i < vehicles.Count; i++) {
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
        public void ChangeLanes()
        {
            DoChangesInDirection(true); //changes to right
            DoChangesInDirection(false); //changes to left
        }

        private void DoChangesInDirection(bool toRight)
        {
            //Debug.Log("DoChangesInDirection");
            var uminLC = 20f;
            var waitTime = 4f;
            for (int i = 0; i < vehicles.Count; i++) {
                if (!vehicles[i].isVirtual && vehicles[i].u > uminLC) {
                    // test if there is a target lane 
                    // and if last change is sufficiently long ago
                    if (vehicles[i].debug) {
                        Debug.Log("Enter LC");
                    }
                    var newLane = (toRight) ? vehicles[i].lane + 1 : vehicles[i].lane - 1;
                    var targetLaneExists = (newLane >= 0) && (newLane < path.lanesCount);
                    var lastChangeSufficTimeAgo = (vehicles[i].dt_afterLC > waitTime)
                    && (vehicles[i].dt_lastPassiveLC > 0.2f * waitTime);

                    if (targetLaneExists && lastChangeSufficTimeAgo) {
                        if (vehicles[i].debug) {
                            Debug.Log("Enter Second LC");
                        }
                        var iLead = vehicles[i].iLead;
                        var iLag = vehicles[i].iLag; // actually not used
                        var iLeadNew = (toRight) ? vehicles[i].iLeadRight : vehicles[i].iLeadLeft;
                        var iLagNew = (toRight) ? vehicles[i].iLagRight : vehicles[i].iLagLeft; ;

                        // check if also the new leader/follower did not change recently

                        //Debug.Log("iLeadNew="+iLeadNew+" dt_afterLC_iLeadNew="+vehicles[iLeadNew].dt_afterLC+" dt_afterLC_iLagNew="+vehicles[iLag].dt_afterLC); 
                        if (vehicles[i].debug) {
                            Debug.Log("iLead: " + iLead + "iLag: " + iLeadNew + "iLeadNew: " + iLagNew);
                            Debug.Log("vehicles[iLeadNew].dt_afterLC: " + vehicles[iLeadNew].dt_afterLC);
                            Debug.Log("vehicles[iLagNew].dt_afterLC:" + vehicles[iLagNew].dt_afterLC);
                        }

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
                            var changeSuccessful = !vehicles[i].isVirtual && (sNew > 0) && (sLagNew > 0) && MOBILOK;
                            if (vehicles[i].debug) {
                                Debug.Log("! virtual: " + !vehicles[i].isVirtual + "sNew: " + sNew + "sLagNew: " + sLagNew + "MOBILOK: " + MOBILOK);
                            }
                            if (changeSuccessful) {
                                if (vehicles[i].debug) {
                                    Debug.Log("Enter 3rd LC");
                                }
                                // do lane change in the direction toRight (left if toRight=0)
                                //!! only regular lane changes within road; merging/diverging separately!
                                //vehicles[i].currentVelocity = Vector3.zero;
                                vehicles[i].originalPos = vehicles[i].transform.position;
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

        public void UpdateSpeedPositions(float dt)
        {
            foreach (var item in vehicles) {
                if (item.isVirtual) {
                    continue;
                }
                //Debug.Log("U before:" + item.u);
                item.u += Mathf.Max(0, item.speed * dt + 0.5f * item.acc * dt * dt);
                //Debug.Log("U After:" + item.u);
                item.speed = Mathf.Max(item.speed + item.acc * dt, 0);
            }

            SortVehicles();
            UpdateEnvironment();
        }

        public void UpdateBCDown(List<VehicleController> vehiclesWaiting)
        {
            //outflow
            if (vehicles.Count > 0 && vehicles[0].u > pathLength) {
                var vehicle = vehicles[0];
                vehicles.RemoveAt(0);
                vehicle.gameObject.SetActive(false);
                vehiclesWaiting.Add(vehicle);
                SortVehicles();
                UpdateEnvironment();
            }
        }

        public void UpdateBCUp(List<VehicleController> vehiclesWaiting)
        {

            var smin = 15f;
            var currentVehiclesCount = 0;
            foreach (var item in vehicles) {
                if (!item.isVirtual) {
                    ++currentVehiclesCount;
                }
            }

            if (currentVehiclesCount < vehiclesCount && vehiclesWaiting.Count > 0) {
                var succes = false;
                var space = path.pathLength;
                var lane = path.lanesCount - 1;

                var iLead = vehicles.Count - 1;
                while ((iLead > 0) && (vehicles[iLead].lane != lane)) {
                    iLead--;
                }
                if (iLead == -1) {
                    succes = true;
                } else {
                    space = vehicles[iLead].u - vehicles[iLead].length;
                    succes = (space > smin);
                }

                if (!succes) {
                    var spaceMax = 0f;
                    for (var candLane = path.lanesCount - 1; candLane >=0; candLane--) {
                        iLead = vehicles.Count - 1;
                        while ((iLead >=0) && vehicles[iLead].lane != candLane) {
                            iLead--;
                        }
                        space = iLead >= 0 ? vehicles[iLead].u - vehicles[iLead].length : path.pathLength + candLane;
                        if (space > spaceMax) {
                            lane = candLane;
                            spaceMax = space;
                        }
                    }
                }
                succes = space >= smin;
                if (succes) {
                    var speedNew = Mathf.Min(longModelCar.v0, longModelCar.speedLimit, space / longModelCar.T);
                    var vehicle = vehiclesWaiting[0];
                    vehicle.gameObject.SetActive(true);
                    vehicle.Init(lane, speedNew);
                    vehiclesWaiting.Remove(vehicle);
                    vehicles.Add(vehicle);
                }
            }
        }

        public void MergeDiverge(TrafficPathController newPath, float offset, float uBegin, float uEnd, bool isMerge, bool toRight,
            bool ignoreRoute=false, bool prioOther=false, bool prioOwn=false)
        {
            var padding = 0; // visib. extension for orig drivers to target vehs
            var paddingLTC =           // visib. extension for target drivers to orig vehs
            (isMerge && prioOwn) ? 20 : 0;

            var loc_ignoreRoute = ignoreRoute; // default: routes  matter at diverges
            if (isMerge) loc_ignoreRoute = true;  // merging must be always possible

            var loc_prioOther = prioOther;

            var loc_prioOwn =  prioOwn;
            if (loc_prioOwn && loc_prioOther) {
                Debug.Log("road.mergeDiverge: Warning: prioOther and prioOwn" +
                        " cannot be true simultaneously; setting prioOwn=false");
                loc_prioOwn = false;
            }




            // (1) get neighbourhood
            // GetTargetNeighbourhood also sets [this|newPath].iTargetFirst

            var uNewBegin = uBegin + offset;
            var uNewEnd = uEnd + offset;
            var originLane = (toRight) ? path.lanesCount - 1 : 0;
            var targetLane = (toRight) ? 0 : newPath.path.lanesCount - 1;
            var originVehicles = this.GetTargetNeighbourhood(
            uBegin - paddingLTC, uEnd, originLane); // padding only for LT coupling!

            var targetVehicles = newPath.GetTargetNeighbourhood(
            uNewBegin - padding, uNewEnd + padding, targetLane);

            var iMerge = 0; // candidate of the originVehicles neighbourhood
            var uTarget = 0f;  // long. coordinate of this vehicle on the orig road


            // (2) select changing vehicle (if any): 
            // only one at each calling; the first vehicle has priority!

            // (2a) immediate success if no target vehicles in neighbourhood
            // and at least one (real) origin vehicle: the first one changes
            //Debug.Log("targetVehicles.Count: " + targetVehicles.Count);
            //Debug.Log("originVehicles.Count: " + originVehicles.Count);
            var success = ((targetVehicles.Count == 0) && (originVehicles.Count > 0)
                  && !originVehicles[0].isVirtual
                  && (originVehicles[0].u >= uBegin) // otherwise only LT coupl
                  && (loc_ignoreRoute || originVehicles[0].divergeAhead));
            //Debug.Log("success: " + success);
            if (success) { iMerge = 0; uTarget = originVehicles[0].u + offset; }

            // (2b) otherwise select the first suitable candidate of originVehicles

            else if (originVehicles.Count > 0) {

                // initializing of interacting partners with virtual vehicles
                // having no interaction because of their positions
                // default models also initialized in the constructor

                var duLeader = 1000f; // initially big distances w/o interaction
                var duFollower = -1000f;
                var leaderNew = new VehicleController(0, 0, uNewBegin + 10000, targetLane, 0, "car");
                leaderNew.longModel = longModelCar;
                leaderNew.LCModel = LCModelCar;
                var followerNew = new VehicleController(0, 0, uNewBegin - 10000, targetLane, 0, "car");
                followerNew.longModel = longModelCar;
                leaderNew.LCModel = LCModelCar;

                // loop over originVehicles for merging veh candidates
                for (var i = 0; (i < originVehicles.Count) && (!success); i++) {               
                    if (!originVehicles[i].isVirtual
                       && (loc_ignoreRoute || originVehicles[i].divergeAhead)) {

                        //inChangeRegion can be false for LTC since then paddingLTC>0
                        var inChangeRegion = (originVehicles[i].u > uBegin);

                        uTarget = originVehicles[i].u + offset;

                        // inner loop over targetVehicles: search prospective 
                        // new leader leaderNew and follower followerNew and get the gaps
                        // notice: even if there are >0 target vehicles 
                        // (that is guaranteed because of the inner-loop conditions),
                        //  none may be eligible
                        // therefore check for jTarget==-1

                        var jTarget = -1; ;
                        for (var j = 0; j < targetVehicles.Count; j++) {
                            var du = targetVehicles[j].u - uTarget;
                            if ((du > 0) && (du < duLeader)) {
                                duLeader = du; leaderNew = targetVehicles[j];
                            }
                            if ((du < 0) && (du > duFollower)) {
                                jTarget = j; duFollower = du; followerNew = targetVehicles[j];
                            }
                        }


                        // get input variables for MOBIL
                        // qualifiers for state var s,acc: 
                        // [nothing] own vehicle before LC
                        // vehicles: leaderNew, followerNew
                        // subscripts/qualifiers:
                        //   New=own vehicle after LC
                        //   LeadNew= new leader (not affected by LC but acc needed)
                        //   Lag new lag vehicle before LC (only relevant for accLag)
                        //   LagNew=new lag vehicle after LC (for accLagNew)

                        var sNew = duLeader - leaderNew.length;
                        var sLagNew = -duFollower - originVehicles[i].length;
                        var speedLeadNew = leaderNew.speed;
                        var accLeadNew = leaderNew.acc; // leaders=exogen. to MOBIL
                        var speedLagNew = followerNew.speed;
                        var speed = originVehicles[i].speed;

                        var LCModel = (toRight) ? LCModelMandatoryRight
                        : LCModelMandatoryLeft;

                        var vrel = originVehicles[i].speed / originVehicles[i].longModel.v0;

                        var acc = originVehicles[i].acc;
                        var accNew = originVehicles[i].longModel.CalculateAcceleration(
                        sNew, speed, speedLeadNew, accLeadNew);
                        var accLag = followerNew.acc;
                        var accLagNew = originVehicles[i].longModel.CalculateAcceleration(
                        sLagNew, speedLagNew, speed, accNew);




                        // MOBIL decisions
                        var prio_OK = (!loc_prioOther) || loc_prioOwn
                        || (!LCModel.RespectPriority(accLag, accLagNew));

                        var MOBILOK = LCModel.RealizeLaneChange(
                        vrel, acc, accNew, accLagNew, toRight);

                        success = prio_OK && inChangeRegion && MOBILOK
                        && (!originVehicles[i].isVirtual)
                        && (sNew > 0) && (sLagNew > 0);

                        if (success) { iMerge = i; }
                       
                    } // !obstacle

                }// merging veh loop
            }// else branch (there are target vehicles)


            //(3) realize longitudinal-transversal coupling (LTC)
            // exerted onto target vehicles if merge and loc_prioOwn


            if (isMerge && loc_prioOwn) {

                // (3a) determine stop line such that there cannot be a grid lock for any
                // merging vehicle, particularly the longest vehicle

                var vehLenMax = 9;
                var stopLinePosNew = uNewEnd - vehLenMax - 2;
                var bSafe = 4;

                // (3b) all target vehs stop at stop line if at least one origin veh
                // is follower and 
                // the deceleration to do so is less than bSafe
                // if the last orig vehicle is a leader and interacting decel is less,
                // use it

                for (var j = 0; j < targetVehicles.Count; j++) {
                    var sStop = stopLinePosNew - targetVehicles[j].u; // gap to stop for target veh
                    var speedTarget = targetVehicles[j].speed;
                    var accTargetStop = targetVehicles[j].longModel.CalculateAcceleration(sStop, speedTarget, 0, 0);

                    var iLast = -1;
                    for (var i = originVehicles.Count - 1; (i >= 0) && (iLast == -1); i--) {
                        if (!originVehicles[i].isVirtual) { iLast = i; }
                    }

                    if ((iLast > -1) && !targetVehicles[j].isVirtual) {
                        var du = originVehicles[iLast].u + offset - targetVehicles[j].u;
                        var lastOrigIsLeader = (du > 0);
                        if (lastOrigIsLeader) {
                            var s = du - originVehicles[iLast].length;
                            var speedOrig = originVehicles[iLast].speed;
                            var accLTC
                            = targetVehicles[j].longModel.CalculateAcceleration(s, speedTarget, speedOrig, 0);
                            var accTarget = Mathf.Min(targetVehicles[j].acc,
                                       Mathf.Max(accLTC, accTargetStop));
                            if (accTarget > -bSafe) {
                                targetVehicles[j].acc = accTarget;
                            }
                        } else { // if last orig not leading, stop always if it can be done safely
                            if (accTargetStop > -bSafe) {
                                var accTarget = Mathf.Min(targetVehicles[j].acc, accTargetStop);
                                targetVehicles[j].acc = accTarget;
                            }
                        }              
                    }

                }
            }

            //(4) if success, do the actual merging!
            if (success) {// do the actual merging 

                //originVehicles[iMerge]=veh[iMerge+this.iTargetFirst] 

                var iOrig = iMerge + iTargetFirst;
                
                var changingVeh = vehicles[iOrig]; //originVehicles[iMerge];
                var vOld = (toRight) ? targetLane - 1 : targetLane + 1; // rel. to NEW road
                changingVeh.fromRight = !toRight;
                changingVeh.justMerged = true;
                changingVeh.u += offset;
                changingVeh.lane = targetLane;
                changingVeh.laneOld = vOld; // following for  drawing purposes
                changingVeh.v = vOld;  // real lane position (graphical)
                changingVeh.dt_afterLC = 0;             // just changed
                changingVeh.divergeAhead = false; // reset mandatory LC behaviour


                //####################################################################
                vehicles.RemoveRange(iOrig, 1);// removes chg veh from orig.
                newPath.vehicles.Add(changingVeh); // appends changingVeh at last pos;
                                               //####################################################################

                //newPath.nveh=newPath.veh.length;
                newPath.SortVehicles();       // move the mergingVeh at correct position
                newPath.UpdateEnvironment(); // and provide updated neighbors
            }// end do the actual merging
        }

        private List<VehicleController> GetTargetNeighbourhood(float umin, float umax, int targetLane)
        {
            List<VehicleController> targets = new List<VehicleController>();
            var firstTime = true;
            iTargetFirst = 0;
            for (int i=0; i<vehicles.Count; i++) {
                if ((vehicles[i].lane == targetLane) && (vehicles[i].u >= umin) && (vehicles[i].u <= umax)) {
                    if (firstTime) {
                        iTargetFirst = i;
                        firstTime = false;
                    }
                    targets.Add(vehicles[i]);
                }
            }
            return targets;
        }


        public void UpdateFinalPositions()
        {
            var numLanes = path.lanesCount;

            foreach (var item in vehicles) {
                var centerStart = GetPositionFromLongitute(item.u);
                var centerEnd = GetPositionFromLongitute(item.u + 10f);

                var dir = (centerEnd - centerStart).normalized;
                var right = Vector3.Cross(Vector3.up, dir) * path.calculatedWidth;
                var left = -right;
                var seg = (2f * item.lane + 1f) / (2f * path.lanesCount); //lerp value based on lane number               
                var pos = Vector3.Lerp(centerStart + left, centerStart + right, seg); //actualy position based on lane number
                var lookAt = Vector3.Lerp(centerEnd + left, centerEnd + right, seg) + (dir * 30f);
                var duringLC = item.dt_afterLC < item.dt_LC;

                if (duringLC) {
                    if (!item.justMerged) {
                        var oldSeg = (2f * item.laneOld + 1f) / (2f * path.lanesCount);
                        var oldPos = Vector3.Lerp(centerStart + left, centerStart + right, oldSeg);
                        pos = Vector3.Lerp(oldPos, pos, item.dt_afterLC / item.dt_LC);
                    } else {
                        //recalculate path
                        var newWidth = path.unit == Unit.Meters ? path.widthPerLane * (path.lanesCount + 2) : path.widthPerLane * (path.lanesCount + 2) / 3.2808f;
                        right = Vector3.Cross(Vector3.up, dir) * newWidth;
                        left = -right;
                        var oldSeg = (2f * item.lane + 1f) / (2f * (path.lanesCount + 2));
                        var oldPos = Vector3.Lerp(centerStart + left, centerStart + right, oldSeg);
                        pos = Vector3.Lerp(oldPos, pos, item.dt_afterLC / item.dt_LC);
                    }
                } else {
                    item.justMerged = false;
                }
                item.SetPosition(pos);
                item.SetLookAt(lookAt);
                //item.SetPosition(GetPositionFromLongitute(item.u));
                //item.SetLookAt(GetPositionFromLongitute(item.u + 0.1f));
            }
        }

        public void SortVehicles()
        {
            vehicles.Sort();
        }

    }
}