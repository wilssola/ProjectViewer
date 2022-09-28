using Smaa;
using UnityEngine;

public class SwitchSMAA : MonoBehaviour
{
    private SMAA smaa;

    private void Awake()
    {
        smaa = GetComponent<SMAA>();

        if (smaa == null) return;

        if (Application.platform != RuntimePlatform.WebGLPlayer) return;
            
        smaa.enabled = false;
    }
}
