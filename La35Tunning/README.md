# Guía rápida para entender C# y MonoGame en este proyecto

Este documento está pensado para que vos puedas entender lo que hace cada parte del juego y poder defenderlo si te preguntan en clase. No hace falta saber todo de memoria; con esta guía podés explicar de forma lógica lo que tu proyecto hace.

---

## 1. Qué es C#

C# es un lenguaje de programación orientado a objetos. Eso significa que todo se organiza en clases, objetos y métodos.

Algunas cosas básicas que se usan mucho en este proyecto son:

- Clase: una plantilla para crear objetos.
- Objeto: una instancia de una clase.
- Método: una función dentro de una clase.
- Propiedad: una variable con acceso controlado.
- Atributo: una variable interna de una clase.

Ejemplo simple:

```csharp
public class Auto
{
    public string Modelo;

    public void Acelerar()
    {
        Console.WriteLine("El auto acelera");
    }
}
```

Aquí:

- `Auto` es la clase.
- `Modelo` es un atributo.
- `Acelerar()` es un método.

En un juego, las clases representan cosas reales o virtuales del juego, como autos, pantallas, jugadores, cámaras, semáforos, etc.

---

## 2. Qué es MonoGame

MonoGame es una librería para crear videojuegos con C#. Permite dibujar cosas en pantalla, detectar teclado, manejar música, texturas, animaciones y lógica del juego.

Es como una base para hacer videojuegos 2D/3D en C#.

Lo importante es entender que MonoGame te da muchas herramientas ya hechas, por ejemplo:

- dibujar imágenes en pantalla,
- escuchar teclado,
- manejar la resolución,
- controlar música,
- leer el tiempo del juego,
- mover objetos con posiciones en pantalla.

En otras palabras: vos escribís la lógica del juego, y MonoGame te da las herramientas para dibujar y actualizar todo.

---

## 3. El ciclo de vida de un juego en MonoGame

Todo juego tiene un flujo muy parecido. En MonoGame, la clase principal normalmente hereda de `Game`.

### `Initialize()`
Se ejecuta una vez al empezar el juego. Acá se crean cosas iniciales, variables, estado general del juego.

```csharp
protected override void Initialize()
{
    _jugador = new Jugador("Valentin", 7000000m);
    base.Initialize();
}
```

Qué hace: crea al jugador principal al iniciar el juego.

### `LoadContent()`
Se ejecuta una vez después de `Initialize()`. Acá se cargan imágenes, fuentes, música y recursos del juego.

```csharp
_fuente = Content.Load<SpriteFont>("FuentePrincipal");
```

Qué hace: carga una fuente para dibujar texto.

### `Update(GameTime gameTime)`
Se ejecuta cada frame. Acá va la lógica del juego: mover objetos, detectar teclado, cambiar pantallas, revisar si ganó o perdió.

```csharp
_pantallaCarrera?.Update(gameTime);
```

Qué hace: actualiza la pantalla de carrera cada frame.

### `Draw(GameTime gameTime)`
Se ejecuta cada frame para dibujar los elementos en pantalla.

```csharp
_spriteBatch.Begin();
_menuPrincipal.Draw(_spriteBatch, _fuente, GraphicsDevice);
_spriteBatch.End();
```

Qué hace: empieza a dibujar y luego dibuja el menú.

---

## 4. Las clases y funciones más importantes que usamos

### `Game`
Es la clase base de MonoGame. Todo juego empieza desde una clase que hereda de `Game`.

En este proyecto la clase principal es `Game1`:

```csharp
public class Game1 : Game
```

Eso significa que `Game1` tiene todos los métodos y comportamientos básicos de un juego.

### `GraphicsDeviceManager`
Controla la resolución y la ventana del juego.

```csharp
_graphics = new GraphicsDeviceManager(this);
```

Qué hace: define cómo se ve la ventana y si está en pantalla completa.

### `SpriteBatch`
Es la herramienta que usa MonoGame para dibujar imágenes en pantalla.

```csharp
_spriteBatch = new SpriteBatch(GraphicsDevice);
```

Qué hace: comienza un lote de dibujado, dibuja todo y al final termina.

Ejemplo:

```csharp
_spriteBatch.Begin();
_spriteBatch.Draw(_texturaAuto, _posicion, Color.White);
_spriteBatch.End();
```

