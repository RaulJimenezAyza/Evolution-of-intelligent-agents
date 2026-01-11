using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public bool activarMutaciones = true;
    public GameObject criaturaPrefab;
    public bool usuarioJugando = false;
    public bool puedeComer = true;
    public float distanciaVision = 20;
    public float size = 1.0f;
    public float energy = 20;
    public float energiaObtenida = 10;
    public float energiaReproducionObtenida = 1;
    public float energiaReproducion = 0;
    public float energiaNecesariaParaReproducion = 10;
    public float FB = 0;
    public float LR = 0;
    public int numeroDeHijos = 1;
    public int cantidadHijosCreados = 0;
    private bool haMutado = false;
    float elapsed = 0f;
    public float tiempoDeVida = 0f;
    public int numRaycasts = 36;
    public float[] inputsToNN;

    public float cantidadMutacion = 0.8f;
    public float posibilidadMutacion = 0.2f; 
    public NN nn;
    public Movement movement;

    public bool isDead = false;

    // Start is called before the first frame update
    void Awake()
    {
        nn = gameObject.GetComponent<NN>();
        movement = gameObject.GetComponent<Movement>();
        this.name = "Agent";
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //only do this once
        if(!haMutado)
        {
            //call mutate function to mutate the neural network
            MutateCreature();
            this.transform.localScale = new Vector3(size,size,size);
            haMutado = true;
            energy = 20;
        }

        ManageEnergy();

        int inputsPerRay = 4; // Food, Wall, Agent, Distance
        inputsToNN = new float[numRaycasts * inputsPerRay+1];//El mas 1 es para poner tambien la energia
        // Set up a variable to store the angle between raycasts
        float angleBetweenRaycasts = 5;
        float startAngle = -((numRaycasts - 1) / 2f) * angleBetweenRaycasts;
        // Use multiple raycasts to detect food objects
        RaycastHit hit;
        for (int i = 0; i < numRaycasts; i++)
        {
            float angle = startAngle + i * angleBetweenRaycasts;
            // Rotate the direction of the raycast by the specified angle around the y-axis of the agent
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 rayDirection = rotation * transform.forward;
            // Increase the starting point of the raycast by 0.1 units
            Vector3 rayStart = transform.position + Vector3.up * (size * 0.1f);
            int baseIndex = i * inputsPerRay;
            // Reset por defecto
            inputsToNN[baseIndex + 0] = 0f; // Food
            inputsToNN[baseIndex + 1] = 0f; // Wall
            inputsToNN[baseIndex + 2] = 0f; // Agent
            inputsToNN[baseIndex + 3] = 1f; // Distancia máxima
            if (Physics.Raycast(rayStart, rayDirection, out hit, distanciaVision))
            {
                
                //Distancia a lo que hemos visto
                float normalizedDistance = hit.distance / distanciaVision;
                inputsToNN[baseIndex + 3] = normalizedDistance;//Assignamos el valor al lugar correspondiente en el vector

                if (hit.collider.CompareTag("Food"))
                {
                    inputsToNN[baseIndex + 0] = 1f;
                    // Draw a line representing the raycast in the scene view for debugging purposes
                    Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.red);
                }
                else if (hit.collider.CompareTag("Wall"))
                {
                    inputsToNN[baseIndex + 1] = 1f;
                    // Draw a line representing the raycast in the scene view for debugging purposes
                    Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.blue);
                }
                else if (hit.collider.CompareTag("Agent"))
                {
                    inputsToNN[baseIndex + 2] = 1f;
                    // Draw a line representing the raycast in the scene view for debugging purposes
                    Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.green);
                }
            }
            else
            {
                // Draw a line representing the raycast in the scene view for debugging purposes
                Debug.DrawRay(rayStart, rayDirection * distanciaVision, Color.gray);
            }
        }
        float energyNormalized = Mathf.Clamp01(energy / 100f);
        inputsToNN[inputsToNN.Length - 1] = energyNormalized;
        // Get outputs from the neural network
        float [] outputsFromNN = nn.Brain(inputsToNN);

        //Store the outputs from the neural network in variables
        FB = outputsFromNN[0];
        LR = outputsFromNN[1];

        //if the agent is the user, use the inputs from the user instead of the neural network
        if (usuarioJugando)
        {
            FB = Input.GetAxis("Vertical");
            LR = Input.GetAxis("Horizontal")/10;
        }

        //Move the agent using the move function
        movement.Move(FB, LR);
    }

    //this function gets called whenever the agent collides with a trigger. (Which in this case is the food)
    void OnTriggerEnter(Collider col)
    {
        //if the agent collides with a food object, it will eat it and gain energy.
        if (col.gameObject.tag == "Food" && puedeComer)
        {
            energy += energiaObtenida;
            energiaReproducion += energiaReproducionObtenida;
            Destroy(col.gameObject);
        }
    }

    public void ManageEnergy()
    {
        elapsed += Time.deltaTime;
        tiempoDeVida += Time.deltaTime;
        if (elapsed >= 1f)
        {
            elapsed = elapsed % 1f;

            //subtract 1 energy per second
            energy -= 1f;

            //if agent has enough energy to reproduce, reproduce
            if (energiaReproducion >= energiaNecesariaParaReproducion)
            {
                energiaReproducion = 0;
                Reproduce();
            }
        }

        //Starve
        float agentY = this.transform.position.y;
        if (energy <= 0 || agentY < -10)
        {
            this.transform.Rotate(0, 0, 180);
            //this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 3.5f, this.transform.position.z);
            Destroy(this.gameObject,3);
            GetComponent<Movement>().enabled = false;
        }

    }

    private void MutateCreature()
    {
        if(activarMutaciones)
        {
            cantidadMutacion += Random.Range(-1.0f, 1.0f)/100;
            posibilidadMutacion += Random.Range(-1.0f, 1.0f)/100;
        }

        //make sure mutation amount and chance are positive using max function
        cantidadMutacion = Mathf.Max(cantidadMutacion, 0);
        posibilidadMutacion = Mathf.Max(posibilidadMutacion, 0);

        nn.MutateNetwork(cantidadMutacion, posibilidadMutacion);
    }



    public void Reproduce()
    {
        NN parentNN = GetComponent<NN>();
        //replicate
        for (int i = 0; i< numeroDeHijos; i ++) // I left this here so I could possibly change the number of children a parent has at a time.
        {
            //create a new agent, and set its position to the parent's position + a random offset in the x and z directions (so they don't all spawn on top of each other)
            GameObject child = Instantiate(criaturaPrefab, new Vector3(
                (float)this.transform.position.x + Random.Range(-10, 11), 
                0.75f, 
                (float)this.transform.position.z+ Random.Range(-10, 11)), 
                Quaternion.identity);

            NN childNN = child.GetComponent<NN>();
            // --- HEREDAR ARQUITECTURA ---
            childNN.numInputs = parentNN.numInputs;
            childNN.networkShape = (int[])parentNN.networkShape.Clone();
            // Crear nuevas capas con el mismo tamaño
            childNN.layers = new NN.Layer[parentNN.layers.Length];
            for (int j = 0; j < parentNN.layers.Length; j++)
            {
                childNN.layers[j] = new NN.Layer(
                    parentNN.layers[j].n_inputs,
                    parentNN.layers[j].n_neurons
                );
            }
            // --- COPIAR LOS PESOS ---
            childNN.layers = parentNN.copyLayers(); ;

            // Reset valores
            Creature childCreature = child.GetComponent<Creature>();
            childCreature.tiempoDeVida = 0f;
            childCreature.energy = 20;
            childCreature.energiaReproducion = 0;

            // Mutar distancia de visión ±4
            float variacion = Random.Range(-4f, 4f);
            childCreature.distanciaVision = Mathf.Clamp(this.distanciaVision + variacion,2f,50f);
            cantidadHijosCreados++;
        }
        energiaReproducion = 0;

    }
}
