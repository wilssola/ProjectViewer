using System;
using System.Collections;
using UnityEngine;

using TMPro;

using UnityEngine.Rendering.PostProcessing;
using PostProcessing.Runtime;
using UnityFx.Outline;

public class ViewController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float targetDistance = 15f;
    
    [SerializeField] private float orbitMaxRotationSwipe = 180f;
    
    [SerializeField] private float panSpeed = 0.5f;
    
    [SerializeField] private float zoomMin = 0.1f;
    [SerializeField] private float zoomMax = 25f;
    [SerializeField] private float zoomScale = 5f;
    [SerializeField] private float zoomPosition;

    [SerializeField] private bool onOrbit = true;
    [SerializeField] private bool onOrthographic;

    [SerializeField] private TMP_Text controlOrbitText;
    [SerializeField] private TMP_Text controlOrthographicText;
    
    [SerializeField] private OutlineBehaviour outline;
    [SerializeField] private PostProcessVolume volume;

    public static ViewController Instance;
    
    public Camera Camera { private set; get; }
    public bool OnHotspot { private set; get; }

    private Vector3 _lastPosition;
    private Quaternion _lastRotation;
    
    private Vector3 _initialMouseWorldPosition;
    private Vector3 _initialMouseViewportPosition;
    private Vector3 _initialMousePerspectiveWorldPosition;
    
    private PostProcessOutline _outline;
    private MotionBlur _motionBlur;

    private bool canOrbit = true;
    private bool canPan = true;
    private bool canZoom = true;

    private void SetLast()
    {
        _lastPosition = Camera.transform.position;
        _lastRotation = Camera.transform.rotation;
    }
    
    private void GetLast()
    {
        Camera.transform.position = _lastPosition;
        Camera.transform.rotation = _lastRotation;
    }

    private void ClearLast()
    {
        _lastPosition = Vector3.zero;
        _lastRotation = Quaternion.identity;
    }

    public void LerpHotspot(Transform pivot, Transform target, Vector3 offset, float duration)
    {
        SetLast();
        
        Camera.transform.position = pivot.position;
        Camera.transform.LookAt(target);
        Camera.transform.position = pivot.position + offset;

        var newPosition = Camera.transform.position;
        var newRotation = Camera.transform.rotation;

        GetLast();

        OnHotspot = true;
        
        StartCoroutine(LerpPosition(newPosition, duration));
        StartCoroutine(LerpRotation(newRotation, duration));
    }
    
    public void UnlerpHotspot(float duration)
    {
        OnHotspot = false;
        
        StartCoroutine(LerpPosition(_lastPosition, duration));
        StartCoroutine(LerpRotation(_lastRotation, duration));
    }

    public IEnumerator LerpPosition(Vector3 targetPosition, float duration)
    {
        var time = 0f;
        var startPosition = Camera.transform.position;
        while (time < duration)
        {
            Camera.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        Camera.transform.position = targetPosition;
    }

    public IEnumerator LerpRotation(Quaternion targetRotation, float duration)
    {
        var time = 0f;
        var startRotation = Camera.transform.rotation;
        while (time < duration)
        {
            Camera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        Camera.transform.rotation = targetRotation;
    }

    private void EnableOutline(bool active)
    {
        if (outline != null)
            outline.enabled = Application.platform == RuntimePlatform.WebGLPlayer && active;

        if (_outline != null)
            _outline.active = Application.platform == RuntimePlatform.WindowsPlayer && active;
    }
    
    private void EnableBlur(bool active)
    {
        if (_motionBlur != null)
            _motionBlur.active = active;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Camera = Camera.main;
        
        Camera.transform.position = target.transform.position;
        Camera.transform.Translate(new Vector3(0, 0, -targetDistance));
        Camera.transform.LookAt(target);

        targetDistance = Vector3.Distance(target.position, Camera.transform.position);
        
        onOrbit = true;
        onOrthographic = false;

        UpdateOrthographicUI();
        UpdateOrbitUI();

        if (volume != null && volume.profile != null)
        {
            _outline = volume.profile.GetSetting<PostProcessOutline>();
            _motionBlur = volume.profile.GetSetting<MotionBlur>();
            
            EnableOutline(true);
            EnableBlur(true);
        }

        OnHotspot = false;
    }

    private void Update()
    {
        if (OnHotspot) return;
        
        LimitCamera();
        LimitCameraRotation();

        if (Input.touchCount == 0)
        {
            canOrbit = true;
            canPan = true;
            canZoom = true;
            
            Zoom(Input.GetAxis("Mouse ScrollWheel"));
        }

        if (canOrbit)
        {
            if (Input.GetMouseButtonDown(0) && (Input.touchCount != 2 && Input.touchCount != 3))
            {
                _initialMouseViewportPosition = Camera.ScreenToViewportPoint(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0) && (Input.touchCount != 2 && Input.touchCount != 3))
            {
                Orbit();

                canPan = false;
                canZoom = false;
            }
        }

        if (Input.touchCount == 2)
        {
            var touchZero = Input.GetTouch(0);
            var touchOne = Input.GetTouch(1);

            var touchZeroInitialPosition = touchZero.position - touchZero.deltaPosition;
            var touchOneInitialPosition = touchOne.position - touchOne.deltaPosition;

            var initialMagnitude = (touchZeroInitialPosition - touchOneInitialPosition).magnitude;
            var magnitude = (touchZero.position - touchOne.position).magnitude;

            var difference = magnitude - initialMagnitude;

            if (difference != 0)
            {
                canOrbit = false;
                canPan = false;
            }
            
            Zoom(difference * 0.01f);
        }

        if ((Input.GetMouseButtonDown(1) && Input.touchCount == 0) || (Input.GetMouseButtonDown(0) && Input.touchCount == 3))
        {
            // Orthographic
            _initialMouseWorldPosition = Camera.ScreenToWorldPoint(Input.mousePosition);
            
            // Perspective
            _initialMousePerspectiveWorldPosition = GetPerspectiveWorldPosition(0);
        }
        else if ((Input.GetMouseButton(1) && Input.touchCount == 0) || (Input.GetMouseButton(0) && Input.touchCount == 3))
        {
            Pan();

            canOrbit = false;
            canZoom = false;
        }

        LimitCamera();
        LimitCameraRotation();
    }

    private void LateUpdate()
    {
        LimitCamera();
        LimitCameraRotation();
    }
    
    private void FixedUpdate()
    {
        LimitCamera();
        LimitCameraRotation();
    }

    private void LimitCameraRotation()
    {
        return;
        
        var angles = Camera.transform.rotation.eulerAngles;
        angles.x = Math.Clamp(angles.x, 0f, 90f);
        Camera.transform.rotation = Quaternion.Euler(angles);
    }

    private void LimitCamera()
    {
        const float minHeight = 1f;

        var maxPosition = zoomMax;

        var cameraPosition = Camera.transform.position;

        if (cameraPosition.y < minHeight)
            Camera.transform.position = new Vector3(cameraPosition.x, 1f, cameraPosition.z);
        else if (cameraPosition.y > maxPosition)
            Camera.transform.position = new Vector3(cameraPosition.x, maxPosition, cameraPosition.z);
        
        if (cameraPosition.x < -maxPosition)
            Camera.transform.position = new Vector3(-maxPosition, cameraPosition.y, cameraPosition.z);
        else if (cameraPosition.x > maxPosition)
            Camera.transform.position = new Vector3(maxPosition, cameraPosition.y, cameraPosition.z);

        if (cameraPosition.z < -maxPosition)
            Camera.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, -maxPosition);
        else if (cameraPosition.z > maxPosition)
            Camera.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, maxPosition);

        var cameraRotation = Camera.transform.rotation.eulerAngles;
        
        if (cameraRotation.z != 0)
            Camera.transform.rotation = Quaternion.Euler(cameraRotation.x, cameraRotation.y, 0f);
    }

    private void CheckLimitCamera(Vector3 target, Vector3 previous)
    {
        const float minHeight = 1f;
        
        var maxPosition = zoomMax * (onOrthographic ? 1.5f : 1f);
        
        if (target.y < minHeight || target.y > maxPosition || target.x < -maxPosition || target.x > maxPosition || target.z < -maxPosition || target.z > maxPosition)
            Camera.transform.position = previous;
    }

    public void SwitchTop()
    {
        if (OnHotspot) return;
        
        if (target == null) return;
        
        StartCoroutine(LerpPosition(target.position + new Vector3(0, zoomMax, 0), 1f));
        StartCoroutine(LerpRotation(Quaternion.Euler(new Vector3(90, 0, 0)), 1f));

        SetOrbit(false);
    }
    
    private void SetOrthographic(bool onOrthographic)
    {
        if (OnHotspot)
        {
            Camera.orthographic = false;
            this.onOrthographic = false;
            
            return;
        }
        
        Camera.orthographic = onOrthographic;
        this.onOrthographic = Camera.orthographic;

        if (onOrthographic)
            if(Vector3.Distance(Camera.transform.position, target.transform.position) < zoomMax)
                Camera.transform.Translate(new Vector3(0, 0, -zoomMax));
        
        EnableOutline(!onOrthographic);
        EnableBlur(!onOrthographic);
        
        SetOrbit(!onOrthographic);

        UpdateOrthographicUI();
    }

    private void UpdateOrthographicUI()
    {
        if(controlOrthographicText != null)
            controlOrthographicText.text = "ORTHOGRAPHIC CAMERA - " + (onOrthographic ? "ON" : "OFF");
    }

    public void SwitchOrthographic()
    {
        SetOrthographic(!onOrthographic);
    }

    private void Orbit()
    {
        // Obter o novo vetor posição do mouse/dedo na tela/camera.
        var newViewportPosition = Camera.ScreenToViewportPoint(Input.mousePosition);
        // Subtrair o novo vetor pelo inicial para obter a direção contrária ao movimento feito pelo mouse/dedo.
        var directionViewportPosition = _initialMouseViewportPosition - newViewportPosition;
        
        // Sabendo que a tela/viewport tem um tamanho de 1x1 em coordenadas de viewport, um arrasto da esquerda para a
        // direita da tela terá uma distância de 1 unidade viewport, assim, temos uma relação de 1 unidade viewport = PI.
        // Assim, cada unidade de viewport será multiplicada por orbitMaxRotationSwipe para realizar a conversão de
        // unidade viewport para radianos desejado.
        var horizontalInput = -directionViewportPosition.x * orbitMaxRotationSwipe;
        var verticalInput = directionViewportPosition.y * orbitMaxRotationSwipe;

        if (onOrbit)
            if (target != null)
                Camera.transform.position = target.position;
        
        Camera.transform.Rotate(new Vector3(1, 0, 0), verticalInput);
        Camera.transform.Rotate(new Vector3(0, 1, 0), horizontalInput, Space.World);
        
        if (onOrbit)
            Camera.transform.Translate(new Vector3(0, 0, -targetDistance));
        
        // Salvar nova posição para comparar o movimento a cada quadro que o função for chamada.
        _initialMouseViewportPosition = newViewportPosition;
        
        LimitCamera();
        LimitCameraRotation();
    }

    private void SetOrbit(bool onOrbit)
    {
        this.onOrbit = onOrbit && !OnHotspot && !onOrthographic;

        if (onOrbit)
            Orbit();

        UpdateOrbitUI();
    }

    private void UpdateOrbitUI()
    {
        if(controlOrbitText != null)
            controlOrbitText.text = "ORBITAL CAMERA - " + (onOrbit ? "ON" : "OFF");
    }

    public void SwitchOrbit()
    {
        SetOrbit(!onOrbit);
    }

    private void Pan()
    {
        var horizontalInput = Input.GetAxis("Mouse X");
        var verticalInput = Input.GetAxis("Mouse Y");
            
        if (horizontalInput == 0 && verticalInput == 0) return;

        var speedChangeByDistance = Math.Clamp(panSpeed * (targetDistance / 10f), panSpeed / 2f, panSpeed * 1.25f);

        // Orthographic
        var directionWorldPosition =
            _initialMouseWorldPosition - Camera.ScreenToWorldPoint(Input.mousePosition);
        Camera.transform.position += directionWorldPosition * speedChangeByDistance;

        // Perspective
        var directionPerspectiveWorldPosition =
            _initialMousePerspectiveWorldPosition - GetPerspectiveWorldPosition(0);
        Camera.transform.position += directionPerspectiveWorldPosition * speedChangeByDistance;
        
        LimitCamera();

        SetOrbit(false);
    }

    private void Zoom(float zoomDifference)
    {
        if (zoomDifference == 0) return;

        var scaleChangeByDistance = Math.Clamp(zoomScale * (targetDistance / 10f) / 10f, zoomScale / 2f, zoomScale * 1.25f);

        zoomPosition = Mathf.MoveTowards(zoomPosition, scaleChangeByDistance, 2.5f * Time.deltaTime);
        
        // Orthographic
        if (Camera.orthographic)
        {
            _initialMouseWorldPosition = Camera.ScreenToWorldPoint(Input.mousePosition);

            var directionWorldPosition =
                _initialMouseWorldPosition - Camera.ScreenToWorldPoint(Input.mousePosition);
            transform.position += directionWorldPosition;
            
            Camera.orthographicSize =
                Mathf.Clamp(Camera.orthographicSize - zoomDifference * zoomPosition, zoomMin, zoomMax);
        }

        // Perspective
        const float maxDifferenceEnterDistanceAndZoomToBeEquals = 0.1f;
        var canZoom = true;
        var oldDistance = Math.Round(Vector3.Distance(Camera.transform.position, target.position));
        var lockZoomMinCondition = zoomDifference > 0 && (Math.Abs(oldDistance - zoomScale) < maxDifferenceEnterDistanceAndZoomToBeEquals || Math.Abs(oldDistance - zoomMin) < maxDifferenceEnterDistanceAndZoomToBeEquals);
        var lockZoomMaxCondition = zoomDifference < 0 && Math.Abs(oldDistance - zoomMax) < maxDifferenceEnterDistanceAndZoomToBeEquals;
        
        if(onOrbit && (lockZoomMinCondition || lockZoomMaxCondition))
            canZoom = false;

        if (canZoom)
        {
            var previousPosition = Camera.transform.position;
            Camera.transform.position += Camera.transform.forward * zoomDifference * zoomPosition * (Camera.orthographic ? (zoomDifference > 0 ? 0.1f : 2f) : 1f);
            
            var targetPosition = Camera.transform.position;
            CheckLimitCamera(targetPosition, previousPosition);
        }

        LimitCamera();

        targetDistance = Vector3.Distance(Camera.transform.position, target.position);
    }
    
    private Vector3 GetPerspectiveWorldPosition(float z){
        var mousePosition = Camera.ScreenPointToRay(Input.mousePosition);
        var ground = new Plane(Camera.transform.forward, new Vector3(0, 0, z));
        
        ground.Raycast(mousePosition, out var distanceToGround);
        
        return mousePosition.GetPoint(distanceToGround);
    }
}
