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

        public LaneChangingModel LCModelCar;
        public LaneChangingModel LCModelMandatoryRight;
        public LaneChangingModel LCModelMandatoryLeft;

        private List<VehicleController> waitingVehicles;
        // Start is called before the first frame update
        void Awake()
        {
            waitingVehicles = new List<VehicleController>();
            mainPath.Init(prefabs, mainPathVehiclesCount, mainPathObstacles);

            onRampPath.Init(prefabs, onRampPathVehiclesCount, onRampPathObstacles);
        }

        private void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;

            mainPath.UpdateVehiclesModels(LCModelCar);
            onRampPath.UpdateVehiclesModels(LCModelCar);

            onRampPath.SetLCMandatory(0, onRampPath.path.pathLength, true, LCModelMandatoryRight, LCModelMandatoryLeft);

            mainPath.UpdateLastLCTimes(dt);
            mainPath.CalcAccelerations();
            mainPath.ChangeLanes();
            mainPath.UpdateSpeedPositions(dt);
            mainPath.UpdateBCDown(waitingVehicles);
            mainPath.UpdateBCUp(waitingVehicles);
            mainPath.UpdateFinalPositions();

            //onramp
            onRampPath.CalcAccelerations();
            onRampPath.UpdateSpeedPositions(dt);
            onRampPath.UpdateBCUp(waitingVehicles);
            onRampPath.MergeDiverge(mainPath, 36.5326f, 139.5343f, 243.3057f, true, true, LCModelMandatoryRight, LCModelMandatoryLeft);
            onRampPath.UpdateFinalPositions();

        }
    }
}