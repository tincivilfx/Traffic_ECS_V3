using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ThirtyTwoBitsHash : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for (uint k = 0; k < 1000; k++) {
            uint num = k;
            StringBuilder sb = new StringBuilder("", 50);
            sb.AppendFormat("{0}->", num);
            int generation = 10;
            int i = 0;
            while (i < generation) {

                for (int j = 1; j < sizeof(uint) - 1; j++) {
                    byte left = GetBit(num, j - 1);
                    byte right = GetBit(num, j + 1);
                    byte value = EvaluateRule(left, right);
                    if (value == 0) {
                        ClearBit(ref num, j);
                    } else {
                        SetBit(ref num, j);
                    }
                }
                i++;
            }
            sb.Append(num);
            Debug.Log(sb.ToString());
        }
    }


    private byte EvaluateRule(byte left, byte right)
    {
        if (left == 1 && right == 1) {
            return 0;
        } else {
            return 1;
        }
    }

    private void ClearBit(ref uint value, int bit)
    {
        value &= ~((uint)1 << bit);
    }

    private void SetBit(ref uint value, int bit)
    {
        value |= (uint)1 << bit;
    }

    private byte GetBit(in uint value, int bit)
    {
        return (byte)((value >> bit) & (uint)1);
    }

}
