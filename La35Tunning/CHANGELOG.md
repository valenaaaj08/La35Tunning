# Changelog - La35Tunning

Este archivo documenta todos los hitos y mejoras significativas en el desarrollo de *La35Tunning*.

## [1.0.0] - 2026-08-14

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