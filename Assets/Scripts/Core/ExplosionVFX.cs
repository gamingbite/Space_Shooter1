using UnityEngine;
using System.Collections;

/// <summary>
/// ExplosionVFX - Hiệu ứng nổ mới dùng Particle System thuần code
/// Thay thế sprite-based explosion (explode_2_0)
/// Gồm: Flash core → Shockwave ring → Sparks → Smoke fade
/// </summary>
[RequireComponent(typeof(AutoDestroy))]
public class ExplosionVFX : MonoBehaviour
{
    [Header("Core Flash")]
    [SerializeField] private Color coreColorStart = new Color(1f, 1f, 0.9f, 1f);
    [SerializeField] private Color coreColorEnd = new Color(1f, 0.4f, 0.0f, 0f);
    [SerializeField] private float coreMaxSize = 1.2f;
    [SerializeField] private float coreDuration = 0.25f;

    [Header("Shockwave Ring")]
    [SerializeField] private Color ringColor = new Color(1f, 0.6f, 0.1f, 0.7f);
    [SerializeField] private float ringMaxSize = 2.5f;
    [SerializeField] private float ringDuration = 0.4f;

    [Header("Sparks")]
    [SerializeField] private int sparkCount = 14;
    [SerializeField] private float sparkSpeed = 5f;
    [SerializeField] private float sparkLifetime = 0.5f;
    [SerializeField] private Color sparkColor = new Color(1f, 0.85f, 0.2f, 1f);

    [Header("Secondary Sparks")]
    [SerializeField] private int debrisCount = 8;
    [SerializeField] private float debrisSpeed = 3f;
    [SerializeField] private Color debrisColor = new Color(1f, 0.4f, 0.1f, 0.9f);

    [Header("Smoke")]
    [SerializeField] private float smokeMaxSize = 1.8f;
    [SerializeField] private float smokeDuration = 0.6f;
    [SerializeField] private Color smokeColor = new Color(0.3f, 0.2f, 0.15f, 0.5f);

    [Header("Light")]
    [SerializeField] private bool useLight = true;
    [SerializeField] private float lightIntensity = 3f;
    [SerializeField] private Color lightColor = new Color(1f, 0.5f, 0.1f);

    private static Sprite _circleSprite;

    private void Start()
    {
        if (_circleSprite == null)
            _circleSprite = MakeCircleSprite(64);

        StartCoroutine(PlayExplosion());
    }

    private IEnumerator PlayExplosion()
    {
        // Phase 1: Core flash + Light burst
        StartCoroutine(PlayCore());

        // Phase 2: Shockwave ring
        StartCoroutine(PlayRing());

        // Phase 3: Sparks
        SpawnSparks();
        SpawnDebris();

        // Phase 4: Smoke
        StartCoroutine(PlaySmoke());

        // Light pulse
        if (useLight)
            StartCoroutine(PulseLight());

        // Tổng thời gian = max của tất cả phases
        yield return null;
    }

    // ── CORE FLASH ──────────────────────────────────────────────────────────────

    private IEnumerator PlayCore()
    {
        GameObject core = MakeCircleGO("EXP_Core", coreColorStart, 10);
        float elapsed = 0f;

        while (elapsed < coreDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / coreDuration;

            // Expand quickly then contract
            float sizeCurve = t < 0.3f ? t / 0.3f : 1f - ((t - 0.3f) / 0.7f);
            core.transform.localScale = Vector3.one * coreMaxSize * sizeCurve;

            // Color lerp
            if (core.GetComponent<SpriteRenderer>() is SpriteRenderer sr)
                sr.color = Color.Lerp(coreColorStart, coreColorEnd, t);

            yield return null;
        }

        Destroy(core);
    }

    // ── SHOCKWAVE RING ──────────────────────────────────────────────────────────

    private IEnumerator PlayRing()
    {
        // Ring dùng 2 circles: outer - inner để tạo ring shape
        GameObject outerRing = MakeCircleGO("EXP_Ring", ringColor, 8);
        outerRing.transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < ringDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ringDuration;

            float size = Mathf.Lerp(0.1f, ringMaxSize, Mathf.Sqrt(t));
            outerRing.transform.localScale = Vector3.one * size;

            if (outerRing.GetComponent<SpriteRenderer>() is SpriteRenderer sr)
            {
                Color c = ringColor;
                c.a = Mathf.Lerp(ringColor.a, 0f, t * t);
                sr.color = c;
            }

            yield return null;
        }

