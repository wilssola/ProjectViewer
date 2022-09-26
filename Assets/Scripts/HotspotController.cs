using System;
using UnityEngine;

using UnityTimer;
using TMPro;

public class HotspotController : MonoBehaviour
{
    public bool visible = true;
    
    public string id { private set; get; }
    
    public string name, url;

    public Transform target, pivot;

    public Canvas canvas;
    public SpriteRenderer sprite;
    public SpriteGlow.SpriteGlowEffect glow;

    public Animator animator;

    public TMP_Text text;

    public Vector3 cameraOffset;

    public LayerMask layerMask;
    
    private readonly int _hotspot = Animator.StringToHash("Hotspot");

    private void OnBecameInvisible()
    {
        canvas.enabled = false;
    }

    private void OnBecameVisible()
    {
        canvas.enabled = true;
    }

    private void Start()
    {
        if (canvas != null)
        {
            canvas.worldCamera = Camera.main;
        }

        if (text != null)
            text.text = name;

        id = Guid.NewGuid().ToString();
    }

    private void LateUpdate()
    {
        if (ViewController.Instance.Camera == null) return;

        HotspotLookAt();

        PreventHotspotWall();
    }

    private void HotspotLookAt()
    {
        if (!ViewController.Instance.Camera.orthographic)
        {
            if (!canvas.enabled)
            {
                canvas.enabled = true;
                sprite.enabled = true;
                glow.enabled = true;
            }

            canvas.transform.LookAt(2f * canvas.transform.position - ViewController.Instance.Camera.transform.position);
        }
        else
        {
            canvas.enabled = false;
            sprite.enabled = false;
            glow.enabled = false;
        }
    }

    private bool CheckIsVisible(RaycastHit hit)
    {
        return hit.transform.CompareTag($"CameraCollider");
    }

    private void PreventHotspotWall()
    {
        var cameraDirection = ViewController.Instance.Camera.transform.position - transform.position;

        if (Physics.Raycast(transform.position, cameraDirection, out var hit, int.MaxValue, layerMask))
        {
            var isVisible = CheckIsVisible(hit);

            if (!isVisible)
            {
                if (Physics.Raycast(transform.position, cameraDirection, out var hit1, int.MaxValue, layerMask))
                {
                    isVisible = CheckIsVisible(hit);
                }

                Debug.DrawLine(transform.position, hit1.point, Color.red);
            }

            HideHotspotWall(isVisible);
        }

        Debug.DrawLine(transform.position, hit.point, Color.blue);
    }
    
    private void HideHotspotWall(bool isVisible)
    {
        if (isVisible)
        {
            if (ViewController.Instance.Camera.orthographic) return;
            if (!visible) return;
            
            canvas.enabled = true;
            sprite.enabled = true;
            glow.enabled = true;
            animator.SetBool(_hotspot, true);
        }
        else
        {
            //canvas.enabled = false;
            animator.SetBool(_hotspot, false);
            Timer.Register(0.5f, () =>
            {
                canvas.enabled = false;
                sprite.enabled = false;
                glow.enabled = false;
            });
        }
    }

    public void ShowHotspot()
    {
        ViewController.Instance.LerpHotspot(pivot, target, cameraOffset, 1f);

        Timer.Register(0.5f, () => CanvasController.Instance.ShowWebBrowser(url));

        CanvasController.Instance.ShowHotspot(id);
    }
}
