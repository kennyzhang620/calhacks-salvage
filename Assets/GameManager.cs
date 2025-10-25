using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public static class GameManager
{
    public static int ObjectsActive = 0;
    public static int ObjectIndex = -1;
    public static int ObjectStored = -1;
    public static int SpawnLimit = 6;
    public static Dictionary<string, GameObject> Objects = new Dictionary<string, GameObject>();

    public static Vector3 anchorLocation;

    public static GameObject CheckDuplicate(Vector3 v, float radi)
    {
        foreach (var vr in Objects)
        {
         //   Debug.Log((vr.Value.transform.position - v).magnitude);
            if (vr.Value && ((vr.Value.transform.position - v).magnitude < radi))
                return vr.Value;
        }

        return null;
    }

}
