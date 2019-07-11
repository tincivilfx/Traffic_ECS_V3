using Unity.Mathematics;
using UnityEngine;

namespace CivilFX.TrafficV3
{
    [CreateAssetMenu(menuName = "CivilFX/TrafficV3/Models/CarFollowing/ACC", fileName = "New ACC")]
    public class ACC : CarFollowingModel
    {
        [SerializeField]
        private float cool;

        public ACC() : base(20f, 1.3f, 2f, 1f, 2f)
        {
            bMax = 18;
        }

        public ACC(float _v0, float _T, float _s0, float _a, float _b) : base(_v0, _T, _s0, _a, _b)
        {
            bMax = 18;
        }

        public override float CalculateAcceleration(float s, float v, float vl, float al)
        {
            if (s < 0.001) { return -bMax; }// particularly for s<0

            // acceleration noise to avoid some artifacts (no noise if s<s0)
            // sig_speedFluct=noiseAcc*sqrt(t*dt/12)

            var noiseAcc = (s < s0) ? 0f : 0.3f;
            var accRnd = noiseAcc * (UnityEngine.Random.Range(0f, 1f) - 0.5f);

            // determine valid local v0

            var v0eff = Mathf.Min(v0, speedLimit, speedMax);
            v0eff *= alpha_v0;

            // actual acceleration model

            var accFree = (v < v0eff) ? a * (1 - Mathf.Pow(v / v0eff, 4))
            : a * (1 - v / v0eff);
            var sstar = s0
            + Mathf.Max(0, v * T + 0.5f * v * (v - vl) / Mathf.Sqrt(a * b));
            var accInt = -a * Mathf.Pow(sstar / Mathf.Max(s, s0), 2);
            var accIDM = accFree + accInt;

            var accCAH = (vl * (v - vl) < -2 * s * al)
            ? v * v * al / (vl * vl - 2 * s * al)
            : al - Mathf.Pow(v - vl, 2) / (2 * Mathf.Max(s, 0.01f)) * ((v > vl) ? 1 : 0);
            accCAH = Mathf.Min(accCAH, a);

            var accMix = (accIDM > accCAH) ? accIDM : accCAH + b * math.tanh((accIDM - accCAH) / b);
            var arg = (accIDM - accCAH) / b;

            var accACC = cool * accMix + (1 - cool) * accIDM;

            var accReturn = (v0eff < 0.00001) ? 0 : Mathf.Max(-bMax, accACC + accRnd);

            return accReturn;
        }

        public override float CalculateAccGiveWay(float sYield, float sPrio, float v, float vPrio, float accOld)
        {
            var accPrioNoYield = this.CalculateAcceleration(sPrio, vPrio, 0, 0);
            var accYield = this.CalculateAcceleration(sYield, v, 0, 0);
            var priorityRelevant = ((accPrioNoYield < -0.2 * this.b)
                      && (accYield < -0.2 * this.b));
            var accGiveWay = priorityRelevant ? accYield : accOld;
            return accGiveWay;
        }
    }
}