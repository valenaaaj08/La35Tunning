# Cambios v0.3.0: mejora visual (guía para aprender)

Este archivo explica **qué** se cambió, **por qué** y **qué concepto** hay detrás.
Todo lo nuevo en el código está marcado con comentarios `// NUEVO`, `// ANTES` / `// AHORA` o `// CAMBIO`.

> **Regla que seguimos:** no se borró la lógica del juego. Las clases `Semaforo`, `Taller`,
> `Concesionario`, `Jugador` y los componentes quedan **igual**. Se agregaron cosas, y se
> rehicieron las partes de **dibujo** (los métodos `Draw`) de cada pantalla.

---

## 0. Archivos tocados

| Archivo | Tipo de cambio |
|---|---|
| `Sistemas/UI.cs` | **NUEVO**: caja de herramientas de dibujo |
| `Content/FuenteTitulo.spritefont`, `Content/FuenteNegrita.spritefont` | **NUEVOS**: Arial Bold 40 y 20 |
| `Content/Content.mgcb` | se agregaron las 2 fuentes y `motor.png`, `turbo.png`, `neumatico.png` |
| `Entidades/Auto.cs` | ruedas bien puestas y girando, recorte de bordes, métodos de dibujo nuevos |
| `La35Tunning/Factories/AutoFactory.cs` | medidas exactas de llantas de cada auto |
| `Escenas/MenuPrincipal.cs` | diseño nuevo, centrado, hover, botón SALIR |
| `Escenas/PantallaConfiguracion.cs` | ventana centrada con barra de volumen |
| `Escenas/PantallaConcesionario.cs` | tarjetas con foto, estadísticas, layout centrado |
| `Escenas/PantallaTaller.cs` | paneles, pestañas, íconos, auto apoyado en el piso |
| `Escenas/PantallaCarrera.cs` | escenario completo, semáforo, HUD, resultados |
| `Sistemas/Camara2D.cs` | se agregó `Centro` y `MundoAPantalla()` |
| `Game1.cs` | conecta todo (fuentes, cámara, botones volver/salir) |

Para ver **cada línea** cambiada: abrí `cambios.diff`, o en Visual Studio usá *Git > Ver cambios*.

---

## 1. `UI.cs`: no repetir código de dibujo

**Problema:** cada pantalla tenía su propio `DibujarBoton`, su propio `_texturaPixel`, sus propios colores.

**Solución:** una clase `static` con todo lo común:

```csharp
UI.Panel(spriteBatch, rect, 0.9f, UI.Acento);          // panel con sombra y franja de color
UI.Boton(spriteBatch, rect, "VOLVER", mouseEncima);    // botón con hover
UI.TextoCentrado(spriteBatch, UI.Titulo, "TALLER", centro, UI.TextoClaro);
UI.Chip(spriteBatch, pos, "DINERO", UI.Dinero(7000000), UI.Verde);   // "$ 7.000.000"
```

**Concepto 1, paleta única:** todos los colores están arriba en `UI.cs`. Si querés que el amarillo
sea naranja, cambiás `UI.Acento` y cambia en todo el juego.

**Concepto 2, alfa premultiplicado:** MonoGame espera que los colores semitransparentes tengan
R, G y B ya multiplicados por el alfa. Por eso se escribe `Color.Black * 0.5f` (multiplica los 4
canales) y **no** `new Color(0, 0, 0, 128)`.

**Concepto 3, texturas generadas por código:** `Circulo`, `Brillo` (degradé radial) y
`DegradeVertical` se crean con `SetData` píxel por píxel, como tu `CrearTexturaEngranaje`.
Con eso salen las sombras de los autos, el brillo del semáforo, las luces de los faroles, etc.

**Concepto 4, `LimitesOpacos()`:** muchas imágenes traen mucho "aire" transparente alrededor
(ej: `gol.png` es 1536x1024 pero el auto ocupa 1478x500; `Taller.png` tiene 80 px vacíos a cada lado).
Esta función busca el rectángulo donde realmente hay dibujo y se usa como `sourceRectangle` al dibujar.
Así todos los autos y botones miden lo que tienen que medir.

