using UnityEngine;

/// <summary>
/// Background scroll từ trên xuống dưới vô hạn.
/// Cần 2 GameObject (BG_A, BG_B) với cùng Sprite, cùng script này.
/// BG_A bắt đầu ở y=0, BG_B bắt đầu ở y = spriteHeight.
/// Khi một tấm chạy quá -spriteHeight thì reset lên trên.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 1.5f;

    // Chiều cao của sprite tính bằng world units (tự động tính)
    private float _spriteHeight;
    private bool _initialized;

    private void Start()
    {
        InitHeight();
    }

    private void InitHeight()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            // bounds.size đã nhân scale
            _spriteHeight = sr.bounds.size.y;
        }
        else
        {
            _spriteHeight = 12.8f; // fallback
        }
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized) return;

        // Scroll xuống dưới
        transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

        // Khi tấm này ra khỏi màn hình phía dưới → nhảy lên trên
        // Tấm partner đang ở phía trên, khi tấm này reset lên là seamless
        if (transform.position.y <= -_spriteHeight)
        {
            float newY = transform.position.y + _spriteHeight * 2f;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

#if UNITY_EDITOR
    // Hiển thị bounds trong Scene View để debug
    private void OnDrawGizmosSelected()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, sr.bounds.size);
    }
#endif
}
