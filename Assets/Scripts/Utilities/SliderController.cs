using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{

    public Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        slider.onValueChanged.AddListener((v) => {

            Time.timeScale = v;

        });
    }
}