**Concepto 5, `FondoCubriendo()`:** antes los fondos se estiraban a la pantalla. `FondoTaller.jpg`
es 4:3 y tu monitor 16:9, así que se veía aplastado. Ahora se recorta lo que sobra (como
`background-size: cover` en CSS) y la imagen mantiene su proporción.

---

## 2. Ruedas bien puestas y girando (`Auto.cs` + `AutoFactory.cs`)

### Qué estaba mal
1. Todos los autos usaban el **mismo diámetro** de llanta (`39.68`), pero cada sprite tiene pasaruedas de distinto tamaño.
2. Las posiciones estaban "a ojo" y no tenían en cuenta el borde transparente de cada imagen.
3. El giro era `velocidad * dt * 2`: la rueda giraba mucho más lento de lo que avanzaba el auto (parecía que patinaba).
4. El rival avanzaba con `AvanzarDistancia()` y **sus ruedas no giraban nunca**.
5. Los png de llantas no son cuadrados (ej: 209x226): al rotar, la rueda "bailaba".
6. El Fiat Uno tiene las ruedas **pintadas** en la imagen: la llanta nueva no las tapaba del todo.

### Cómo se arregló
- Se midió **cada pasarueda** de cada sprite (con un script que detecta el hueco oscuro/transparente
  y después verificando visualmente). Los 4 números nuevos en `AutoFactory`:
  ```csharp
  //                                                     llanta izq X, llanta der X, centro Y, DIÁMETRO
  new Auto("Fiat Uno", 7.5f, 0.18f, 3800000, texturaUno, null, 0.180f, 0.778f, 0.746f, 0.143f);
  ```
  X y diámetro son fracción del **ancho** de la imagen; Y es fracción del **alto**.
- **Física del giro:** una rueda que no patina, al avanzar una distancia `d`, gira `d / radio` radianes:
  ```csharp
  AnguloLlanta += distancia / radio;
  ```
- **Efecto "rueda de carreta":** si una rueda gira más de ~24° por cuadro, el ojo la ve girar para atrás
  (pasa en las películas). Se limita el giro visual por cuadro y, a alta velocidad, se dibuja una copia
  semitransparente para dar desenfoque de movimiento.
- La llanta se dibuja con **escala distinta en X y en Y** (`Vector2`) para que quede perfectamente redonda.
- `Draw()` en la carrera ahora usa `Posicion.Y` como **línea del piso**: todos los autos apoyan las
  ruedas en el asfalto aunque sus imágenes midan distinto.

### Métodos nuevos que podés usar
```csharp
auto.DibujarSobrePiso(spriteBatch, puntoDelPiso, anchoEnPixeles, Color.White);  // apoyado en un punto
auto.DibujarEnArea(spriteBatch, rectangulo, Color.White);                        // lo más grande que entre
auto.GirarLlantas(distancia);
```
El `Draw(spriteBatch, posicion, escala, escalaLlanta)` viejo se mantiene para no romper nada.

---

## 3. Menú principal

- **Antes:** posiciones fijas pensadas para una ventana de 800 px (`292 + desplazamientoX`), fondo estirado, botones con distinto "aire" transparente.
- **Ahora:**
  - `ActualizarRectangulos()` calcula todo según el tamaño real de la pantalla: **CORRER al medio y más grande**, TALLER a la izquierda, CONCESIONARIO a la derecha.
  - **Hover animado:** al pasar el mouse el botón crece un 6 %, sube un poquito y aparece un brillo. CORRER "late" suave para invitarte a jugar.
  - Engranaje en un botón redondo que **gira** con el mouse encima.
  - **Botón SALIR** (antes no había forma de cerrar el juego en pantalla completa).
  - **Barra de abajo** con piloto, auto equipado y dinero.
  - Fundido desde negro al entrar.
- **`Reiniciar()`**, concepto de "detección de flanco": un clic es *"ahora apretado Y antes suelto"*.
  Al volver de Configuración con el botón VOLVER, el mouse sigue apretado y el menú lo tomaba como un clic
  nuevo sobre el botón que estuviera debajo. `Reiniciar()` hace que el primer cuadro solo "lea" el mouse.

