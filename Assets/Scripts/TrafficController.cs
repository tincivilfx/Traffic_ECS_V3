using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV3
{

    [System.Serializable]
    public class RampInfo
    {
        public TrafficPathController newPath;
        public int targetLane = -1;
        public float offset;
        public float umin;
        public float umax;
        public bool isMerge; //!isMerge means diverge
        public bool toRight; //!toright means toLeft       
    }


    [System.Serializable]
    public class PathInfo
    {
        public TrafficPathController path;
        public VehicleController[] obstacles;
        public int vehiclesCount;
        public bool allowLaneChaning;
        public bool allowRespawning;
        public bool allowDespawning;
        public RampInfo [] rampInfos;
    }

    public class TrafficController : MonoBehaviour
    {
        public GameObject[] prefabs;

        public PathInfo[] pathInfos;

        public CarFollowingModel longModelCar;
        public LaneChangingModel LCModelCar;
        public LaneChangingModel LCModelMandatoryRight;
        public LaneChangingModel LCModelMandatoryLeft;

        private List<VehicleController> waitingVehicles;
        // Start is called before the first frame update
        void Awake()
        {
            waitingVehicles = new List<VehicleController>();
            foreach (var item in pathInfos) {
                item.path.Init(prefabs, item.vehiclesCount, item.obstacles);
                item.path.SetModels(longModelCar, longModelCar, LCModelCar, LCModelCar, LCModelMandatoryRight, LCModelMandatoryLeft);
            }
        }

        private void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;         
            foreach (var item in pathInfos) {

                item.path.UpdateVehiclesModels(LCModelCar);
                item.path.CalcAccelerations();
                item.path.UpdateLastLCTimes(dt);
                if (item.allowLaneChaning) {                 
                    item.path.ChangeLanes();
                }
                
                item.path.UpdateSpeedPositions(dt);

                if (item.allowDespawning) {
                    item.path.UpdateBCDown(waitingVehicles);
                }
                
                if (item.allowRespawning) {
                    item.path.UpdateBCUp(waitingVehicles);
                }
                //merge/diverse
                foreach (var item2 in item.rampInfos) {
                    item.path.MergeDiverge(item2.newPath, item2.offset, item2.umin, item2.umax, item2.isMerge, item2.toRight, item2.targetLane ,true);
                }
                item.path.UpdateFinalPositions();
            }         
        }
    }
}