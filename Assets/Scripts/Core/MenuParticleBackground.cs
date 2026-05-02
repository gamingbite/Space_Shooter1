using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Tạo hiệu ứng nền động cho màn hình chính:
/// - Stars twinkle (ngôi sao nhấp nháy)
/// - Shooting stars (sao băng)
/// - Nebula particles (mây tinh vân)
/// - Distant explosions (nổ xa xa)
/// </summary>
public class MenuParticleBackground : MonoBehaviour
{
    [Header("Star Settings")]
    [SerializeField] private int starCount = 80;
    [SerializeField] private float starAreaWidth = 22f;
    [SerializeField] private float starAreaHeight = 14f;
    [SerializeField] private float minStarSize = 0.03f;
    [SerializeField] private float maxStarSize = 0.12f;

    [Header("Shooting Star Settings")]
    [SerializeField] private float shootingStarInterval = 3.5f;
    [SerializeField] private float shootingStarSpeed = 12f;

    [Header("Nebula Settings")]
    [SerializeField] private int nebulaCount = 6;
    [SerializeField] private Color[] nebulaColors = new Color[]
    {
        new Color(0.2f, 0.05f, 0.4f, 0.08f),
        new Color(0.05f, 0.1f, 0.5f, 0.07f),
        new Color(0.4f, 0.05f, 0.15f, 0.06f),
        new Color(0.0f, 0.2f, 0.4f, 0.07f),
    };

    [Header("Distant Explosion Settings")]
    [SerializeField] private float explosionInterval = 6f;
    [SerializeField] private Color[] explosionColors = new Color[]
    {
        new Color(1f, 0.5f, 0.1f, 0.6f),
        new Color(1f, 0.8f, 0.2f, 0.5f),
        new Color(0.8f, 0.2f, 0.1f, 0.5f),
    };

    private List<StarData> stars = new List<StarData>();
    private Camera mainCam;

    private struct StarData
    {
        public GameObject obj;
        public SpriteRenderer sr;
        public float twinkleSpeed;
        public float twinkleOffset;
        public float baseAlpha;
    }

    private void Start()
    {
        mainCam = Camera.main;
        CreateNebulaLayer();
        CreateStars();
        StartCoroutine(ShootingStarRoutine());
        StartCoroutine(DistantExplosionRoutine());

        // Subscribe to game state changes
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameManager.GameState state)
    {
        // Ẩn background effect khi vào gameplay
        gameObject.SetActive(state == GameManager.GameState.Menu);
    }

    private void Update()
    {
        // Twinkle stars
        float t = Time.time;
        foreach (var star in stars)
        {
            if (star.sr == null) continue;
            float alpha = star.baseAlpha * (0.5f + 0.5f * Mathf.Sin(t * star.twinkleSpeed + star.twinkleOffset));
            var c = star.sr.color;
            c.a = alpha;
            star.sr.color = c;
        }
    }

    // ── STARS ────────────────────────────────────────────────────────────────

    private void CreateStars()
    {
        Sprite circle = CreateCircleSprite(32);

        for (int i = 0; i < starCount; i++)
        {
            float x = Random.Range(-starAreaWidth / 2f, starAreaWidth / 2f);
            float y = Random.Range(-starAreaHeight / 2f, starAreaHeight / 2f);
            float z = Random.Range(1f, 5f); // z for sorting (further back)

            GameObject starObj = new GameObject("Star_" + i);
            starObj.transform.SetParent(transform);
            starObj.transform.position = new Vector3(x, y, z);

            float size = Random.Range(minStarSize, maxStarSize);
            starObj.transform.localScale = Vector3.one * size;

            SpriteRenderer sr = starObj.AddComponent<SpriteRenderer>();
            sr.sprite = circle;
            sr.sortingOrder = -10;

            // Màu ngôi sao: trắng, xanh nhạt, vàng nhạt, tím nhạt
            Color[] starColors = {
                Color.white,
                new Color(0.8f, 0.9f, 1f),
                new Color(1f, 1f, 0.8f),
                new Color(0.9f, 0.8f, 1f),
                new Color(0.7f, 0.9f, 1f),
            };
            Color col = starColors[Random.Range(0, starColors.Length)];
            float alpha = Random.Range(0.3f, 1f);
            sr.color = new Color(col.r, col.g, col.b, alpha);

            StarData sd = new StarData
            {
                obj = starObj,
                sr = sr,
                twinkleSpeed = Random.Range(0.5f, 2.5f),
                twinkleOffset = Random.Range(0f, Mathf.PI * 2f),
                baseAlpha = alpha
            };
            stars.Add(sd);
        }
    }

    // ── NEBULA ────────────────────────────────────────────────────────────────

    private void CreateNebulaLayer()
    {
        Sprite circle = CreateCircleSprite(64);

        for (int i = 0; i < nebulaCount; i++)
        {
            float x = Random.Range(-starAreaWidth / 2f, starAreaWidth / 2f);
            float y = Random.Range(-starAreaHeight / 2f, starAreaHeight / 2f);

            GameObject nebulaObj = new GameObject("Nebula_" + i);
            nebulaObj.transform.SetParent(transform);
            nebulaObj.transform.position = new Vector3(x, y, 6f);

            float size = Random.Range(3f, 7f);
            nebulaObj.transform.localScale = new Vector3(size * Random.Range(0.8f, 1.5f), size, 1f);

            SpriteRenderer sr = nebulaObj.AddComponent<SpriteRenderer>();
            sr.sprite = circle;
            sr.sortingOrder = -15;
            sr.color = nebulaColors[Random.Range(0, nebulaColors.Length)];

            // Slow drift
            StartCoroutine(NebulaDrift(nebulaObj, new Vector2(Random.Range(-0.05f, 0.05f), Random.Range(-0.02f, 0.02f))));
        }
    }

