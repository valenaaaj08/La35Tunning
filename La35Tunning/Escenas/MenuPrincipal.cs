using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace La35Tunning.Escenas
{
    public class MenuPrincipal
    {
        private MouseState _mouseAnterior;
        private Texture2D _fondoMenu;

        // Texturas para los botones
        private Texture2D _texturaTaller;
        private Texture2D _texturaConcesionario;
        private Texture2D _texturaCorrer;
        private Texture2D _texturaPixel;
        private Texture2D _texturaEngranaje;
        private bool _mouseSobreConfiguracion;
        private readonly GraphicsDevice _graphicsDevice;

        // Rectángulos de posición y tamaño en pantalla para cada botón
        // Movidos más abajo y centrados
        private Rectangle _rectBotonCorrer = new Rectangle(292, 380, 225, 105);
        private Rectangle _rectBotonConcesionario = new Rectangle(292, 500, 225, 105);
        private Rectangle _rectBotonTaller = new Rectangle(292, 620, 225, 105);
        private Rectangle _rectBotonConfiguracion; // se calcula en el constructor, según el ancho de pantalla

        public EstadoJuego? SiguienteEstado { get; private set; }

        public MenuPrincipal(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            // El menú se encarga de cargar sus propias imágenes
            _fondoMenu = content.Load<Texture2D>("FondoMenu");
            _texturaTaller = content.Load<Texture2D>("Taller");
            _texturaConcesionario = content.Load<Texture2D>("Concesionario");
            _texturaCorrer = content.Load<Texture2D>("Correr");
            _texturaPixel = new Texture2D(graphicsDevice, 1, 1);
            _texturaPixel.SetData(new[] { Color.White });

            // El engranaje lo generamos por código, no es un archivo del Content Pipeline
            _texturaEngranaje = CrearTexturaEngranaje(graphicsDevice, 64);

            // Arriba a la derecha, con 10px de margen respecto al borde de la pantalla
            int anchoPantalla = graphicsDevice.PresentationParameters.BackBufferWidth;
            _rectBotonConfiguracion = new Rectangle(anchoPantalla - 74, 10, 64, 64);
        }

        public void Update(GameTime gameTime)
        {
            ActualizarRectangulos();
            MouseState mouseActual = Mouse.GetState();
            bool hizoClic = (mouseActual.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released);

            _mouseSobreConfiguracion = _rectBotonConfiguracion.Contains(mouseActual.Position);

            SiguienteEstado = null;

            if (hizoClic)
            {
                Point posicionMouse = mouseActual.Position;

                if (_rectBotonTaller.Contains(posicionMouse))
                {
                    SiguienteEstado = EstadoJuego.Taller;
                }
                else if (_rectBotonConcesionario.Contains(posicionMouse))
                {
                    SiguienteEstado = EstadoJuego.Concesionario;
                }
                // Si agregas lógica para Correr, lo manejás acá

                else if (_rectBotonCorrer.Contains(posicionMouse))
                {
                    SiguienteEstado = EstadoJuego.Carrera;
                }
                else if (_rectBotonConfiguracion.Contains(posicionMouse))
                {
                    SiguienteEstado = EstadoJuego.Configuracion;
                }
            }


            _mouseAnterior = mouseActual;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente, GraphicsDevice graphicsDevice)
        {
            ActualizarRectangulos();
            // 1. Dibujar fondo estirado a toda la pantalla
            if (_fondoMenu != null)
            {
                spriteBatch.Draw(_fondoMenu, new Rectangle(0, 0, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
            }

            // 2. Dibujar las imágenes de los botones
            if (_texturaTaller != null)
                spriteBatch.Draw(_texturaTaller, _rectBotonTaller, Color.White);

            if (_texturaConcesionario != null)
                spriteBatch.Draw(_texturaConcesionario, _rectBotonConcesionario, Color.White);

            if (_texturaCorrer != null)
                spriteBatch.Draw(_texturaCorrer, _rectBotonCorrer, Color.White);

            // Ícono de configuración arriba a la derecha. Se pone dorado cuando el mouse pasa por encima.
            Color colorEngranaje = _mouseSobreConfiguracion ? Color.Gold : Color.White;
            spriteBatch.Draw(_texturaEngranaje, _rectBotonConfiguracion, colorEngranaje);
        }

        private void ActualizarRectangulos()
        {
            int desplazamientoX = Math.Max(0, (_graphicsDevice.Viewport.Width - 800) / 2);
            _rectBotonTaller.X = 292 + desplazamientoX;
            _rectBotonConcesionario.X = 292 + desplazamientoX;
            _rectBotonCorrer.X = 292 + desplazamientoX;
            _rectBotonConfiguracion.X = _graphicsDevice.Viewport.Width - 74;
        }

        // Arma a mano un ícono de engranaje como Texture2D, pixel por pixel.
        // La idea es parecida a recorrer una matriz en Java: por cada pixel (x,y) del cuadrado
        // de "tamano x tamano", calculamos qué tan lejos está del centro (distancia) y en qué
        // ángulo está, y con eso decidimos si ese pixel es parte del "cuerpo" del engranaje,
        // de un "diente", del agujero del medio, o si va transparente.
        private Texture2D CrearTexturaEngranaje(GraphicsDevice graphicsDevice, int tamano)
        {
            Color[] datos = new Color[tamano * tamano];
            Vector2 centro = new Vector2(tamano / 2f, tamano / 2f);

            float radioExterior = tamano * 0.46f; // hasta acá llegan los dientes
            float radioInterior = tamano * 0.32f; // hasta acá llega el cuerpo entre diente y diente
            float radioAgujero = tamano * 0.14f;  // agujero del centro
            const int cantidadDientes = 8;

            for (int y = 0; y < tamano; y++)
            {
                for (int x = 0; x < tamano; x++)
                {
                    Vector2 punto = new Vector2(x, y) - centro;
                    float distancia = punto.Length();
                    float angulo = (float)System.Math.Atan2(punto.Y, punto.X);

                    // Cos(angulo * cantidadDientes) va subiendo y bajando entre -1 y 1 a medida
                    // que angulo da la vuelta completa: eso genera los "picos" de los dientes.
                    bool estaEnUnDiente = System.Math.Cos(angulo * cantidadDientes) > 0.2f;
                    float radioLimite = estaEnUnDiente ? radioExterior : radioInterior;

                    bool esCuerpoDelEngranaje = distancia <= radioLimite;
                    bool esAgujeroCentral = distancia <= radioAgujero;

                    datos[y * tamano + x] = (esCuerpoDelEngranaje && !esAgujeroCentral)
                        ? Color.White
                        : Color.Transparent;
                }
            }

            Texture2D textura = new Texture2D(graphicsDevice, tamano, tamano);
            textura.SetData(datos);
            return textura;
        }
    }
}