//Lane changing model
//MOBIL: 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CivilFX.TrafficV3
{
    [CreateAssetMenu(menuName = "CivilFX/TrafficV3/MOBIL Parameters", fileName = "New MOBIL")]
    public class SOMOBILParameters : ScriptableObject
    {
        public float bSafe = 4f;
        public float bSafeMax = 20f;
        public float p = 0.1f; //politeness
        public float bThr = 0.2f; //b threshold
        public float biasRight = 0.3f; //
    }
}