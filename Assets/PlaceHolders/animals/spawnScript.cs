using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class ResourceGenerator : MonoBehaviour
{
    [Header("Spawn settings")]
    public GameObject[] resourcePrefabs;
    public float spawnChance;
 
    [Header("Grid setup")]
    public Vector2 centerPosition;
    public float gridRange = 30f;
    public int maxSpawnedResources = 30;
    public float distanceBetweenCheck = 5f;
 
    [Header("Raycast setup")]
    public float heightOfCheck = 10f, raycastDistance = 30f;
    public LayerMask layerMask;
 
    private void Start()
    {
        SpawnResources();
    }
 
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            DeleteResources();
            SpawnResources();
        }
    }
 
    void SpawnResources()
    {
        if (gridRange < 0f || distanceBetweenCheck <= 0f)
        {
            Debug.LogError("Grid range must be non-negative and spacing must be greater than zero.", this);
            return;
        }

        int checksFromCenter = Mathf.FloorToInt(gridRange / distanceBetweenCheck);
        for (int x = -checksFromCenter; x <= checksFromCenter; x++)
        {
            for (int z = -checksFromCenter; z <= checksFromCenter; z++)
            {
                float worldX = centerPosition.x + x * distanceBetweenCheck;
                float worldZ = centerPosition.y + z * distanceBetweenCheck;
                RaycastHit hit;
                if(Physics.Raycast(new Vector3(worldX, heightOfCheck, worldZ), Vector3.down, out hit, raycastDistance, layerMask))
                {
                    if(spawnChance > Random.Range(0f, 101f))
                    {
                        int randomIndex = Random.Range(0, resourcePrefabs.Length);
                        Instantiate(resourcePrefabs[randomIndex], hit.point, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)), transform);
                        maxSpawnedResources--;
                        if(maxSpawnedResources <= 0)
                        {
                            return; 
                        }
                    }
                }
            }
        }
    }
 
    void DeleteResources()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}