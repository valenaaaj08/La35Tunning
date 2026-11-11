# Changelog - La35Tunning

Este archivo documenta todos los hitos y mejoras significativas en el desarrollo de *La35Tunning*.

## [0.0.0] - 2026-08-14

### Arquitectura y Sistema Base
- **Implementación de Arquitectura de Escenas**: Creación de un sistema modular basado en `IPantallas.cs` y clases dedicadas (`MenuPrincipal.cs`, `PantallaCarrera.cs`, `PantallaTaller.cs`, `PantallaConcesionario.cs`).
- **Sistema de Gestión (Managers)**: Estructuración de la lógica mediante un sistema centralizado para controlar el flujo de juego (`EstadoJuego.cs`).
- **Implementación de Sistemas Core**:
    - **Camara2D**: Sistema de seguimiento de cámara para la vista lateral.
    - **Semaforo**: Lógica de tiempos y estados para la largada.
    - **Taller y Concesionario**: Sistemas de gestión de inventario y compra de vehículos.

### Componentes Mecánicos (Módulo `Componentes`)
- **Sistema de Física Vehicular**: Implementación de clases modulares para la personalización de autos:
    - `Motor.cs`: Cálculo de potencia y RPM.
    - `Transmision.cs`: Gestión de cambios.
    - `Turbo.cs` y `Intercooler.cs`: Modificadores de rendimiento.
    - `Neumatico.cs`: Cálculo de adherencia y fricción.
- **Entidades**: Definición de `Entidad.cs` y `Auto.cs` como base de todos los objetos dinámicos.

### Assets y Contenido
- **Gestión de Recursos**: Configuración completa de `Content.mgcb` para el manejo de:
    - **Sprites de Autos**: Implementación de modelos (`Uno`, `Gol`, `Corsa`, `Clio`).
    - **Interfaz de Usuario**: Sprites de velocímetro, aguja, fondos de menú y taller.
    - **Sistema de Semáforo**: Integración de estados visuales (`semaforo1.png` a `semaforo5.png`).
    - **Personalización**: Implementación de múltiples variantes de llantas (`llanta1` a `llanta5`).

### Mejoras Técnicas
- **Refactorización de `Game1.cs`**: Limpieza del ciclo de vida del juego (Update/Draw) delegando la lógica a las escenas correspondientes.
- **Estructuración de Proyectos**: Organización lógica en carpetas (`Entidades`, `Escenas`, `Sistemas`, `Modelos`, `Componentes`).

## [0.1.0] - 2026-08-24

### Bloque 0 — Correcciones de flujo y assets
- Se asigna un auto inicial (Fiat Uno) al jugador al arrancar, para poder probar el Taller.
- Se conectó el botón "Correr" del menú principal al nuevo estado `EstadoJuego.Carrera`.
- Se agregó el manejo de `EstadoJuego.Carrera` en `Game1.Update()` y `Game1.Draw()`, instanciando `PantallaCarrera` con un auto rival de prueba (Gol) y un `Semaforo`.
- Se corrigió `FuentePrincipal.spritefont`: se agregó un `CharacterRegion` (161-255) para soportar tildes y ñ, que antes rompían el juego con la excepción "Text contains characters that cannot be resolved".

### Bloque 1 — Cámara y escalado de autos (código aplicado, pendiente de confirmar en prueba)
- Se integró `Camera2D` en `Game1.Draw()` para el estado `Carrera`: los autos se dibujan con la transformación de cámara (siguiendo al auto del jugador) y el semáforo (HUD) se dibuja en un `SpriteBatch` separado, sin transformación de cámara.
- Se detectó que `Uno.png` (729x342) y `gol.png` (1536x1024) tienen resoluciones muy distintas, y `Auto.Draw()` las dibujaba a tamaño original sin escalar — el auto rival tapaba toda la pantalla, incluido el auto del jugador. Se agregó un escalado por ancho objetivo fijo (`AnchoDeseadoEnPantalla`) en `Auto.cs` para que todos los autos midan lo mismo en pantalla sin importar la resolución de su imagen original.
- **Sin confirmar todavía por prueba real** si el fix de escalado resuelve el problema visual.

### Pendiente / problemas conocidos
- No hay ningún fondo ni referencia visual en el mundo del juego durante la carrera (pantalla negra). Como la cámara centra siempre al auto del jugador, esto hace que el movimiento no se perciba aunque la posición del auto sí cambie internamente. Falta agregar una pista/fondo o marcas de distancia.
- No hay HUD de tiempo/distancia en pantalla durante la carrera.
- No hay pantalla de resultado visual al terminar la carrera (el resultado solo se imprime en la consola de depuración).
- Pantalla de Concesionario no implementada (archivo vacío).
- Sin red/multijugador todavía (Etapas 3-4 de la propuesta).

## [0.2.0] - 2026-08-25

### Bloque 1 — HUD, resultado visual y economía (confirmado por prueba)
- `PantallaCarrera.DibujarHud()`: agrega tiempo transcurrido y distancia restante en pantalla durante la carrera.
- `PantallaCarrera.Draw()`: agrega marcas de distancia cada 200 unidades (referencia visual temporal de movimiento, mientras no haya un fondo/pista real — ver sección "Pendiente").
- Cartel de resultado visible al terminar la carrera (ganaste / ganó el rival / descalificado por salida anticipada), con opción de reintentar con [ENTER]. Reemplaza el `MostrarResultado()` anterior que solo escribía en la consola de depuración.
- `PantallaCarrera` ahora recibe al `Jugador` y le suma un premio en dinero ($5.000) si gana la carrera.
- Se corrigió `PantallaTaller.Draw()`: el texto de "Dinero" y "Auto" dependía incorrectamente de que existiera `TexturaTaller` (imagen de frente del auto), que en el auto de prueba es `null` — antes de este fix, la plata nunca se veía en pantalla aunque sí se sumara por dentro.
- Ajustado el balance de prueba entre el auto del jugador y el rival de prueba, para que la carrera sea ganable.

