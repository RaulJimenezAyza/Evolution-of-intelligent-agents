using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
public class ObjectTracker : MonoBehaviour
{
    [Header("Cada cuántos segundos actualizar estadísticas")]
    public float updateInterval = 2f;

    [Header("Referencia al TextMeshProUGUI donde mostrar los datos")]
    public TextMeshProUGUI statsText;

    UdpClient client;
    public string ip = "127.0.0.1";
    public int port = 5005;

    float timer = 0f;

    // =============================
    // LISTAS HISTÓRICAS
    // =============================
    List<float> historialTiempos = new List<float>();
    private void Start()
    {
        client = new UdpClient();
    }
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            timer = 0f;
            ActualizarEstadisticas();
        }
    }

    void ActualizarEstadisticas()
    {
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");

        int agentesVivos = agents.Length;
        int comidaActiva = foods.Length;
        SendValue(agentesVivos + ";" + comidaActiva);
        // --- REGISTRAR TIEMPOS DE VIDA (HISTÓRICOS) ---
        foreach (GameObject a in agents)
        {
            float vida = a.GetComponent<Creature>().tiempoDeVida;

            if (!historialTiempos.Contains(vida))
                historialTiempos.Add(vida);
        }

        // ------------------------------------------------------------------
        // SI NO HAY AGENTES VIVOS
        // ------------------------------------------------------------------
        if (agentesVivos == 0)
        {
            if (historialTiempos.Count == 0)
            {
                statsText.text = "No hay datos todavía.";
                return;
            }

            statsText.text =
                $"<b>Máx histórico:</b> {FormatoTiempo(historialTiempos.Max())}\n" +
                $"<b>Mín histórico:</b> {FormatoTiempo(historialTiempos.Min())}\n" +
                $"<b>Media histórica:</b> {FormatoTiempo(historialTiempos.Average())}";

            return;
        }

        // ================================================================
        // DATOS ACTUALES
        // ================================================================
        float[] vidas = agents
            .Select(a => a.GetComponent<Creature>().tiempoDeVida)
            .ToArray();

        float vidaMinActual = vidas.Min();
        float vidaMaxActual = vidas.Max();
        float vidaMediaActual = vidas.Average();

        // ================================================================
        // DATOS HISTÓRICOS
        // ================================================================
        float vidaMinHistorica = historialTiempos.Min();
        float vidaMaxHistorica = historialTiempos.Max();
        float vidaMediaHistorica = historialTiempos.Average();

        // ================================================================
        // MOSTRAR EN PANTALLA
        // ================================================================
        statsText.text =
            "<b>TIEMPO DE VIDA ACTUAL</b>\n" +
            $"Máx actual: {FormatoTiempo(vidaMaxActual)}\n" +
            $"Mín actual: {FormatoTiempo(vidaMinActual)}\n" +
            $"Media actual: {FormatoTiempo(vidaMediaActual)}\n" +

            "<b>HISTÓRICO TOTAL</b>\n" +
            $"Máx histórico: {FormatoTiempo(vidaMaxHistorica)}\n" +
            $"Mín histórico: {FormatoTiempo(vidaMinHistorica)}\n" +
            $"Media histórica: {FormatoTiempo(vidaMediaHistorica)}";
    }

    // ============================================================
    // FORMATEADOR DE TIEMPO
    // ============================================================
    string FormatoTiempo(float segundos)
    {
        int total = Mathf.FloorToInt(segundos);

        int horas = total / 3600;
        int minutos = (total % 3600) / 60;
        int secs = total % 60;

        if (horas > 0)
            return $"{horas} h {minutos:D2} min {secs:D2} s";

        if (minutos > 0)
            return $"{minutos} min {secs:D2} s";

        return $"{secs} s";
    }
    public void SendValue(string v)
    {
        byte[] data = Encoding.UTF8.GetBytes(v);
        client.Send(data, data.Length, ip, port);
    }

    void OnApplicationQuit()
    {
        client.Close();
    }
}
