using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player ship - di chuyển WASD/Arrow + bắn Space/Auto
/// Sử dụng New Input System
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private AudioClip shootSound;

    [Header("Bounds")]
    [SerializeField] private float boundaryPadding = 0.5f;

    private Damageable damageable;
    private AudioSource audioSource;
    private float nextFireTime;
    private Camera mainCam;
    private Vector2 screenBoundsMin;
    private Vector2 screenBoundsMax;

    // New Input System
    private Vector2 moveInput;
    private bool isShooting;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        mainCam = Camera.main;
        CalculateBounds();

        if (damageable != null)
        {
            damageable.OnDeath += HandleDeath;
        }
    }

    private void Update()
    {
        // Allow input if GameManager not initialized yet OR state is Playing
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        ReadInput();
        HandleMovement();
        HandleShooting();
    }

    private void ReadInput()
    {
        // New Input System - đọc keyboard trực tiếp
        var kb = Keyboard.current;
        if (kb == null) return;

        float h = 0f;
        float v = 0f;

        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h = -1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h = 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v = 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v = -1f;

        moveInput = new Vector2(h, v);
        isShooting = kb.spaceKey.isPressed || kb.jKey.isPressed;
    }

    private void HandleMovement()
    {
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0f).normalized * moveSpeed * Time.deltaTime;
        transform.position += move;

        // Clamp to screen bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, screenBoundsMin.x, screenBoundsMax.x);
        pos.y = Mathf.Clamp(pos.y, screenBoundsMin.y, screenBoundsMax.y);
        transform.position = pos;
    }

    private void HandleShooting()
    {
        if (isShooting && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Fire()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("PlayerController: projectilePrefab is NULL!");
            return;
        }
        if (shootPoint == null)
        {
            Debug.LogError("PlayerController: shootPoint is NULL!");
            return;
        }

        var bullet = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
        Debug.Log($"FIRED! Bullet spawned at {shootPoint.position}, obj={bullet.name}");

        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound, 0.5f);
        }
    }

    private void HandleDeath()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }
        gameObject.SetActive(false);
    }

    private void CalculateBounds()
    {
        if (mainCam == null) return;
        Vector3 bottomLeft = mainCam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = mainCam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        screenBoundsMin = new Vector2(bottomLeft.x + boundaryPadding, bottomLeft.y + boundaryPadding);
        screenBoundsMax = new Vector2(topRight.x - boundaryPadding, topRight.y - boundaryPadding);
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.OnDeath -= HandleDeath;
    }
}
