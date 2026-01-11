using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    public GameObject agentPrefab;
    private GameObject[] agentList;
    public int floorScale = 1;

    // Update is called once per frame
    void FixedUpdate()
    {
        agentList = GameObject.FindGameObjectsWithTag("Agent");

        // if there are no agents in the scene, spawn one at a random location. 
        // This is to ensure that there is always at least one agent in the scene.
        if (agentList.Length < 10)
        {
            SpawnCreature();
        } 
    }

    void SpawnCreature()
    {
        int x = Random.Range(-70, 70)*floorScale;
        int z = Random.Range(-70, 70)*floorScale;
        // Punto inicial del raycast: alto (por si hay colinas, rampas, etc.)
        Vector3 startPoint = new Vector3(x, 200f, z);
        RaycastHit hit;

        if (Physics.Raycast(startPoint, Vector3.down, out hit, Mathf.Infinity))
        {
            // Verificamos que el collider tenga el tag "Floor"
            if (hit.collider.CompareTag("Floor"))
            {
                Vector3 spawnPos = hit.point + Vector3.up * 0.15f;
                Instantiate(agentPrefab, spawnPos, Quaternion.identity);
                return; // éxito, salimos de la función
            }
        }
        else
        {
            Debug.LogWarning("No se encontró suelo en la posición: " + x + ", " + z);
        }
    }
}