        Destroy(outerRing);
    }

    // ── SPARKS ──────────────────────────────────────────────────────────────────

    private void SpawnSparks()
    {
        for (int i = 0; i < sparkCount; i++)
        {
            float angle = (360f / sparkCount) * i + Random.Range(-15f, 15f);
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject spark = MakeCircleGO("EXP_Spark_" + i, sparkColor, 9);
            float sparkSize = Random.Range(0.06f, 0.14f);
            spark.transform.localScale = Vector3.one * sparkSize;

            float speed = sparkSpeed * Random.Range(0.7f, 1.4f);
            float lifetime = sparkLifetime * Random.Range(0.7f, 1.2f);
            StartCoroutine(MoveSpark(spark, dir * speed, lifetime));
        }
    }

    private void SpawnDebris()
    {
        for (int i = 0; i < debrisCount; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject debris = MakeCircleGO("EXP_Debris_" + i, debrisColor, 7);
            float size = Random.Range(0.08f, 0.18f);
            debris.transform.localScale = Vector3.one * size;

            float speed = debrisSpeed * Random.Range(0.5f, 1.5f);
            float lifetime = sparkLifetime * Random.Range(1.0f, 1.8f);
            StartCoroutine(MoveSpark(debris, dir * speed, lifetime, true));
        }
    }

    private IEnumerator MoveSpark(GameObject obj, Vector2 velocity, float lifetime, bool scaleDown = false)
    {
        float elapsed = 0f;
        Vector3 initScale = obj.transform.localScale;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Color initColor = sr != null ? sr.color : Color.white;

        while (elapsed < lifetime && obj != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // Gravity drag
            velocity.y -= 2f * Time.deltaTime;
            obj.transform.position += new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime;

            // Fade
            if (sr != null)
            {
                Color c = initColor;
                c.a = Mathf.Lerp(initColor.a, 0f, t);
                sr.color = c;
            }

            // Scale down debris
            if (scaleDown)
                obj.transform.localScale = Vector3.Lerp(initScale, Vector3.zero, t);

            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    // ── SMOKE ───────────────────────────────────────────────────────────────────

    private IEnumerator PlaySmoke()
    {
        yield return new WaitForSeconds(0.1f); // Delay after core

        GameObject smoke = MakeCircleGO("EXP_Smoke", smokeColor, 6);
        smoke.transform.localScale = Vector3.zero;

        float elapsed = 0f;
        float driftX = Random.Range(-0.3f, 0.3f);
        float driftY = Random.Range(0.1f, 0.4f);

        while (elapsed < smokeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / smokeDuration;

            float size = Mathf.Lerp(0.2f, smokeMaxSize, t);
            smoke.transform.localScale = Vector3.one * size;
            smoke.transform.position += new Vector3(driftX, driftY, 0) * Time.deltaTime * 0.5f;

            if (smoke.GetComponent<SpriteRenderer>() is SpriteRenderer sr)
            {
                Color c = smokeColor;
                c.a = Mathf.Lerp(smokeColor.a, 0f, t * t);
                sr.color = c;
            }

            yield return null;
        }

        Destroy(smoke);
    }

    // ── LIGHT PULSE ─────────────────────────────────────────────────────────────

    private IEnumerator PulseLight()
    {
        var light2D = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        if (light2D == null)
        {
            light2D = gameObject.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light2D.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
            light2D.color = lightColor;
            light2D.pointLightOuterRadius = 3f;
            light2D.pointLightInnerRadius = 0.5f;
        }

        light2D.intensity = lightIntensity;
        float elapsed = 0f;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            light2D.intensity = Mathf.Lerp(lightIntensity, 0f, t * t);
            yield return null;
        }

        light2D.intensity = 0f;
    }

    // ── UTILITIES ───────────────────────────────────────────────────────────────

    private GameObject MakeCircleGO(string name, Color color, int sortOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _circleSprite;
        sr.color = color;
        sr.sortingOrder = sortOrder;

        return go;
    }

    private static Sprite MakeCircleSprite(int res)
    {
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 c = new Vector2(res / 2f, res / 2f);
        float r = res / 2f;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c);
                float a = Mathf.Clamp01(1f - d / r);
                a = Mathf.Pow(a, 1.2f); // Glow falloff
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }
}
