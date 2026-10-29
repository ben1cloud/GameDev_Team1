using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public int amountToPool;
    [SerializeField] private bool _parentTransform = false;



    void Start()
    {
        pooledObjects = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(objectToPool, _parentTransform? transform : null);
            tmp.SetActive(false);
            pooledObjects.Add(tmp);
        }
    }

    public void ClearPool()
    {
        foreach(GameObject g in pooledObjects)
        {
            g.SetActive(false);
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        return null;
    }
}
