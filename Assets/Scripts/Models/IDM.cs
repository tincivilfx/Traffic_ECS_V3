using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV3
{
    [CreateAssetMenu(menuName = "CivilFX/TrafficV3/Models/CarFollowing/IDM", fileName = "New IDM")]
    public class IDM : CarFollowingModel
    {

        //added empty constructor for asset to be created correctly
        //from the menu
        public IDM() : base(20f, 1.3f, 2f, 1f, 2f)
        {
            bMax = 16;
        }
        public IDM(float _v0, float _T, float _s0, float _a, float _b) : base(_v0, _T, _s0, _a, _b)
        {
            bMax = 16;
        }
        public override float CalculateAcceleration(float s, float v, float vl, float al)
        {
            var noiseAcc = 0.3f;
            var accRnd = noiseAcc * (Random.Range(float.MinValue, float.MaxValue) - 0.5f);

            var v0eff = Mathf.Min(v0, speedLimit, speedMax);
            v0eff *= alpha_v0;

            var accFree = (v < v0eff) ? a * (1 - Mathf.Pow(v0 / v0eff, 4)) : a * (1 - v / v0eff);
            var sstar = s0 + Mathf.Max(0f, v * T + (0.5f * v * (v - vl) / Mathf.Sqrt(a * b)));
            var accInt = -a * Mathf.Pow(sstar / Mathf.Max(s, s0), 2);
            var accInt_IDMplus = accInt + a;
            return (v0eff < 0.00001) ? 0 : Mathf.Max(-bMax, accFree + accInt + accRnd);
        }

        public override float CalculateAccGiveWay(float sYield, float sPrio,float v, float vPrio, float accOld)
        {
            sPrio = 0f; //reminder: this argument is not used in this model
            var accNew = CalculateAcceleration(sYield, v, vPrio, 0);
            return (accNew > -2 * b) ? accNew : accOld;
        }

    }
}