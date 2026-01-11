using UnityEngine;
using System.Collections.Generic;

public class CreatureSpawner2 : MonoBehaviour
{
    public GameObject agentPrefab;
    public int floorScale = 1;

    private List<GameObject> agentList = new List<GameObject>();
    private bool initialized = false;

    void Start()
    {
        Invoke(nameof(DelayedInit), 0.1f);
    }

    void DelayedInit()
    {
        initialized = true;
    }

    void FixedUpdate()
    {
        if (!initialized || agentPrefab == null) return;

        agentList.RemoveAll(agent => agent == null);

        if (agentList.Count < 1)
        {
            SpawnCreature();
        }
    }

    void SpawnCreature()
    {
        if (agentPrefab == null) return;

        int x = Random.Range(-100, 101) * floorScale;
        int z = Random.Range(-100, 101) * floorScale;
        GameObject agent = Instantiate(agentPrefab, new Vector3((float)x, 0.75f, (float)z), Quaternion.identity);
        agentList.Add(agent);
    }
}