using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CivilFX.TrafficV3 {
    public class ButtonController : MonoBehaviour
    {

        public Button button;
        // Start is called before the first frame update
        void Start()
        {
            button.onClick.AddListener(() => {

                foreach (var item in Resources.FindObjectsOfTypeAll<PathDriver>()) {
                    item.ResetSimulation();
                }

        });
        }

    }
}