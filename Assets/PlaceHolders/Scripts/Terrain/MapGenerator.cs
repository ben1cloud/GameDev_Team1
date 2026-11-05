using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int mapWidth, mapHeight;
    [SerializeField] private float noiseScale;
    public bool autoUpdate;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);

        MapDisplay display = FindFirstObjectByType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }
}
