using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Procedural tree, step 1: a straight, tapering trunk, plus branches that begin
/// above a user-defined height. Each branch's starting radius is
/// (trunk radius at that height) * branchRadiusRatio.
///
/// Usage: add to an empty GameObject, assign a material to the MeshRenderer,
/// then right-click the component header -> "Generate Tree" (or enter Play mode).
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralTree : MonoBehaviour
{
    const string MeshName = "Procedural Tree";

    [Header("General")]
    public int seed = 12345;
    public bool generateOnStart = true;

    [Header("Trunk")]
    [Min(0.01f)] public float trunkBaseRadius = 0.5f;
    [Tooltip("Exponential taper per metre of length: r(h) = baseRadius * e^(-rate * h). " +
             "Higher = skinnier and shorter tree.")]
    [Min(0.001f)] public float trunkTaperRate = 0.2f;
    [Tooltip("The trunk ends once its radius would drop below this.")]
    [Min(0.001f)] public float trunkMinRadius = 0.03f;
    [Min(0.05f)] public float trunkSegmentLength = 0.25f;
    [Range(3, 64)] public int trunkRadialSegments = 16;

    [Header("Branches")]
    [Tooltip("Height above the base (local space, metres) at which branches begin to spawn.")]
    public float branchStartHeight = 3f;
    [Tooltip("Average vertical distance between branch nodes.")]
    [Min(0.05f)] public float branchSpacing = 0.7f;
    [Range(1, 6)] public int branchesPerNode = 2;
    [Tooltip("Branch starting radius as a fraction of the trunk radius at the spawn height.")]
    [Range(0.05f, 1f)] public float branchRadiusRatio = 0.5f;
    [Tooltip("Branches taper faster than the trunk, which sets how long they get.")]
    [Min(0.001f)] public float branchTaperRate = 0.5f;
    [Min(0.001f)] public float branchMinRadius = 0.01f;
    [Min(0.05f)] public float branchSegmentLength = 0.2f;
    [Range(3, 32)] public int branchRadialSegments = 8;
    [Tooltip("Angle from vertical, in degrees. 0 = straight up, 90 = horizontal.")]
    [Range(0f, 120f)] public float branchPitchMin = 40f;
    [Range(0f, 120f)] public float branchPitchMax = 70f;
    [Tooltip("Random +/- degrees added to each branch's evenly spaced compass direction.")]
    [Range(0f, 90f)] public float branchAzimuthJitter = 25f;

    [Header("Surface Noise (makes cross-sections 'roughly' circular)")]
    [Range(0f, 0.3f)] public float radiusNoise = 0.08f;
    public float noiseFrequency = 1.5f;          // around the circumference
    public float noiseVerticalFrequency = 0.6f;  // along the length (per metre)
    [Min(0.01f)] public float uvTilingV = 0.5f;  // texture repeats per metre of length

    // Mesh buffers, rebuilt on every Generate()
    readonly List<Vector3> vertices = new List<Vector3>();
    readonly List<Vector3> normals = new List<Vector3>();
    readonly List<Vector2> uvs = new List<Vector2>();
    readonly List<int> triangles = new List<int>();

    System.Random rng;

    void Start()
    {
        if (generateOnStart) Generate();
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>Radius of the trunk at a given height above the base.</summary>
    public float TrunkRadiusAt(float height)
    {
        return trunkBaseRadius * Mathf.Exp(-trunkTaperRate * height);
    }

    /// <summary>Total trunk height: where the radius reaches trunkMinRadius.</summary>
    public float TrunkHeight
    {
        get
        {
            if (trunkBaseRadius <= trunkMinRadius) return 0f;
            return Mathf.Log(trunkBaseRadius / trunkMinRadius) / trunkTaperRate;
        }
    }

    [ContextMenu("Generate Tree")]
    public void Generate()
    {
        rng = new System.Random(seed);
        vertices.Clear();
        normals.Clear();
        uvs.Clear();
        triangles.Clear();

        // Trunk: straight up from the local origin.
        AddTube(Vector3.zero, Vector3.up,
                trunkBaseRadius, trunkTaperRate, trunkMinRadius,
                trunkSegmentLength, trunkRadialSegments, Range(10f, 1000f));

        SpawnBranches();
        ApplyMesh();
    }

    // ------------------------------------------------------------------
    // Branches
    // ------------------------------------------------------------------

    void SpawnBranches()
    {
        float trunkHeight = TrunkHeight;
        float azimuthStart = Range(0f, 360f);

        for (float h = branchStartHeight;
             h < trunkHeight;
             h += branchSpacing * Range(0.75f, 1.25f))
        {
            // Branch radius is tied directly to the trunk's radius at this height.
            float branchRadius = TrunkRadiusAt(h) * branchRadiusRatio;

            for (int j = 0; j < branchesPerNode; j++)
            {
                float azimuth = (azimuthStart
                                 + j * 360f / branchesPerNode
                                 + Range(-branchAzimuthJitter, branchAzimuthJitter)) * Mathf.Deg2Rad;
                float pitch = Range(branchPitchMin, branchPitchMax) * Mathf.Deg2Rad;

                Vector3 dir = new Vector3(
                    Mathf.Sin(pitch) * Mathf.Cos(azimuth),
                    Mathf.Cos(pitch),
                    Mathf.Sin(pitch) * Mathf.Sin(azimuth));

                // Starts on the trunk's axis, so the first (smaller) rings sit inside the trunk.
                AddTube(new Vector3(0f, h, 0f), dir,
                        branchRadius, branchTaperRate, branchMinRadius,
                        branchSegmentLength, branchRadialSegments, Range(10f, 1000f));
            }

            azimuthStart += 137.5f; // golden angle keeps successive nodes from lining up
        }
    }

    // ------------------------------------------------------------------
    // Tube builder (used for both trunk and branches)
    // ------------------------------------------------------------------

    /// <summary>
    /// Appends a straight, exponentially tapering tube to the mesh buffers.
    /// It runs from startRadius down to minRadius, then closes with a small cone tip.
    /// </summary>
    void AddTube(Vector3 origin, Vector3 dir,
                 float startRadius, float taperRate, float minRadius,
                 float segmentLength, int radialSegments, float noiseSeed)
    {
        if (startRadius <= minRadius) return; // too thin to bother

        dir.Normalize();

        // Solve startRadius * e^(-rate * L) = minRadius for the tube length L.
        float length = Mathf.Log(startRadius / minRadius) / taperRate;
        int segments = Mathf.Max(1, Mathf.CeilToInt(length / segmentLength));
        int stride = radialSegments + 1; // +1: duplicate seam vertex so UVs wrap cleanly
        int baseIndex = vertices.Count;

        // Build a frame perpendicular to dir.
        Vector3 right = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.999f
            ? Vector3.right
            : Vector3.Cross(Vector3.up, dir).normalized;
        Vector3 forward = Vector3.Cross(dir, right);

        // Rings
        for (int ring = 0; ring <= segments; ring++)
        {
            float l = length * ring / segments;
            float r = startRadius * Mathf.Exp(-taperRate * l);
            Vector3 center = origin + dir * l;

            for (int i = 0; i <= radialSegments; i++)
            {
                float t = (float)i / radialSegments;
                float a = t * Mathf.PI * 2f;
                float c = Mathf.Cos(a);
                float s = Mathf.Sin(a);

                // Sampling noise on a circle keeps it continuous around the seam.
                float n = Mathf.PerlinNoise(
                    noiseSeed + c * noiseFrequency + l * noiseVerticalFrequency,
                    noiseSeed * 0.7f + s * noiseFrequency);
                float rr = r * (1f + (n - 0.5f) * 2f * radiusNoise);

                Vector3 radial = right * c + forward * s;
                vertices.Add(center + radial * rr);

                // Surface tilts toward the tip because the tube narrows (dr/dl = -rate * r).
                normals.Add((radial + dir * (taperRate * r)).normalized);
                uvs.Add(new Vector2(t, l * uvTilingV));
            }
        }

        // Side triangles (wound clockwise when seen from outside, as Unity expects)
        for (int ring = 0; ring < segments; ring++)
        {
            for (int i = 0; i < radialSegments; i++)
            {
                int a = baseIndex + ring * stride + i;
                int b = a + 1;
                int c = a + stride;
                int d = c + 1;

                triangles.Add(a); triangles.Add(b); triangles.Add(c);
                triangles.Add(b); triangles.Add(d); triangles.Add(c);
            }
        }

        // Tip cap: a small cone closing the narrow end.
        int apexIndex = vertices.Count;
        vertices.Add(origin + dir * (length + minRadius));
        normals.Add(dir);
        uvs.Add(new Vector2(0.5f, (length + minRadius) * uvTilingV));

        int lastRingStart = baseIndex + segments * stride;
        for (int i = 0; i < radialSegments; i++)
        {
            triangles.Add(lastRingStart + i);
            triangles.Add(lastRingStart + i + 1);
            triangles.Add(apexIndex);
        }
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    void ApplyMesh()
    {
        MeshFilter mf = GetComponent<MeshFilter>();

        // Clean up the previous generated mesh so we don't leak it.
        if (mf.sharedMesh != null && mf.sharedMesh.name == MeshName)
        {
            if (Application.isPlaying) Destroy(mf.sharedMesh);
            else DestroyImmediate(mf.sharedMesh);
        }

        Mesh mesh = new Mesh { name = MeshName };
        mesh.indexFormat = vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        mf.sharedMesh = mesh;
    }

    float Range(float min, float max)
    {
        return min + (float)rng.NextDouble() * (max - min);
    }
}