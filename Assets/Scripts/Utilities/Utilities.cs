using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities 
{
    public static float Map(float value, float lowerLimit, float uperLimit, float lowerValue, float uperValue)
    {
        return lowerValue + ((uperValue - lowerValue) / (uperLimit - lowerLimit)) * (value - lowerLimit);
    }
}