## 4. Configuración

Tu código ya tenía `ObtenerDesplazamiento()` pensado para centrar todo, pero devolvía `Vector2.Zero`.
Ahora devuelve el desplazamiento real para centrar un "lienzo" de 900x500. También se arregló
`Mover(Rectangle)`, que **solo sumaba X y se olvidaba de Y**.

Lo importante: los rectángulos (`RectBajarVolumen`, `RectVolver`…) se definen **una sola vez** y se
usan tanto para dibujar como para detectar el clic. Así nunca más "se ve en un lado y se clickea en otro".

Además: barra de volumen clickeable/arrastrable, se dibuja como ventana emergente sobre el menú.

## 5. Concesionario

- Layout centrado y adaptable (en 1366x768 las tarjetas se achican solas).
- Cada auto en una **tarjeta** con: vidriera iluminada con el auto **con sus llantas puestas** (antes se veía sin ruedas), precio, barras de **velocidad** y **aceleración**.
- Si no te alcanza: precio en rojo y "Te faltan $ X".
- El auto equipado tiene borde amarillo y la etiqueta "EN USO".
- Botón "< VOLVER" (además de ESC). Mensajes en verde (ok) o rojo (error).
- Las flechas de scroll se movieron al lado del título "EN VENTA".

## 6. Taller

- **Bug grave arreglado:** el auto se dibujaba en `(700, 700)` a escala 1, o sea con el tamaño de su
  imagen original (¡el Gol mide 1536 px!) y quedaba fuera de pantalla. Ahora se apoya **sobre la mancha
  oscura del piso** del fondo (se midió dónde está: 50 % horizontal, 80 % vertical) con `UI.PuntoDelFondo()`.
- Panel izquierdo con **pestañas clickeables** PIEZAS / LLANTAS.
- Piezas con ícono (`motor.png`, `turbo.png`, `neumatico.png`, ya estaban en Content pero no en el `.mgcb`),
  precio en amarillo, rojo si no alcanza, "INSTALADA" en verde si ya la tenés.
- Llantas en grilla con la elegida **girando** como en una vidriera y etiqueta "PUESTA".
- **Ficha técnica** a la derecha: potencia total, barras de velocidad y aceleración reales, piezas instaladas.
- Mensajes al instalar ("¡Instalaste Turbo Stage 1!", "No te alcanza la plata…"). Antes no avisaba nada.
- Concepto: los métodos `RectFilaPieza(i)`, `RectLlanta(i)` y `RectPestanaPiezas()` calculan las posiciones
  en **un solo lugar** y los usan `Update()` (clics) y `Draw()` (dibujo). Antes el clic usaba
  `new Rectangle(50, 270 + i * 25, …)`, que no coincidía con lo dibujado.

## 7. Carrera

### Escenario (antes: pantalla negra con rayas grises)
Se dibuja en **3 capas** (ver `Game1.DibujarCarrera()`):
1. **Fondo sin cámara:** cielo de atardecer, estrellas, sol y dos filas de edificios con **parallax**
   (los lejanos se mueven al 12 % de la velocidad, los cercanos al 45 %). Los edificios se generan con
   `UI.AzarFijo(i, semilla)`: un número "al azar" pero que **siempre da lo mismo para el mismo i**, así
   la calle no cambia cada cuadro.
2. **Mundo con cámara:** vereda, faroles con luz, carteles de 100/200/300 m, cartel de LARGADA, pórtico de META,
   cordones, asfalto con marcas de goma, líneas blancas, divisoria amarilla, línea de largada, **meta a cuadros**,
   humo de gomas al salir, los dos autos y el guardarraíl.
3. **HUD sin cámara:** tiempo, metros que faltan, dinero, mini-mapa con los dos autos, velocímetro.

