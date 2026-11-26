using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap, Mesh };
    public DrawMode drawMode;

    [Header("Map Settings")]
    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    [SerializeField] private float noiseScale;
    [SerializeField] private float mapHeightMultiplier;
    [SerializeField] private AnimationCurve meshHeightCurve;


    [Header("Noise Settings")]
    [SerializeField] private int octaves;
    [Range(0, 1)] [SerializeField] private float persistence;
    [SerializeField] private float lacunarity;

    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;
    public bool autoUpdate;

    [Header("Terrain Settings")]
    [SerializeField] private TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence, lacunarity, offset);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++) 
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].maxHeight)
                    {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindAnyObjectByType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap) 
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawMode == DrawMode.ColourMap)  
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
        else if (drawMode == DrawMode.Mesh) 
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, mapHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
    }

    void OnValidate()
    {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;

    }
}

[System.Serializable]
public struct TerrainType
{
    public string label;
    public float maxHeight;
    public Color colour;
}