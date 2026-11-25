using UnityEngine;

public class AttackVisualizer : MonoBehaviour
{
    private const int CircleSegments = 24;
    private const int MaxRails = 12;
    private readonly LineRenderer[] lines = new LineRenderer[3 + MaxRails];
    private Material lineMaterial;
    private float hideAt;
    private bool isVisible;

    void Awake()
    {
        lineMaterial = new Material(Shader.Find("Sprites/Default"));

        for (int i = 0; i < lines.Length; i++)
        {
            GameObject lineObject = new GameObject($"Attack Volume Line {i}");
            lineObject.transform.SetParent(transform, false);

            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.positionCount = CircleSegments + 1;
            line.useWorldSpace = true;
            line.sharedMaterial = lineMaterial;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            lines[i] = line;
        }
    }

    void Update()
    {
        if (isVisible && Time.time >= hideAt)
        {
            HideRay();
        }
    }

    public void ShowAttack(Vector3 start, Vector3 direction, float range, float radius, Color color, bool isHeavy)
    {
        Vector3 forward = direction.normalized;
        Vector3 reference = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.99f
            ? Vector3.right
            : Vector3.up;
        Vector3 right = Vector3.Cross(forward, reference).normalized;
        Vector3 up = Vector3.Cross(right, forward).normalized;
        Vector3 end = start + forward * range;
        Vector3 middle = Vector3.Lerp(start, end, 0.5f);
        int railCount = isHeavy ? MaxRails : 6;
        float width = isHeavy ? 0.045f : 0.025f;

        SetCircle(0, start, right, up, radius, color, width);
        SetCircle(1, middle, right, up, radius, color, width);
        SetCircle(2, end, right, up, radius, color, width);

        for (int i = 0; i < MaxRails; i++)
        {
            LineRenderer line = lines[3 + i];
            line.enabled = i < railCount;
            if (i >= railCount)
            {
                continue;
            }

            float angle = 2f * Mathf.PI * i / railCount;
            Vector3 offset = radius * (right * Mathf.Cos(angle) + up * Mathf.Sin(angle));
            ConfigureLine(line, color, width, 2);
            line.SetPosition(0, start + offset);
            line.SetPosition(1, end + offset);
        }

        isVisible = true;
        hideAt = Time.time + 0.8f;
    }

    private void SetCircle(int index, Vector3 center, Vector3 right, Vector3 up, float radius, Color color, float width)
    {
        LineRenderer line = lines[index];
        ConfigureLine(line, color, width, CircleSegments + 1);

        for (int i = 0; i <= CircleSegments; i++)
        {
            float angle = 2f * Mathf.PI * i / CircleSegments;
            Vector3 point = center + radius * (right * Mathf.Cos(angle) + up * Mathf.Sin(angle));
            line.SetPosition(i, point);
        }
    }

    private static void ConfigureLine(LineRenderer line, Color color, float width, int positionCount)
    {
        line.positionCount = positionCount;
        line.startWidth = width;
        line.endWidth = width;
        line.startColor = color;
        line.endColor = color;
        line.enabled = true;
    }

    public void HideRay()
    {
        foreach (LineRenderer line in lines)
        {
            line.enabled = false;
        }

        isVisible = false;
    }

    void OnDestroy()
    {
        Destroy(lineMaterial);
    }
}