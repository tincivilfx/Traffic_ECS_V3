//car following model
//IDM : Intelligent Driver Model
using UnityEngine;

namespace CivilFX.TrafficV3
{

    public enum VehicleType
    {
        Car,
        Truck,
        Custom
    }

    [CreateAssetMenu(menuName = "CivilFX/TrafficV3/IDM Parameters", fileName = "New IDM")]
    public class SOIDMParameters : ScriptableObject
    {
        public VehicleType type = VehicleType.Custom;
        public float desiredSpeed; //v0 (km/h)
        public float safetyTime; //T (s)
        public float minGap; //s0 (m)
        public float acceleration; //a (m/s^2)
        public float deceleration; //b (m/s^2)
        public int accelerationExponent; //sigma (const)
    }
}