### `Texture2D`
Representa una imagen cargada en memoria, como un auto, un fondo, un semáforo, etc.

```csharp
Texture2D texturaLlantaDefault = Content.Load<Texture2D>("llantaDefault");
```

### `SpriteFont`
Representa una fuente para dibujar texto.

```csharp
_fuente = Content.Load<SpriteFont>("FuentePrincipal");
```

### `Keyboard`
Se usa para detectar qué teclas preseiona el usuario.

```csharp
if (Keyboard.GetState().IsKeyDown(Keys.Escape))
```

Qué hace: si apreta Escape, vuelve al menú.

### `GameTime`
Representa el tiempo del juego. Se usa para controlar velocidad, animaciones y deltas.

```csharp
(float)gameTime.ElapsedGameTime.TotalSeconds
```

Esto te da cuánto tiempo pasó entre un frame y el otro.

### `Vector2`
Representa una posición en 2D: X e Y.

```csharp
new Vector2(100, 200)
```

Se usa para ubicar objetos en pantalla.

### `Rectangle`
Representa un rectángulo con posición y tamaño.

```csharp
new Rectangle(0, 0, 800, 600)
```

Se usa mucho para dibujar botones, fondos y rectángulos.

### `Color`
Representa un color.

```csharp
Color.Red
Color.White
Color.Black * 0.5f
```

Se usa para pintar texto, fondo, botones y overlays.

---

## 5. Cómo está organizado este proyecto

El proyecto tiene varias partes:

### `Game1.cs`
Es la clase principal. Controla el flujo general del juego:

- qué pantalla está activa,
- qué se dibuja,
- qué se actualiza,
- cómo cambia entre menú, taller, concesionario y carrera.

Es como el “director general” del juego.

### `Escenas/`
Acá están las pantallas del juego.

- `MenuPrincipal.cs`: menú inicial.
- `PantallaCarrera.cs`: la carrera.
- `PantallaTaller.cs`: mejora del auto.
- `PantallaConcesionario.cs`: compra de autos.
- `PantallaConfiguracion.cs`: configuración.
- `IPantallas.cs`: interfaz que define que todas las pantallas deben tener `Update` y `Draw`.

### `Entidades/`
Acá están los objetos del juego.

- `Auto.cs`: define cómo se comporta un auto.
- `Entidad.cs`: base para entidades del juego.

### `Modelos/`
Acá va la lógica de negocio del jugador y del estado del juego.

- `Jugador.cs`: dinero, auto actual, datos del jugador.
- `EstadoJuego.cs`: enum que indica en qué pantalla está el juego.

### `Sistemas/`
Acá van sistemas del juego.

- `Camara2D.cs`: cámara para seguir al auto.
- `Semaforo.cs`: lógica del semáforo.
- `Taller.cs`: manejo del taller.
- `Concesionario.cs`: catalogo de autos.

### `Componentes/`
Acá están las piezas del auto.

- `Motor.cs`
- `Turbo.cs`
- `Transmision.cs`
- `Intercooler.cs`
- `Neumatico.cs`

Estas piezas modifican el rendimiento del auto.

---

## 6. Explicación de los archivos más importantes del proyecto

### `Game1.cs`
Es el archivo más importante porque conecta todo.

Tiene estas funciones principales:

- `Initialize()`: prepara el juego al iniciar.
- `LoadContent()`: carga imágenes, música y fuentes.
- `Update()`: actualiza la pantalla actual.
- `Draw()`: dibuja la pantalla actual.
- `CambiarEstado()`: cambia de menú a taller, carrera, concesionario, etc.

En palabras simples:

> `Game1` decide qué pantalla está activa y hace que todo el juego funcione en conjunto.

### `Auto.cs`
Es la clase que representa el auto del jugador y del rival.

Tiene cosas como:

- la posición del auto,
- velocidad actual,
- aceleración,
- piezas instaladas,
- si llegó a la meta,
- si fue descalificado.

También tiene métodos como:

- `ActualizarEnCarrera()`: decide cómo se mueve el auto mientras corre.
- `AvanzarDistancia()`: mueve al auto por una distancia fija.
- `ReiniciarParaCarrera()`: vuelve el auto a la línea de salida.
- `Draw()`: dibuja el auto en pantalla.

