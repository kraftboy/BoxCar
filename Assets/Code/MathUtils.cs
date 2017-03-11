using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class MathUtils
{
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }
}

