using System.Collections;
using UnityEngine;
using TMPro;

public class AnswerBox : MonoBehaviour
{
    public string answer;

    [Header("Visual & Effects")]
    [Tooltip("Audio clip saat box jawaban benar ditabrak. Jika kosong, akan memainkan suara cling bel procedurial bawaan.")]
    public AudioClip clingSound;

    private TMP_Text answerText;
    private Renderer boxRenderer;
    private Color originalColor = Color.white;
    private bool hasTriggered = false;

    private static AudioClip proceduralChimeClip;

    private void Awake()
    {
        answerText = GetComponentInChildren<TMP_Text>();
        boxRenderer = GetComponent<Renderer>();
    }

    public void Setup(string answerTextValue, Color boxColor)
    {
        answer = answerTextValue;
        hasTriggered = false;
        originalColor = boxColor;

        if (answerText != null)
        {
            answerText.text = answer.ToUpper();
        }

        if (boxRenderer != null)
        {
            boxRenderer.material.color = boxColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        hasTriggered = true;

        QuizManager quiz = QuizManager.Instance != null ? QuizManager.Instance : FindFirstObjectByType<QuizManager>();

        if (quiz != null)
        {
            quiz.CheckAnswer(answer, this);
        }
    }

    /// <summary>
    /// Memainkan efek 'cling' saat jawaban benar: suara cling, partikel sparkle, teks +10, dan animasi pop-shrink
    /// </summary>
    public void PlayClingAndDisappear()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        PlayClingSound(transform.position);
        SpawnSparkleBurst(transform.position, originalColor);
        SpawnFloatingScore(transform.position + Vector3.up * 0.8f);

        StartCoroutine(PopAndShrinkRoutine());
    }

    /// <summary>
    /// Menghilangkan box lain dengan efek shrink halus saat soal selesai
    /// </summary>
    public void FadeAndDisappear()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        StartCoroutine(ShrinkRoutine(0.12f));
    }

    private IEnumerator PopAndShrinkRoutine()
    {
        Vector3 startScale = transform.localScale;
        Vector3 popScale = startScale * 1.3f;

        // Pop membesar sedikit (0.04 detik)
        float elapsed = 0f;
        while (elapsed < 0.04f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, popScale, elapsed / 0.04f);
            yield return null;
        }

        // Mengecil hingga hilang (0.12 detik)
        elapsed = 0f;
        while (elapsed < 0.12f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(popScale, Vector3.zero, elapsed / 0.12f);
            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator ShrinkRoutine(float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void PlayClingSound(Vector3 position)
    {
        AudioClip clipToPlay = clingSound;
        if (clipToPlay == null)
        {
            clipToPlay = GetOrCreateChimeClip();
        }

        if (clipToPlay != null)
        {
            AudioSource.PlayClipAtPoint(clipToPlay, position, 1f);
        }
    }

    private static AudioClip GetOrCreateChimeClip()
    {
        if (proceduralChimeClip != null)
            return proceduralChimeClip;

        int sampleRate = 44100;
        float duration = 0.35f;
        int totalSamples = (int)(sampleRate * duration);
        float[] samples = new float[totalSamples];

        // Frekuensi bel ganda bernada tinggi yang manis: E6 (1318.5Hz) & C7 (2093Hz)
        float f1 = 1318.5f;
        float f2 = 2093.0f;

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 9f);
            float tone = (Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.6f) + (Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.4f);
            samples[i] = tone * envelope;
        }

        proceduralChimeClip = AudioClip.Create("ClingSound", totalSamples, 1, sampleRate, false);
        proceduralChimeClip.SetData(samples, 0);
        return proceduralChimeClip;
    }

    private void SpawnSparkleBurst(Vector3 position, Color color)
    {
        GameObject fxObj = new GameObject("SparkleBurstFX");
        fxObj.transform.position = position;

        ParticleSystem ps = fxObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.45f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
        main.startColor = new ParticleSystem.MinMaxGradient(Color.white, color);
        main.gravityModifier = 0.4f;
        main.playOnAwake = false;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 25) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.35f;

        ps.Play();
        Destroy(fxObj, 0.8f);
    }

    private void SpawnFloatingScore(Vector3 position)
    {
        GameObject textObj = new GameObject("FloatingScoreText");
        textObj.transform.position = position;

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = "+10";
        textMesh.fontSize = 42;
        textMesh.characterSize = 0.12f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = new Color(1f, 0.88f, 0.2f); // Kuning emas

        textObj.AddComponent<FloatingTextAnimator>();
    }
}