    private IEnumerator NebulaDrift(GameObject obj, Vector2 velocity)
    {
        while (obj != null)
        {
            obj.transform.position += new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime;
            yield return null;
        }
    }

    // ── SHOOTING STARS ────────────────────────────────────────────────────────

    private IEnumerator ShootingStarRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootingStarInterval + Random.Range(-1f, 1f));

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Menu)
                continue;

            yield return StartCoroutine(SpawnShootingStar());
        }
    }

    private IEnumerator SpawnShootingStar()
    {
        // Xuất phát từ cạnh trên/phải của màn hình
        float height = mainCam != null ? mainCam.orthographicSize : 5f;
        float width = mainCam != null ? height * mainCam.aspect : 8f;

        float startX = Random.Range(-width * 0.5f, width);
        float startY = height + 1f;

        Vector3 startPos = new Vector3(startX, startY, 2f);
        Vector3 direction = new Vector3(Random.Range(-0.5f, -0.2f), -1f, 0).normalized;

        // Tạo trail bằng nhiều GameObject nhỏ
        int trailLen = 12;
        List<GameObject> trailObjs = new List<GameObject>();
        Sprite circle = CreateCircleSprite(8);

        for (int i = 0; i < trailLen; i++)
        {
            GameObject dot = new GameObject("ShootingStar_Dot_" + i);
            dot.transform.SetParent(transform);
            SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
            sr.sprite = circle;
            sr.sortingOrder = 5;
            float t = (float)i / trailLen;
            float alpha = (1f - t) * 0.9f;
            float size = Mathf.Lerp(0.15f, 0.02f, t);
            dot.transform.localScale = Vector3.one * size;
            dot.transform.position = startPos - direction * (i * 0.15f);
            sr.color = new Color(1f, 1f, 0.9f, alpha);
            trailObjs.Add(dot);
        }

        float duration = 1.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector3 move = direction * shootingStarSpeed * Time.deltaTime;
            foreach (var dot in trailObjs)
            {
                if (dot != null)
                    dot.transform.position += move;
            }
            yield return null;
        }

        // Cleanup
        foreach (var dot in trailObjs)
        {
            if (dot != null)
                Destroy(dot);
        }
    }

    // ── DISTANT EXPLOSIONS ─────────────────────────────────────────────────────

    private IEnumerator DistantExplosionRoutine()
    {
        yield return new WaitForSeconds(2f); // Delay before first explosion

        while (true)
        {
            yield return new WaitForSeconds(explosionInterval + Random.Range(-2f, 2f));

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Menu)
                continue;

            SpawnDistantExplosion();
        }
    }

    private void SpawnDistantExplosion()
    {
        float height = mainCam != null ? mainCam.orthographicSize : 5f;
        float width = mainCam != null ? height * mainCam.aspect : 8f;

        // Spawn ở rìa màn hình hoặc nền, tránh trung tâm (nơi có UI)
        float x = Random.Range(-width * 0.8f, width * 0.8f);
        float y = Random.Range(-height * 0.8f, -height * 0.1f); // Ưu tiên phần dưới
        Vector3 pos = new Vector3(x, y, 3f);

        StartCoroutine(PlayDistantExplosion(pos));
    }

    private IEnumerator PlayDistantExplosion(Vector3 pos)
    {
        Sprite circle = CreateCircleSprite(32);
        Color explosionCol = explosionColors[Random.Range(0, explosionColors.Length)];
        float maxSize = Random.Range(0.3f, 0.8f);

        // Flash core
        GameObject core = new GameObject("ExplosionCore");
        core.transform.SetParent(transform);
        core.transform.position = pos;
        SpriteRenderer coreSR = core.AddComponent<SpriteRenderer>();
        coreSR.sprite = circle;
        coreSR.sortingOrder = 3;
        coreSR.color = Color.white;

        // Expand and fade
        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float size = Mathf.Lerp(0f, maxSize, Mathf.Sqrt(t));
            core.transform.localScale = Vector3.one * size;
            float alpha = Mathf.Lerp(0.9f, 0f, t);
            Color col = Color.Lerp(Color.white, explosionCol, t * 2f);
            col.a = alpha;
            coreSR.color = col;
            yield return null;
        }

        Destroy(core);

        // Spawn ring
        yield return StartCoroutine(PlayExplosionRing(pos, explosionCol, maxSize));
    }

    private IEnumerator PlayExplosionRing(Vector3 pos, Color col, float startSize)
    {
        Sprite circle = CreateCircleSprite(32);

        GameObject ring = new GameObject("ExplosionRing");
        ring.transform.SetParent(transform);
        ring.transform.position = pos;
        SpriteRenderer ringSR = ring.AddComponent<SpriteRenderer>();
        ringSR.sprite = circle;
        ringSR.sortingOrder = 2;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float size = Mathf.Lerp(startSize, startSize * 3f, t);
            ring.transform.localScale = Vector3.one * size;
            Color c = col;
            c.a = Mathf.Lerp(0.4f, 0f, t);
            ringSR.color = c;
            yield return null;
        }

        Destroy(ring);
    }

    // ── UTILITY ────────────────────────────────────────────────────────────────

    private static Sprite CreateCircleSprite(int resolution)
    {
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f;

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - dist / radius);
                // Soft glow falloff
                alpha = Mathf.Pow(alpha, 1.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f), resolution);
    }
}
