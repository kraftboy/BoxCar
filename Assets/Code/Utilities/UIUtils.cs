using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class UIUtils
{
    public static void ScreenToOffset(Camera camera, int bottomLeftX, int bottomLeftY, int topRightX, int topRightY, ref RectTransform outRect)
    {
        outRect.offsetMin = new Vector2(bottomLeftX, bottomLeftY);
        outRect.offsetMax = new Vector2(camera.pixelWidth - topRightX, camera.pixelHeight - topRightY);
    }

    public static void MoveToLayer(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        foreach (Transform child in root)
            MoveToLayer(child, layer);
    }
}