### Bloque 1 — Economía del Taller (aplicado, pendiente de confirmar en prueba)
- `Taller.InstalarPieza()` ahora recibe al `Jugador` y descuenta el costo real de la pieza (antes se instalaba gratis). Si no alcanza la plata, no instala nada.

### Pendiente / problemas conocidos
- No hay fondo/pista real (las marcas de distancia son un parche temporal hasta tener el asset de fondo).
- Pantalla de Concesionario no implementada (archivo vacío).
- Sin red/multijugador todavía (Etapas 3-4 de la propuesta).

## [0.3.0] - 2026-11-05

### Mejora visual general (detalle completo y explicado en `CAMBIOS.md`)
- **Nuevo `Sistemas/UI.cs`**: paleta de colores única, paneles, botones con hover, chips, barras, texto con sombra, fondos sin deformar ("cover") y recorte automático del borde transparente de las imágenes.
- **Nuevas fuentes** `FuenteTitulo` (Arial Bold 40) y `FuenteNegrita` (Arial Bold 20). Se agregaron al `Content.mgcb` junto con `motor.png`, `turbo.png` y `neumatico.png`.
- **Ruedas**: medidas exactas por auto (posición y diámetro), giro físico `distancia / radio`, el rival también gira las ruedas, llantas siempre redondas, desenfoque de movimiento y autos apoyados sobre la línea del piso.
- **Menú**: botones centrados (CORRER al medio), hover animado, botón SALIR, barra con piloto/auto/dinero, fondo sin deformar.
- **Configuración**: centrada (se implementó `ObtenerDesplazamiento()`), barra de volumen clickeable, se muestra sobre el menú.
- **Concesionario**: tarjetas con foto del auto con llantas, precio y barras de estadísticas, layout centrado, botón VOLVER.
- **Taller**: auto apoyado sobre la mancha del piso del fondo (antes quedaba fuera de pantalla), pestañas, íconos, ficha técnica, mensajes al instalar.
- **Carrera**: escenario completo con parallax (cielo, edificios, faroles, carteles de distancia, largada, meta a cuadros, guardarraíl), cámara con zoom según la resolución, humo de gomas, HUD nuevo con mini-mapa, panel de resultado con tiempo de reacción.
- **Semáforo**: se usan los 6 sprites sin deformarse ni "saltar", parado en la pista junto a la largada, con el texto al costado.

### Correcciones
- La carrera arrancaba 0.8 s después del verde (se perdía el tiempo de reacción). Ahora arranca en el instante del verde.
- `Game1` pintaba de negro la textura de píxel compartida (`SetData(Color.Black)`).
- `Mover(Rectangle)` de Configuración no sumaba el desplazamiento en Y.
- Un clic al volver de otra pantalla podía activar un botón del menú o instalar una pieza sin querer (se agregó `Reiniciar()`).
- `Velocimetro.png` tenía un rectángulo gris semitransparente: se limpia al cargar.
- Al volver a entrar a Carrera siempre se reinicia la largada.

### Pendiente
- Rival con aceleración real / IA fantasma, sonidos, módulo de red.
- Compilar y probar en Windows (los cambios se revisaron a mano, sin compilador).

## [0.4.0] - 2026-11-11

### Juego en red (LAN), cliente-servidor
- **Nuevo proyecto `La35Tunning.Servidor`** (consola): junta a 2 jugadores, les manda el mismo semáforo, reenvía las posiciones, decide el ganador y da por ganador al que queda si el otro se desconecta. Rechaza tiempos imposibles (menos de 4 s).
- **Nuevo `Sistemas/ClienteRed.cs`**: conexión TCP con un hilo que lee los mensajes del servidor.
- **Nueva `Escenas/PantallaRed.cs`**: escribir la IP del servidor y esperar al rival.
- Menú: botón **JUGAR EN RED (LAN)**. `EstadoJuego.Red`.
- `PantallaCarrera`: modo en red (el rival se mueve con lo que manda el servidor y el resultado lo decide el servidor). `Semaforo.ReiniciarConEspera()` y `Auto.MoverA()`.

### Caja de cambios y nombres
- **5 marchas sin embrague**: ESPACIO sube un cambio. Cada marcha tiene un tope de velocidad y empuja menos cuanto más alta es. El tablero marca las RPM, muestra la marcha, avisa "¡CAMBIÁ!" y califica cada cambio (perfecto / bueno / temprano).
- **Carrera más larga**: `DistanciaMeta` pasa de 2800 a 4400 (unos 12 s) y cada marcha dura alrededor de 1 s. El rival de la máquina ahora acelera de a poco.
- **Nombre del jugador**: se elige en la pantalla JUGAR EN RED (antes siempre era "Valentin"). Lo ven el rival y el servidor.

### Correcciones
- Si ya compraste una pieza y te pusiste otra, no podías volver a ponerte la primera. Ahora se vuelve a poner gratis y el taller muestra PUESTA / COMPRADA.
