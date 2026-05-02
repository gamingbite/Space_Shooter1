using UnityEngine;

/// <summary>
/// Bridge: Nhận TakeDamage từ Damageable (được gọi bởi Projectile)
/// và forward sang BossController.TakeDamage()
/// Gắn tự động bởi WaveSpawner khi boss spawn
/// </summary>
public class BossDamageProxy : MonoBehaviour
{
    private BossController boss;
    private Damageable damageable;
    private bool initialized = false;

    public void Init(BossController bossCtrl, Damageable dmg)
    {
        boss = bossCtrl;
        damageable = dmg;
        initialized = true;

        // Subscribe để intercept damage
        if (damageable != null)
            damageable.OnHealthChanged += OnDamageableHealthChanged;
    }

    private float lastHealth = -1f;

    private void OnDamageableHealthChanged(float current, float max)
    {
        if (!initialized || boss == null) return;

        // Tính damage từ delta HP
        if (lastHealth < 0f)
        {
            lastHealth = current;
            return;
        }

        float damage = lastHealth - current;
        if (damage > 0f)
            boss.TakeDamage(damage);

        lastHealth = current;
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.OnHealthChanged -= OnDamageableHealthChanged;
    }
}
