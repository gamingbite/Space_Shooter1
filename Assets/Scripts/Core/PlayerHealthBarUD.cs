using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUD : MonoBehaviour
{
  [Header("Settings")]
    [SerializeField] private Damageable targetDamageable;
    [SerializeField] private Slider healthSlider;

    private void OnEnable()
    {
        if (targetDamageable != null)
        {
            targetDamageable.OnHealthChanged += UpdateHealthBar;
        }
    }

    private void OnDisable()
    {
        if (targetDamageable != null)
        {
            targetDamageable.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void Start()
    {
        if (targetDamageable != null)
        {
            // Thiết lập giá trị tối đa cho Slider khớp với MaxHealth của nhân vật
            healthSlider.maxValue = targetDamageable.MaxHealth;
            UpdateHealthBar(targetDamageable.CurrentHealth, targetDamageable.MaxHealth);
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            // Cập nhật giá trị hiện tại. 
            // Nếu bạn có thay đổi MaxHealth trong game, hãy cập nhật lại maxValue ở đây.
            healthSlider.maxValue = maxHealth; 
            healthSlider.value = currentHealth;
        }
    }
}
