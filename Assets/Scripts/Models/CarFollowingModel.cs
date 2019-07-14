using UnityEngine;
namespace CivilFX.TrafficV3 {
    public abstract class CarFollowingModel : ScriptableObject
    {
        [SerializeField]
        public float v0;
        [SerializeField]
        public float T;
        [SerializeField]
        protected float s0;
        [SerializeField]
        protected float a;
        [SerializeField]
        protected float b;
        [SerializeField]
        protected int alpha_v0;
        [SerializeField]
        public float speedLimit;
        [SerializeField]
        protected float speedMax;
        [SerializeField]
        protected int bMax;

        public CarFollowingModel(float _v0, float _T, float _s0, float _a, float _b)
        {
            v0 = _v0;
            T = _T;
            s0 = _s0;
            a = _a;
            b = _b;
            alpha_v0 = 1;
            speedLimit = 1000;
            speedMax = 1000;
        }

        public abstract float CalculateAcceleration(float s, float v, float vl, float al);
        public abstract float CalculateAccGiveWay(float sYield, float sPrio, float v, float vPrio, float accOld);
    }
}