### Cámara centrada
- Antes: `Zoom = 0.5` fijo y la cámara apuntaba a la **esquina** del auto.
- Ahora: `Zoom = altoPantalla / 760` (siempre se ven 760 unidades de alto: la pista ocupa lo mismo en
  cualquier monitor) y el auto queda a un tercio de la pantalla mirando hacia adelante.

### Semáforo (se usan los 6 sprites)
| Estado | Sprite | Qué muestra |
|---|---|---|
| `Luz1` | `semaforo1` | todo apagado, "3" |
| `Luz2` | `semaforo2` | pre-stage (luz de arriba), "2" |
| `Luz3` | `semaforo3` | ámbar a medio encender, "1" |
| `Luz4` | `semaforo4` | todos los ámbar (espera al azar) |
| `Verde` | `semaforo5` | verde, "¡VERDE - ARRANCAR!" |
| `Fallida` | `semaforoFallida` | cruz roja, también en la pantalla de resultado |

- **Antes:** cada sprite se estiraba a 220x360, pero miden distinto (99x298, 85x295, 89x300…), así que el
  semáforo **saltaba y se deformaba** en cada cambio, y el número grande **tapaba las luces**.
- **Ahora:** un solo semáforo, **parado en la vereda junto a la línea de largada**, recortado a su parte visible,
  escalado sin deformar y apoyado siempre en la misma base. El número va **al costado**, con un brillo del color
  de la luz prendida, y la ayuda va abajo.
- **CAMBIO de jugabilidad (importante):** antes la carrera arrancaba **0.8 s después** del verde y en ese
  tiempo apretar W no hacía nada: se perdía el **tiempo de reacción**, que según la propuesta es lo más
  importante. Ahora se arranca en el instante del verde y se mide la reacción.

### Resultado
Panel con GANASTE / PERDISTE / SALIDA ANTICIPADA, tu tiempo, el del rival, **tiempo de reacción**
(verde < 0.3 s, amarillo < 0.6 s, rojo más lento) y el premio.

### Detalles
- `Velocimetro.png` traía un **rectángulo gris semitransparente** pegado; se borra por código al cargar
  (`LimpiarFondoGrisDelTablero`). Se agregó la velocidad en números.
- Al entrar a Carrera siempre se reinicia la largada (antes, si volvías al menú y entrabas, quedaba el resultado viejo).

## 8. Bugs arreglados en `Game1.cs`
- `_texturaPixel.SetData(new[] { Color.Black })` pintaba de negro una textura que **comparten** otras
  pantallas. Se sacó (con `Color.Black * 0.5f` alcanza).
- Taller y Concesionario tienen botón VOLVER (`DebeVolver`), igual que Configuración.

---

## Cómo probar (checklist)
1. Compilar: `dotnet build` (o F5 en Visual Studio). Las fuentes nuevas usan **Arial**, que ya viene en Windows.
2. Menú: pasar el mouse por los 3 botones, el engranaje y SALIR.
3. Configuración: subir/bajar volumen con flechas, botones y arrastrando la barra.
4. Concesionario: comprar un auto, equiparlo, ver la tarjeta "EN USO".
5. Taller: probar las 6 llantas, instalar piezas, ver la ficha técnica.
6. Carrera: largar antes del verde (debe salir el semáforo con cruz), largar bien, ganar y perder.
7. Probar con los 4 autos que las ruedas queden en los pasaruedas y giren.

## Si algo no compila
Los cambios no se pudieron compilar en el entorno donde se hicieron (no había .NET instalado), así que se
revisaron a mano con mucho cuidado. Si aparece un error, el mensaje de Visual Studio dice archivo y línea:
mandámelo y lo arreglamos juntos.

## Ideas para seguir (próximos pasos)
- **Rival con aceleración real** (hoy avanza a velocidad fija 6.5 desde el primer cuadro) → base para la "IA fantasma".
- **Vender un auto** no lo devuelve al catálogo del concesionario.
- El HUD viejo decía "S = FRENAR", pero en carrera frenar es **soltar W** (la S solo funciona en `Auto.Update`, que no se usa en carrera).
- Sonidos: motor, cambio, semáforo.
- Red (Etapas 3-4 de la propuesta).
