# La35Tunning 🚗💨
> **Videojuego de carreras de aceleración (drag racing) en 2D con enfoque multijugador en red local.**

Proyecto desarrollado para la materia **Programación Sobre Redes** de 6.º año en la **Escuela Técnica Nº 35 D.E. 18 "Ing. Eduardo Latzina"** (Especialidad: Computación).

---

## 👥 Integrantes del Grupo
* **Valentin Von korff**
* **Valentino Tropea**
* **Mauro Zielinski**

---

## 📝 Descripción del Juego
**La35Tunning** es un videojuego de carreras de aceleración (*drag racing*) en 2D con perspectiva de cámara fija y vista lateral, fuertemente inspirado en la cultura automovilística de las picadas urbanas argentinas. Los jugadores se enfrentan en apasionantes duelos de 400 metros sobre escenarios locales icónicos, como la *Av. Lope de Vega*, compitiendo a bordo de clásicos populares de la calle argentina como el *Fiat Uno*, *VW Gol G3* y *Chevrolet Corsa*. 

La jugabilidad del MVP se centra en la precisión: el momento exacto de aceleración en el semáforo para evitar salidas anticipadas (descalificación), y una correcta administración de la economía virtual para comprar piezas (motor, neumáticos, turbos) y tunear el rendimiento del vehículo en el garaje. Todo coordinado y validado en tiempo real mediante una robusta arquitectura cliente-servidor para garantizar la integridad competitiva.

---

## 🛠️ Tecnologías Utilizadas
El proyecto se construye sobre un stack moderno orientado al desarrollo de videojuegos de alto rendimiento en escritorio:
* **C#**: Lenguaje de programación principal bajo paradigma orientado a objetos.
* **MonoGame (v3.8.1)**: Framework de desarrollo de videojuegos en 2D para la gestión de gráficos, sonido, entrada y ciclo de juego (*Game Loop*).
* **.NET 8**: Plataforma de ejecución de software.
* **Visual Studio**: Entorno de desarrollo integrado (IDE) principal.

---

## 🚀 Cómo Compilar y Ejecutar

Seguí estos pasos para clonar el repositorio, compilar el código y ejecutar el juego en tu máquina local:

### Prerrequisitos
Antes de comenzar, asegurate de tener instalado en tu sistema:
1. [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. [Visual Studio 2022](https://visualstudio.microsoft.com/es/vs/) (con la carga de trabajo **"Desarrollo para el escritorio con .NET"** activa).
3. [MonoGame MGCB Editor](https://docs.monogame.net/articles/tools/mgcb_editor.html) (suele instalarse automáticamente con las plantillas de MonoGame).

### Pasos
1. **Clonar el repositorio:**
   Abre **Git Bash** (o tu terminal preferida) y ejecuta el siguiente comando:
   ```bash
   git clone [https://github.com/valenaaaj08/La35Tunning.git](https://github.com/valenaaaj08/La35Tunning.git)
   cd La35Tunning
