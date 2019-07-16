using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV3
{

    [System.Serializable]
    public class RampInfo
    {
        public TrafficPathController newPath;
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
        public RampInfo [] rampInfos;
    }


    public class TrafficController : MonoBehaviour
    {
        public GameObject[] prefabs;

        public PathInfo[] pathInfos;


        public TrafficPathController mainPath;
        public VehicleController[] mainPathObstacles;
        public int mainPathVehiclesCount;

        public TrafficPathController onRampPath;
        public VehicleController[] onRampPathObstacles;
        public int onRampPathVehiclesCount;

        public TrafficPathController onRampPath2;
        public VehicleController[] onRampPath2Obstacles;
        public int onRampPath2VehiclesCount;


        public TrafficPathController offRampPath;


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
                item.path.Init(prefabs, mainPathVehiclesCount, mainPathObstacles);
                item.path.SetModels(longModelCar, longModelCar, LCModelCar, LCModelCar, LCModelMandatoryRight, LCModelMandatoryLeft);
            }

            mainPath.Init(prefabs, mainPathVehiclesCount, mainPathObstacles);
            mainPath.SetModels(longModelCar, longModelCar, LCModelCar, LCModelCar, LCModelMandatoryRight, LCModelMandatoryLeft);

            onRampPath.Init(prefabs, onRampPathVehiclesCount, onRampPathObstacles);
            onRampPath.SetModels(longModelCar, longModelCar, LCModelCar, LCModelCar, LCModelMandatoryRight, LCModelMandatoryLeft);

            onRampPath2.Init(prefabs, onRampPath2VehiclesCount, onRampPath2Obstacles);
            onRampPath2.SetModels(longModelCar, longModelCar, LCModelCar, LCModelCar, LCModelMandatoryRight, LCModelMandatoryLeft);

            /*
            offRampPath.Init(prefabs, 0, null); //we dont want vehicles to respawn on this path
            offRampPath.SetModels(longModelCar, longModelCar, LCModelCar, LCModelCar, LCModelMandatoryRight, LCModelMandatoryLeft);
            */
        }

        private void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;


            foreach (var item in pathInfos) {
                item.path.UpdateVehiclesModels(LCModelCar);

                item.path.CalcAccelerations();

                if (item.allowLaneChaning) {
                    item.path.UpdateLastLCTimes(dt);
                    item.path.ChangeLanes();
                }

                item.path.UpdateSpeedPositions(dt);

                item.path.UpdateBCDown(waitingVehicles);

                if (item.allowRespawning) {
                    item.path.UpdateBCUp(waitingVehicles);
                }

                //merge/diverse
                foreach (var item2 in item.rampInfos) {
                    item.path.MergeDiverge(item2.newPath, item2.offset, item2.umin, item2.umax, item2.isMerge, item2.toRight);
                }
                item.path.UpdateFinalPositions();

            }

            /*
            mainPath.UpdateVehiclesModels(LCModelCar);
            onRampPath.UpdateVehiclesModels(LCModelCar);

            //onRampPath.SetLCMandatory(0, onRampPath.path.pathLength, true);
            //onRampPath2.SetLCMandatory(0, onRampPath2.path.pathLength, false);


            //main path
            mainPath.UpdateLastLCTimes(dt);
            mainPath.CalcAccelerations();
            mainPath.ChangeLanes();
            mainPath.UpdateSpeedPositions(dt);
            mainPath.UpdateBCDown(waitingVehicles);
            mainPath.UpdateBCUp(waitingVehicles);
            //mainPath.MergeDiverge(offRampPath, -759.6795f, 759.6795f, 800f, false, true);
            mainPath.UpdateFinalPositions();

            //onramp2
            onRampPath2.CalcAccelerations();
            onRampPath2.UpdateSpeedPositions(dt);
            onRampPath2.UpdateBCUp(waitingVehicles);
            onRampPath2.MergeDiverge(mainPath, 478.457f, 526.588f, 589.5271f, true, false);
            onRampPath2.UpdateFinalPositions();

            //onramp
            onRampPath.CalcAccelerations();
            onRampPath.UpdateSpeedPositions(dt);
            onRampPath.UpdateBCUp(waitingVehicles);
            onRampPath.MergeDiverge(mainPath, 34.5326f, 148.2116f, 243.3057f, true, true);
            onRampPath.UpdateFinalPositions();
            */




        }
    }
}