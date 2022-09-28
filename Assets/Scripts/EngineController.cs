using UnityEngine;

public class EngineController : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = Application.platform == RuntimePlatform.WebGLPlayer ? 30 : -1;
    }
}
