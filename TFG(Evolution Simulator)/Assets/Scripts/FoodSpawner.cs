using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public float spawnRate = 1;
    public int floorScale = 1;
    public int cantidad = 1;
    public int cantidadMaxComidaInicial = 200;
    public GameObject myPrefab;
    public float timeElapsed = 0;

    void Start()
    {
        // Spawn food at random locations at the start of the game
        for (int i = 0; i < cantidadMaxComidaInicial; i++)
        {
            SpawnFood();
        }
    }

    // FixedUpdate is called once per physics frame
    void FixedUpdate()
    {
        //spawn food every second with timeElapsed
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= spawnRate)
        {
            timeElapsed = timeElapsed % spawnRate;
            for(int i = 0; i <= cantidad; i++)
            {
                SpawnFood();
            }
        }
    }

    void SpawnFood()
    {
        int maxIntentos = 20; // evita bucles infinitos
        int intentos = 0;

        while (intentos < maxIntentos)
        {
            intentos++;

            int x = Random.Range(-70, 85) * floorScale;
            int z = Random.Range(-70, 75) * floorScale;

            Vector3 startPoint = new Vector3(x, 200f, z);
            RaycastHit hit;

            if (Physics.Raycast(startPoint, Vector3.down, out hit, Mathf.Infinity))
            {
                // Verificamos que el collider tenga el tag "Floor"
                if (hit.collider.CompareTag("Floor"))
                {
                    Vector3 spawnPos = hit.point + Vector3.up * 0.15f;
                    Instantiate(myPrefab, spawnPos, Quaternion.identity);
                    return; // éxito, salimos de la función
                }
            }
        }

        Debug.LogWarning("No se pudo encontrar un suelo válido para spawnear comida después de varios intentos.");
    }
}
