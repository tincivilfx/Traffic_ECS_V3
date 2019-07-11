using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV3
{
    [CreateAssetMenu(menuName = "CivilFX/TrafficV3/Models/LaneChanging/MOBIL", fileName = "New MOBIL")]
    public class MOBIL : LaneChangingModel
    {       
        public MOBIL() : base (4f, 20f, 0.1f, 0.2f, 0.3f)
        {

        }

        public MOBIL(float _bSafe, float _bSafeMax, float _p, float _bThr, float _bBiasRight) : base (_bSafe, _bSafeMax, _p, _bThr, _bBiasRight)
        {

        }

        public override bool RealizeLaneChange(float vrel, float acc, float accNew, float accLagNew, bool toRight)
        {
            var signRight = (toRight) ? 1 : -1;

            // safety criterion

            var bSafeActual = vrel * bSafe + (1 - vrel) * bSafeMax;
            //if(accLagNew<-bSafeActual){return false;} //!! <jun19
            //if((accLagNew<-bSafeActual)&&(signRight*this.bBiasRight<41)){return false;}//!!! override safety criterion to really enforce overtaking ban OPTIMIZE
            if (signRight * bBiasRight > 40) {
                return true;
            }

            if (accLagNew < Mathf.Min(-bSafeActual, -Mathf.Abs(bBiasRight))) { return false; }//!!!

            // incentive criterion
            var dacc = accNew - acc + p * accLagNew //!! new
            + bBiasRight * signRight - bThr;

            // hard-prohibit LC against bias if |bias|>9 m/s^2
            if (bBiasRight * signRight < -9) { dacc = -1; }

            return (dacc > 0);
        }

        public override bool RespectPriority(float accLag, float accLagNew)
        {
            return (accLag - accLagNew > 0.1);
        }
    }
}