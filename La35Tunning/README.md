# La35Tunning 🚗💨

Videojuego de carreras de aceleración (drag racing) en 2D con enfoque multijugador en red local.

Proyecto desarrollado para la materia Laboratorio y Programación de 6.º año en la Escuela Técnica Nº 35 D.E. 18 "Ing. Eduardo Latzina" (Especialidad: Computación / Automotores).

---

## 👥 Integrantes del Grupo

* Valentin Von Korff
* Valentino Tropea
* Mauro Zielinski

---

## 📝 Descripción del Juego

La35Tunning es un videojuego de carreras de aceleración (drag racing) en 2D con cámara fija y vista lateral, inspirado en la cultura automovilística de las picadas urbanas argentinas.

Los jugadores compiten en emocionantes duelos de 400 metros sobre escenarios inspirados en lugares emblemáticos de Buenos Aires, como la Av. Lope de Vega, utilizando vehículos clásicos muy populares en Argentina, entre ellos el Fiat Uno, Volkswagen Gol G3 y Chevrolet Corsa.

La jugabilidad del MVP se centra en la precisión del piloto: controlar correctamente el embrague, acelerar en el momento justo cuando se apaga el semáforo para evitar falsas largadas y administrar una economía virtual que permite comprar mejoras como motores, neumáticos y turbocompresores para optimizar el rendimiento del vehículo.

El modo multijugador se implementa mediante una arquitectura cliente-servidor, garantizando la sincronización de la partida y la integridad de la competencia en tiempo real.

---

## 🛠️ Tecnologías Utilizadas

El proyecto utiliza un conjunto de tecnologías modernas orientadas al desarrollo de videojuegos de escritorio.

* **C#** – Lenguaje de programación principal.
* **MonoGame 3.8.1** – Framework para el desarrollo del videojuego, encargado del renderizado, audio, entrada y ciclo principal del juego.
* **.NET 8** – Plataforma de ejecución.
* **Visual Studio 2022** – Entorno de desarrollo (IDE).
* **Sockets TCP/UDP** – Comunicación de red para el modo multijugador.

---

## 🚀 Cómo Compilar y Ejecutar

### Prerrequisitos

Antes de comenzar, asegurate de tener instalado:

* .NET 8 SDK
* Visual Studio 2022 con la carga de trabajo Desarrollo para el escritorio con .NET.
* MonoGame MGCB Editor (generalmente se instala junto con las plantillas de MonoGame).

### Clonar el repositorio

Abrí una terminal y ejecutá:

git clone [https://github.com/TU_USUARIO/La35Tunning.git](https://github.com/TU_USUARIO/La35Tunning.git)
cd La35Tunning

Abrí una terminal y ejecutá:

> **Nota:** Reemplazá `TU_USUARIO` por el nombre del propietario del repositorio.

### 2. Abrir el proyecto

1. Abrí el archivo `.sln` con **Visual Studio 2022**.
2. Esperá a que se restauren automáticamente las dependencias de **NuGet**.
3. Compilá la solución presionado <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>B</kbd>.

### 3. Ejecutar el juego

- Presioná <kbd>F5</kbd> para iniciar el juego en modo depuración.
- Presioná <kbd>Ctrl</kbd> + <kbd>F5</kbd> para ejecutarlo sin depuración.

---

## 🎮 Características Principales

- 🏁 **Carreras de aceleración:** Desafíos de 400 metros.
- 🚘 **Autos clásicos:** Vehículos inspirados en leyendas de la calle argentina.
- 🔧 **Tuning y Mejoras:** Sistema completo de personalización y mejoras de rendimiento.
- 💰 **Economía Virtual:** Ganá dinero compitiendo y comprá nuevas piezas.
- 🌐 **Arquitectura Cliente-Servidor:** Sistema robusto para partidas multijugador en red local (LAN).
- 🖥️ **Interfaz con MonoGame:** Desarrollo gráfico ligero y ágil.

---

## 📌 Estado del Proyecto

🚧 **En desarrollo.**  
Actualmente se encuentra implementando el MVP (*Producto Mínimo Viable*) para la entrega de la materia **Laboratorio y Programación**.

## 📖 Wiki
**https://github.com/valenaaaj08/La35Tunning/wiki**