# Evolution of Intelligent Agents

TFG (Trabajo de Fin de Grado) — Simulador y análisis para la evolución de agentes inteligentes.

## Descripción
Este repositorio contiene el código y herramientas desarrolladas para el proyecto "Evolution of Intelligent Agents". Incluye un simulador (en la carpeta `TFG(Evolution Simulator)`) y una herramienta de visualización en tiempo real (`live_graph.py`) que recibe por UDP los valores de agentes y recursos (food) y crea gráficas de evolución.

## Características
- Simulador de evolución (código en `TFG(Evolution Simulator)`).
- Visualización en tiempo real de métricas (agentes, food) mediante `matplotlib`.
- Gráficas completas y de los últimos 200 datos para seguimiento dinámico.

## Requisitos
- Python 3.8+ (probado con 3.8/3.10)
- Dependencias:
  - matplotlib

Puedes instalarlas con pip:
```bash
python -m pip install -r requirements.txt
```
Si no existe `requirements.txt`, instala matplotlib directamente:
```bash
python -m pip install matplotlib
```

## Estructura del repositorio
- `TFG(Evolution Simulator)/` — Código del simulador (experimentos, agentes, entorno).
- `live_graph.py` — Script para visualizar en tiempo real la evolución de agentes y food.
- `.gitignore`
- `README.md` — (este archivo)

## Uso

### Visualización en tiempo real (live_graph.py)
`live_graph.py` abre un socket UDP en `127.0.0.1:5005` y espera paquetes con el formato:
- Mensaje: `"<agentes>;<food>"`
- Ejemplo válido: `100;50` o `100,5;47,2`  
  (El script acepta comas como separador decimal y las convierte internamente.)

Para ejecutar la visualización:
```bash
python live_graph.py
```
Se abrirá una ventana con 4 subplots:
- Agentes (completo)
- Food (completo)
- Agentes (últimos 200 datos)
- Food (últimos 200 datos)

El gráfico actualiza cada 50 ms y mantiene los últimos 200 puntos en las vistas parciales.

### Enviar datos de prueba por UDP
Ejemplo sencillo en Python para enviar datos de prueba:
```python
import socket
import time

UDP_IP = "127.0.0.1"
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

for t in range(300):
    agentes = 100 + t * 0.1
    food = 50 + (t % 50) * 0.2
    msg = f"{agentes:.2f};{food:.2f}"
    sock.sendto(msg.encode('utf-8'), (UDP_IP, UDP_PORT))
    time.sleep(0.05)
```

También puedes usar herramientas como `netcat` (si tu versión soporta UDP):
```bash
echo -n "120;45" | nc -u 127.0.0.1 5005
```

## Buenas prácticas
- Asegúrate de que solo haya un productor enviando a `127.0.0.1:5005` si ejecutas la visualización en la misma máquina.
- Si el simulador corre en otra máquina, actualiza `UDP_IP` en `live_graph.py` o envía a la IP de la máquina que ejecuta la visualización.
- Para guardar resultados, captura y serializa los datos del simulador en CSV/JSON desde el código del simulador.

## Contribuir
1. Haz fork del repositorio.
2. Crea una rama con tu mejora: `git checkout -b feat/mi-mejora`
3. Haz commit de tus cambios y abre un pull request.


## Contacto
Raúl Jiménez Ayza — rauljayza@gmail.com
