using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV3
{
    public class TrafficController : MonoBehaviour
    {
        public GameObject[] prefabs;
        


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

            mainPath.UpdateVehiclesModels(LCModelCar);
            onRampPath.UpdateVehiclesModels(LCModelCar);

            onRampPath.SetLCMandatory(0, onRampPath.path.pathLength, true);
            onRampPath2.SetLCMandatory(0, onRampPath2.path.pathLength, false);


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





        }
    }
}