using System;
using NUnit.Framework;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public static CameraManager Instance => instance;

    [SerializeField] private float normalZ = -10f;
    [SerializeField] private float zoomedOutZ = -30f;
    [SerializeField] private float transitionSpeed = 2f;

    private Camera mainCamera;
    private float targetZ;

    public bool IsZoomedOut { get; private set; }

    private Vector3 currentPos = Vector3.zero;

    public event Action OnZoomOut;
    private bool isChangeZoom = false;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        mainCamera = Camera.main;
        currentPos = mainCamera.transform.position;
        targetZ = normalZ;
    }

    private void Start()
    {
        WaveManager.Instance.LastBossSpawned += OnLastBossSpawned;
        WaveManager.Instance.MiddleBossDefeated += ZoomIn;

        isChangeZoom = false;
    }

    private void OnDisable()
    {
        WaveManager.Instance.LastBossSpawned -= OnLastBossSpawned;
        WaveManager.Instance.MiddleBossDefeated -= ZoomIn;
    }

    private void OnDestroy()
    {
        WaveManager.Instance.LastBossSpawned -= OnLastBossSpawned;
        WaveManager.Instance.MiddleBossDefeated -= ZoomIn;
    }

    private void Update()
    {
        currentPos = mainCamera.transform.position;
        currentPos.z = Mathf.Lerp(currentPos.z, targetZ, Time.deltaTime * transitionSpeed);
        mainCamera.transform.position = currentPos;

        if(isChangeZoom && Mathf.Abs(currentPos.z - targetZ) < 0.01f)
        {
            OnZoomOut?.Invoke();
            isChangeZoom = false;
        }
    }

    public void ZoomOut()
    {
        targetZ = zoomedOutZ;
        IsZoomedOut = true;

        isChangeZoom = true;
    }

    public void ZoomIn()
    {
        targetZ = normalZ;
        IsZoomedOut = false;

        isChangeZoom = true;
    }

    private void OnLastBossSpawned()
    {
        ZoomOut();
    }
}
