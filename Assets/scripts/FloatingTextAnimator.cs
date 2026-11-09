using UnityEngine;

public class FloatingTextAnimator : MonoBehaviour
{
    private float lifetime = 0.65f;
    private float elapsed = 0f;
    private TextMesh textMesh;
    private Vector3 initialPos;

    private void Start()
    {
        textMesh = GetComponent<TextMesh>();
        initialPos = transform.position;

        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float progress = elapsed / lifetime;

        transform.position = initialPos + Vector3.up * (progress * 1.6f);

        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }

        if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a = Mathf.Clamp01(1f - progress);
            textMesh.color = c;
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
