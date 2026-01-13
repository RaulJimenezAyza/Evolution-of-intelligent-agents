#Archivo para crear las gráficas de análisis
import socket
import matplotlib.pyplot as plt
import matplotlib.animation as animation

# Configuración UDP
UDP_IP = "127.0.0.1"
UDP_PORT = 5005

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))
sock.setblocking(False)  # No bloqueante

# Buffers
xs = []
ys_agents = []
ys_food = []
t = 0

# Configurar figura con 4 subplots
fig, ((ax1, ax2), (ax3, ax4)) = plt.subplots(2, 2, figsize=(12, 8))

# Líneas
line_agents_full, = ax1.plot([], [], linewidth=2, color="blue", label="Agentes")
line_food_full, = ax2.plot([], [], linewidth=2, color="green", label="Food")
line_agents_last, = ax3.plot([], [], linewidth=2, color="blue", label="Agentes últimos 200 datos")
line_food_last, = ax4.plot([], [], linewidth=2, color="green", label="Food últimos 200 datos")

# Configuración de ejes
for ax, ylabel, title in zip([ax1, ax2, ax3, ax4],
                             ["Agentes", "Food", "Agentes", "Food"],
                             ["Agentes (completo)", "Food (completo)",
                              "Agentes (últimos 200 datos)", "Food (últimos 200 datos)"]):
    ax.grid(True, linestyle='--', alpha=0.4)
    ax.set_ylabel(ylabel)
    ax.set_title(title)
    ax.legend()

ax3.set_xlabel("Tiempo")
ax4.set_xlabel("Tiempo")
ax1.set_xlabel("Tiempo")
ax2.set_xlabel("Tiempo")

# Máximo número de puntos para las gráficas parciales
max_recent = 200

def update(frame):
    global t

    # Intentar recibir dato
    try:
        data, addr = sock.recvfrom(1024)
        values = [float(v.replace(',', '.')) for v in data.decode().split(';')]
        agents = values[0]
        food = values[1]

        xs.append(t)
        ys_agents.append(agents)
        ys_food.append(food)
        t += 1
    except BlockingIOError:
        pass

    # Actualizar líneas
    line_agents_full.set_data(xs, ys_agents)
    line_food_full.set_data(xs, ys_food)
    
    # Últimos 200 puntos
    xs_recent = xs[-max_recent:]
    ys_agents_recent = ys_agents[-max_recent:]
    ys_food_recent = ys_food[-max_recent:]
    line_agents_last.set_data(xs_recent, ys_agents_recent)
    line_food_last.set_data(xs_recent, ys_food_recent)

    # Ajustar ejes dinámicamente
    def autoscale(ax, xdata, ydata):
        if ydata:
            ymin = min(ydata)
            ymax = max(ydata)
            margin = (ymax - ymin) * 0.1 if ymax != ymin else 1
            ax.set_xlim(xdata[0], xdata[-1])
            ax.set_ylim(ymin - margin, ymax + margin)

    autoscale(ax1, xs, ys_agents)
    autoscale(ax2, xs, ys_food)
    autoscale(ax3, xs_recent, ys_agents_recent)
    autoscale(ax4, xs_recent, ys_food_recent)

    return line_agents_full, line_food_full, line_agents_last, line_food_last

# Crear animación
ani = animation.FuncAnimation(fig, update, interval=50, blit=False)
plt.tight_layout()
plt.show()
