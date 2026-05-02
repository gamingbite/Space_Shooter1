using UnityEngine;

/// <summary>
/// Projectile - bay theo direction, gây damage khi va chạm.
/// - Đạn PLAYER: spawn ExplosionVFX nhỏ khi trúng enemy/meteor
/// - Đạn ENEMY: spawn ExplosionVFX khi trúng player
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private Vector2 direction = Vector2.up;
    [SerializeField] private bool isEnemyProjectile = false;

    // Cache prefab - load 1 lần dùng mãi
    private static GameObject _playerHitVFX;   // Dùng cho đạn player trúng enemy
    private static GameObject _enemyHitVFX;    // Dùng cho đạn enemy trúng player

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }

    public void SetAsEnemyProjectile()
    {
        isEnemyProjectile = true;
    }

    /// <summary>
    /// Spawn ExplosionVFX tại vị trí va chạm
    /// scale: 1.0 = kích thước bình thường, < 1 = nhỏ hơn
    /// </summary>
    private void SpawnHitVFX(Vector3 position, float scale = 1f)
    {
        // Load từ Resources/Effects/ExplosionVFX
        if (!isEnemyProjectile)
        {
            if (_playerHitVFX == null)
                _playerHitVFX = Resources.Load<GameObject>("Effects/ExplosionVFX");
        }
        else
        {
            if (_enemyHitVFX == null)
                _enemyHitVFX = Resources.Load<GameObject>("Effects/ExplosionVFX");
        }

        var prefab = isEnemyProjectile ? _enemyHitVFX : _playerHitVFX;
        if (prefab == null) return;

        var fx = Instantiate(prefab, position, Quaternion.identity);
        if (scale != 1f)
            fx.transform.localScale = Vector3.one * scale;

        // AutoDestroy sẽ tự hủy, backup 2s
        Destroy(fx, 2f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isEnemyProjectile)
        {
            // Đạn enemy → chỉ damage player
            if (!other.CompareTag("Player")) return;

            var damageable = other.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                SpawnHitVFX(other.transform.position, 0.6f); // Nổ nhỏ khi player bị trúng
                Destroy(gameObject);
            }
        }
        else
        {
            // Đạn player → không damage player, chỉ damage enemy/meteor
            if (other.CompareTag("Player")) return;

            var damageable = other.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                SpawnHitVFX(other.transform.position, 0.5f); // Nổ nhỏ khi bắn trúng enemy
                Destroy(gameObject);
            }
        }
    }
}
