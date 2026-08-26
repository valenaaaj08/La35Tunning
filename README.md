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
* **MonoGame 3.8.5** – Framework para el desarrollo del videojuego, encargado del renderizado, audio, entrada y ciclo principal del juego.
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

### Comandos de Compilación y Ejecución

Abrí una terminal y ejecutá:

1. git clone https://github.com/valenaaaj08/La35Tunning.git.
2. cd La35Tunning.
3. dotnet restore.
4. dotnet build.
5. dotnet run --project La35Tunning/La35Tunning.csproj.

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

**Avance actual:** el flujo Menú → Taller → Carrera funciona de punta a
punta en modo local (rival con velocidad fija, sin red todavía), con
cámara siguiendo al auto del jugador, HUD de tiempo/distancia, cartel
de resultado al terminar la carrera, y premio en dinero por ganar
(confirmado funcionando). Instalar piezas en el Taller ahora también
descuenta dinero real (recién aplicado, todavía sin confirmar con una
prueba). Pendiente: fondo/pista real (hoy solo hay marcas de distancia
como referencia visual temporal), pantalla de Concesionario, y todo el
módulo de red multijugador (Etapas 3-4 de la propuesta).


## 📖 Wiki
**https://github.com/valenaaaj08/La35Tunning/wiki**

