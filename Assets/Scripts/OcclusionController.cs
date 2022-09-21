using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionController : MonoBehaviour
{
    public HardwareOcclusion hardwareOcclusion;

    public Transform target;
    
    private void Awake()
    {
        var tfs = target.GetComponentsInChildren<Transform>();
        var gos = new GameObject[tfs.Length];
        for (var i = 0; i < tfs.Length; i++)
        {
            gos[i] = tfs[i].gameObject;
        }
        
        hardwareOcclusion.Targets = gos;
    }
}
