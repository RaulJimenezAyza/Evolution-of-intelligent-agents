using UnityEngine;

public class Creature2 : MonoBehaviour
{
    [Header("Configuración")]
    public bool activarMutaciones = true;
    public GameObject criaturaPrefab;
    public bool usuarioJugando = false;

    [Header("Energía")]
    public float energia = 20f;
    public float energiaObtenida = 10f;
    public float energiaReproduccion = 0f;
    public float energiaNecesariaParaReproduccion = 10f;

    [Header("Mutación")]
    public float cantidadMutacion = 0.8f;
    public float posibilidadMutacion = 0.2f;

    [Header("Percepción")]
    public float distanciaVision = 20f;
    public int numRayos = 9;

    // Componentes
    private NN2 nn;
    private Movement2 movement;
    private CharacterController controller;

    // Estado
    public bool isDead = false;
    private bool haMutado = false;
    private float elapsed = 0f;
    public float tiempoDeVida = 0f;
    private float[] distances;

    // Outputs
    public float FB = 0f;
    public float LR = 0f;

    void Awake()
    {
        nn = GetComponent<NN2>();
        movement = GetComponent<Movement2>();
        controller = GetComponent<CharacterController>();
        distances = new float[numRayos];

        this.name = "Agent";
    }

    void FixedUpdate()
    {
        if (isDead) return;

        tiempoDeVida += Time.fixedDeltaTime;

        // Mutación una sola vez
        if (!haMutado && activarMutaciones)
        {
            MutarCriatura();
            haMutado = true;
        }

        GestionarEnergia();

        if (!isDead)
        {
            ActualizarPercepcion();
            ProcesarIA();
            movement.Move(FB, LR);
        }
    }

    void ActualizarPercepcion()
    {
        for (int i = 0; i < numRayos; i++)
        {
            float startAngle = -((numRayos - 1) / 2f) * 20f;
            float angle = startAngle + i * 20f;

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 rayDirection = rotation * transform.forward;
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;

            if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, distanciaVision))
            {
                Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.red);
                if (hit.transform.gameObject.tag == "Food")
                {
                    distances[i] = hit.distance / distanciaVision;
                }
                else
                {
                    distances[i] = 1;
                }
            }
            else
            {
                Debug.DrawRay(rayStart, rayDirection * distanciaVision, Color.red);
                distances[i] = 1;
            }
        }
    }

    void ProcesarIA()
    {
        if (usuarioJugando)
        {
            FB = Input.GetAxis("Vertical");
            LR = Input.GetAxis("Horizontal") / 10f;
        }
        else
        {
            float[] outputsFromNN = nn.Brain(distances);
            FB = outputsFromNN[0];
            LR = outputsFromNN[1];
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Food" && !isDead)
        {
            energia += energiaObtenida;
            energiaReproduccion += 1f;
            Destroy(col.gameObject);
        }
    }

    public void GestionarEnergia()
    {
        elapsed += Time.deltaTime;

        if (elapsed >= 1f)
        {
            elapsed = 0f;
            energia -= 1f;

            if (energiaReproduccion >= energiaNecesariaParaReproduccion)
            {
                energiaReproduccion = 0f;
                Reproducir();
            }
        }

        // Morir por energía o caída
        float agentY = this.transform.position.y;
        if (energia <= 0 || agentY < -10)
        {
            Morir();
        }
    }

    void Morir()
    {
        isDead = true;
        GetComponent<Movement2>().enabled = false;
        this.transform.Rotate(0, 0, 180);
        Destroy(this.gameObject, 1f);
    }

    private void MutarCriatura()
    {
        if (activarMutaciones)
        {
            cantidadMutacion += Random.Range(-1.0f, 1.0f) / 100f;
            posibilidadMutacion += Random.Range(-1.0f, 1.0f) / 100f;
        }

        cantidadMutacion = Mathf.Max(cantidadMutacion, 0);
        posibilidadMutacion = Mathf.Max(posibilidadMutacion, 0);

        nn.MutateNetwork(cantidadMutacion, posibilidadMutacion);
        this.transform.localScale = Vector3.one * (1f + (cantidadMutacion - 0.8f));
    }

    public void Reproducir()
    {
        if (criaturaPrefab == null) return;

        Vector3 spawnPos = new Vector3(
            transform.position.x + Random.Range(-10, 11),
            0.75f,
            transform.position.z + Random.Range(-10, 11)
        );

        GameObject child = Instantiate(criaturaPrefab, spawnPos, Quaternion.identity);
        child.GetComponent<NN2>().layers = GetComponent<NN2>().copyLayers();

        energiaReproduccion = 0f;
    }
}