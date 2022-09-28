using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SwitchPostProcessProfile : MonoBehaviour
{
    public PostProcessProfile Windows, WebGL;

    private PostProcessVolume _postProcessVolume;
    
    private void Awake()
    {
        _postProcessVolume = GetComponent<PostProcessVolume>();
        
        if(_postProcessVolume == null) return;
        
#if UNITY_STANDALONE_WIN
        _postProcessVolume.profile = Windows;
#endif

#if UNITY_WEBGL
        _postProcessVolume.profile = WebGL;
#endif
    }
}