### `PantallaCarrera.cs`
Es la pantalla de carrera.

Hace estas cosas:

- controla el semáforo,
- detecta si el jugador salió antes de tiempo,
- mueve al rival,
- calcula si alguien llegó a la meta,
- decide si el jugador ganó dinero.

El flujo es:

1. el semáforo arranca,
2. el auto espera a que esté en verde,
3. la carrera empieza,
4. si llega a la meta, termina la carrera.

### `Jugador.cs`
Es el jugador del juego.

Guarda cosas como:

- nombre,
- dinero,
- auto actual.

Es la clase que representa al usuario del juego.

---

## 7. Qué significa cada función del ciclo principal

### `Update()`
Es la parte donde se calcula la lógica del juego. Acá no se dibuja nada; solo se decide qué pasa.

Ejemplos:

- si el jugador apreta Escape,
- si se compró un auto,
- si se debe cambiar de pantalla,
- si el semáforo ya está en verde.

### `Draw()`
Es la parte donde se dibuja en pantalla todo lo que ya se calculó en `Update()`.

Ejemplos:

- el fondo,
- los botones,
- los autos,
- el semáforo,
- mensajes de texto.

### `LoadContent()`
Es para cargar recursos del juego antes de empezar a dibujar y actualizar.

Si no se cargan ahí, no podrían aparecer los autos, el menú ni el texto.

### `Initialize()`
Es la preparación inicial. Se crea el estado básico del juego para que después cargue todo.

---

## 8. Forma simple de explicar el proyecto en defensa

Si te preguntan qué hace este proyecto, podés decir algo así:

> Este juego es un simulador de tuning de autos en 2D hecho con C# y MonoGame. El juego tiene un menú principal, un concesionario para comprar autos, un taller para mejorar piezas, una pantalla de configuración y una carrera. La clase principal `Game1` controla en qué pantalla está el usuario y hace que el juego cambie entre estados. Los autos tienen velocidad, aceleración y piezas como motor, turbo, neumáticos y transmisión. Cuando el jugador entra a carrera, se activa el semáforo y el auto debe llegar a la meta sin salir antes de tiempo. MonoGame se encarga de dibujar todo en pantalla y detectar eventos del teclado.

Eso te sirve como respuesta breve pero clara.

---

## 9. Dificultades comunes y cómo entenderlas

### Por qué hay muchas clases
Porque cada cosa del juego se separa en una clase distinta. Así el código queda ordenado.

### Por qué hay `Update()` y `Draw()` en varias pantallas
Porque cada pantalla tiene su propio comportamiento. Por ejemplo, la pantalla del taller tiene su propia lógica, y la carrera tiene otra.

### Por qué hay `public class ... : Game`
Porque se está usando la base de MonoGame para crear el juego.

### Por qué hay `Content.Load<T>()`
Porque MonoGame carga todos los recursos de forma organizada desde la carpeta `Content`.

---

## 10. Resumen corto para estudiar

Si te querés aprender esto en una semana, lo más importante es tener presente esto:

- C# = lenguaje para programar.
- MonoGame = librería para crear videojuegos con C#.
- `Game1` = clase principal del juego.
- `Initialize()` = prepara el juego.
- `LoadContent()` = carga imágenes, música y texto.
- `Update()` = lógica del juego.
- `Draw()` = dibujar en pantalla.
- `SpriteBatch` = dibuja objetos.
- `Texture2D` = imagen.
- `Keyboard` = teclado.
- `Vector2` = posición.
- `Rectangle` = rectángulo.
- `GameTime` = tiempo del juego.

Con eso ya podés empezar a defender y explicar el proyecto con mucha más seguridad.

---

## 11. Sugerencia final

Si tenés que explicar tu proyecto en clase, no trates de memorizar todo el código. Hablá de la idea general:

- qué hace el juego,
- qué hace cada pantalla,
- cómo cambia de estado,
- cómo se dibuja un auto,
- cómo funciona la carrera,
- y por qué cada archivo tiene una tarea diferente.

Eso es lo más importante.

---

Si querés, después te puedo hacer una segunda versión de este documento enfocada solo en tu proyecto exacto, con una explicación línea por línea de `Game1.cs`, `Auto.cs` y `PantallaCarrera.cs` para que te lo aprendas de verdad antes de la defensa.
