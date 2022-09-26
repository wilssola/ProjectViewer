using UnityEngine;
using SimpleWebBrowser;

public class CanvasController : MonoBehaviour
{
    public Animator web;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    public WebBrowser2D webBrowser2D;
#endif

    public static CanvasController Instance;

    private HotspotController[] _hotspotControllers;
    
    private readonly int _browser = Animator.StringToHash("Browser");

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _hotspotControllers = FindObjectsOfType<HotspotController>();
    }

    private void Start()
    {
        HideWebBrowser();
    }

    public void ShowWebBrowser(string url)
    {
        if(web == null) return;
        
        web.SetBool(_browser, true);
        
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        webBrowser2D.Navigate(url);
#endif
    }
    
    private void HideWebBrowser()
    {
        if(web == null) return;
        
        web.SetBool(_browser, false);
    }
    
    public void ShowHotspot(string id)
    {
        foreach (var hotspot in _hotspotControllers)
            if (hotspot.id != id)
            {
                hotspot.gameObject.SetActive(false);
                hotspot.visible = false;
            }
    }

    public void HideHotspot()
    {
        ViewController.Instance.UnlerpHotspot(1.5f);
        
        HideWebBrowser();

        foreach (var hotspot in _hotspotControllers)
        {
            hotspot.gameObject.SetActive(true);
            hotspot.visible = true;
        }
    }
}
