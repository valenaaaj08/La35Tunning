using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using La35Tunning.Modelos;
using La35Tunning.Sistemas;

namespace La35Tunning.Entidades
{
    public class Auto : Entidad
    {
        private string _modelo;
        private float _velocidadMaximaBase;
        private float _aceleracionBase;
        private float _velocidadActual = 0f;
        private int _precio;

        private Texture2D _texturaAuto;
        private Texture2D _texturaTaller;
        private Texture2D _llantaDelantera;
        private Texture2D _llantaTrasera;
        public float AnguloLlanta { get; set; } = 0f;

        private Motor _motorActual;
        private Turbo _turboActual;
        private Transmision _transmisionActual;
        private Intercooler _intercoolerActual;
        private Neumatico _neumaticoActual;
        private readonly HashSet<Componente> _piezasCompradas = new HashSet<Componente>();

        private const float AnchoDeseadoEnPantalla = 300f;
        private const float DiametroLlantaEnPantalla = 39.68f;

        private float _posicionLlantaDelanteraX = 0.24f;
        private float _posicionLlantaTraseraX = 0.82f;
        private float _posicionLlantasY = 0.64f;

        // NUEVO: diámetro de la llanta como fracción del ANCHO de la imagen del auto.
        // Cada sprite tiene los pasaruedas de distinto tamaño, así que cada auto
        // necesita el suyo (se mide una vez y se pasa desde AutoFactory).
        private float _diametroLlanta;

        public int Precio { get { return _precio; } }
        public string Modelo { get { return _modelo; } }
        public Texture2D TexturaAuto { get { return _texturaAuto; } }
        public Texture2D TexturaTaller { get { return _texturaTaller; } }
        public float VelocidadActual { get { return _velocidadActual; } }

        public Motor MotorActual { get { return _motorActual; } }
        public Turbo TurboActual { get { return _turboActual; } }
        public Transmision TransmisionActual { get { return _transmisionActual; } }
        public Intercooler IntercoolerActual { get { return _intercoolerActual; } }
        public Neumatico NeumaticoActual { get { return _neumaticoActual; } }

        // NUEVO: datos que necesitan las pantallas para mostrar estadísticas y la llanta puesta.
        public float VelocidadMaximaBase { get { return _velocidadMaximaBase; } }
        public float AceleracionBase { get { return _aceleracionBase; } }
        public float VelocidadMaximaReal { get { return CalcularVelocidadMaximaReal(); } }
        public float AceleracionReal { get { return CalcularAceleracionReal(); } }
        public Texture2D LlantaDelantera { get { return _llantaDelantera; } }

        public const float DistanciaMeta = 2800f;
        private const float PosicionLargada = 100f;

        // NUEVO: para que la pista sepa dónde pintar la línea de largada y la de meta.
        public const float AnchoEnCarrera = AnchoDeseadoEnPantalla;
        public const float FrenteEnLargada = PosicionLargada + AnchoDeseadoEnPantalla;   // trompa del auto al largar
        public const float FrenteEnMeta = FrenteEnLargada + DistanciaMeta;               // trompa del auto al llegar

        public bool Descalificado { get; private set; } = false;
        public bool LlegoAMeta { get; private set; } = false;

        public float ProgresoCarrera
        {
            get
            {
                float avance = (_posicion.X - PosicionLargada) / DistanciaMeta;
                return MathHelper.Clamp(avance, 0f, 1f);
            }
        }

        public Auto(string modelo, float velocidadBase, float aceleracionBase, int precio, Texture2D textura, Texture2D texturaTaller = null,
                    float posicionDelanteraX = 0.24f, float posicionTraseraX = 0.82f, float posicionLlantasY = 0.64f,
                    float diametroLlanta = 0f)
        {
            _modelo = modelo;
            _velocidadMaximaBase = velocidadBase;
            _aceleracionBase = aceleracionBase;
            _texturaAuto = textura;
            _texturaTaller = texturaTaller;
            _precio = precio;
            _posicion = new Vector2(100, 200);

            _posicionLlantaDelanteraX = posicionDelanteraX;
            _posicionLlantaTraseraX = posicionTraseraX;
            _posicionLlantasY = posicionLlantasY;

            // Si no nos pasan el diámetro, usamos la medida vieja (39.68 px sobre un auto de 300 px).
            _diametroLlanta = diametroLlanta > 0f ? diametroLlanta : DiametroLlantaEnPantalla / AnchoDeseadoEnPantalla;

            _motorActual = new Motor("Motor de Fábrica", 1.0f, 0);
            _turboActual = new Turbo("Sin Turbo (Stock)", 1.0f, 0);
            _transmisionActual = new Transmision("Transmisión de Fábrica", 1.0f, 0);
            _intercoolerActual = new Intercooler("Sin Intercooler", 1.0f, 0);
            _neumaticoActual = new Neumatico("Neumáticos de Fábrica", 1.0f, 0);
        }

        public void InstalarLlantas(Texture2D llantaDelantera, Texture2D llantaTrasera)
        {
            _llantaDelantera = llantaDelantera;
            _llantaTrasera = llantaTrasera;
        }

        public List<Componente> ObtenerComponentesInstalados()
        {
            var lista = new List<Componente>();
            if (_motorActual != null) lista.Add(_motorActual);
            if (_turboActual != null) lista.Add(_turboActual);
            if (_transmisionActual != null) lista.Add(_transmisionActual);
            if (_intercoolerActual != null) lista.Add(_intercoolerActual);
            if (_neumaticoActual != null) lista.Add(_neumaticoActual);
            return lista;
        }

        public float MultiplicadorMotorTotal
        {
            get
            {
                float acumulador = 1.0f;
                foreach (var pieza in ObtenerComponentesInstalados())
                {
                    acumulador *= pieza.MultiplicadorRendimiento;
                }
                return acumulador;
            }
        }

        public void InstalarPieza(Componente nuevaPieza)
        {
            if (nuevaPieza == null || _piezasCompradas.Contains(nuevaPieza))
                return;

            if (nuevaPieza is Motor motor)
            {
                _motorActual = motor;
            }
            else if (nuevaPieza is Turbo turbo)
            {
                _turboActual = turbo;
            }
            else if (nuevaPieza is Transmision transmision)
            {
                _transmisionActual = transmision;
            }
            else if (nuevaPieza is Intercooler intercooler)
            {
                _intercoolerActual = intercooler;
            }
            else if (nuevaPieza is Neumatico neumatico)
            {
                _neumaticoActual = neumatico;
            }

            _piezasCompradas.Add(nuevaPieza);
        }

        public bool TienePiezaComprada(Componente pieza)
        {
            return pieza != null && _piezasCompradas.Contains(pieza);
        }

        public void MostrarFichaTecnica()
        {
            System.Console.WriteLine($"--- Ficha Técnica: {_modelo} ---");
            foreach (var pieza in ObtenerComponentesInstalados())
            {
                System.Console.WriteLine($"- {pieza.Nombre} (Mod: {pieza.MultiplicadorRendimiento})");
            }
        }

        public int CalcularValorTotal()
        {
            int valorTotal = Precio;
            foreach (var componente in ObtenerComponentesInstalados())
            {
                valorTotal += componente.Costo;
            }
            return valorTotal;
        }

        public void AvanzarDistancia(float distancia)
        {
            if (Descalificado || LlegoAMeta)
                return;

            _posicion.X += distancia;
            _velocidadActual = distancia;   // NUEVO: así el rival también "tiene velocidad" (humo, HUD)
            GirarLlantas(distancia);        // NUEVO: antes las ruedas del rival no giraban

            if (_posicion.X >= PosicionLargada + DistanciaMeta)
            {
                LlegoAMeta = true;
            }
        }

        public void Descalificar()
        {
            Descalificado = true;
        }

        public void ReiniciarParaCarrera()
        {
            _posicion.X = PosicionLargada;
            _velocidadActual = 0f;
            _ultimoGiro = 0f;
            Descalificado = false;
            LlegoAMeta = false;
        }

        private float CalcularAceleracionReal()
        {
            return _aceleracionBase * MultiplicadorMotorTotal;
        }

        private float CalcularVelocidadMaximaReal()
        {
            return _velocidadMaximaBase * MultiplicadorMotorTotal;
        }

        private bool SeQuiereAcelerar()
        {
            var teclado = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            return teclado.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W);
        }

        private void Acelerar(float aceleracion, float velocidadMaxima)
        {
            _velocidadActual += aceleracion;

            if (_velocidadActual > velocidadMaxima)
            {
                _velocidadActual = velocidadMaxima;
            }
        }

        private void Frenar()
        {
            _velocidadActual -= 0.05f;

            if (_velocidadActual < 0f)
            {
                _velocidadActual = 0f;
            }
        }

        private void MoverAuto(GameTime gameTime)
        {
            _posicion.X += _velocidadActual;

            if (_velocidadActual != 0)
            {
                // ANTES: AnguloLlanta += velocidad * dt * 2  (giraba mucho más lento de lo que avanzaba,
                //        parecía que las ruedas "patinaban" sobre el asfalto).
                // AHORA: una rueda que rueda sin patinar gira (distancia / radio) radianes.
                GirarLlantas(_velocidadActual);
            }
            else
            {
                _ultimoGiro = 0f;
            }

            if (_posicion.X >= PosicionLargada + DistanciaMeta)
            {
                LlegoAMeta = true;
            }
        }

        public bool ActualizarEnCarrera(GameTime gameTime, bool semaforoEnVerde)
        {
            if (!semaforoEnVerde)
            {
                return SeQuiereAcelerar();
            }

            if (Descalificado || LlegoAMeta)
            {
                return false;
            }

            float aceleracionReal = CalcularAceleracionReal();
            float velocidadMaximaReal = CalcularVelocidadMaximaReal();

            if (SeQuiereAcelerar())
            {
                Acelerar(aceleracionReal, velocidadMaximaReal);
            }
            else
            {
                Frenar();
            }

            MoverAuto(gameTime);
            return false;
        }

        public override void Update(GameTime gameTime)
        {
            float aceleracionReal = CalcularAceleracionReal();
            float velocidadMaximaReal = CalcularVelocidadMaximaReal();

            if (SeQuiereAcelerar())
            {
                Acelerar(aceleracionReal, velocidadMaximaReal);
            }
            else if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
            {
                _velocidadActual -= 0.2f;
                if (_velocidadActual < 0f) _velocidadActual = 0f;
            }
            else
            {
                Frenar();
            }

            MoverAuto(gameTime);
        }

        // ==================================================================
        // DIBUJO (reescrito para que las llantas queden en su lugar y giren bien)
        // ==================================================================

        // Más de ~24 grados por cuadro y el ojo ve el "efecto rueda de carreta"
        // (parece que la rueda gira para atrás). Por eso limitamos el giro visual
        // y a alta velocidad dibujamos un poco de desenfoque de movimiento.
        private const float GiroVisualMaximoPorCuadro = 0.42f;
        private float _ultimoGiro;

        /// <summary>Parte visible del sprite (sin el borde transparente que traen las imágenes).</summary>
        private Rectangle LimitesOpacos { get { return UI.LimitesOpacos(_texturaAuto); } }

        /// <summary>Escala necesaria para que la CARROCERÍA mida "ancho" píxeles.</summary>
        public float EscalaParaAncho(float ancho)
        {
            return ancho / LimitesOpacos.Width;
        }

        /// <summary>Radio de la llanta en la carrera (donde el auto mide AnchoDeseadoEnPantalla).</summary>
        public float RadioLlantaEnCarrera
        {
            get { return _diametroLlanta * _texturaAuto.Width * EscalaParaAncho(AnchoDeseadoEnPantalla) / 2f; }
        }

        /// <summary>
        /// Hace girar las llantas como si el auto hubiera rodado "distancia" unidades.
        /// Física: una rueda que no patina gira (distancia / radio) radianes.
        /// </summary>
        public void GirarLlantas(float distancia)
        {
            float radio = RadioLlantaEnCarrera;
            if (radio <= 0.01f)
                return;

            float giro = MathHelper.Clamp(distancia / radio, -GiroVisualMaximoPorCuadro, GiroVisualMaximoPorCuadro);
            AnguloLlanta = (AnguloLlanta + giro) % MathHelper.TwoPi;
            _ultimoGiro = Math.Abs(giro);
        }

        /// <summary>
        /// Cuánto mide el auto desde el techo hasta donde las ruedas tocan el piso
        /// (con la escala indicada). Sirve para "apoyar" el auto sobre una línea de piso.
        /// </summary>
        public float AlturaHastaPiso(float escala)
        {
            Rectangle limites = LimitesOpacos;
            float fondoCarroceria = limites.Height;
            float fondoLlanta = _posicionLlantasY * _texturaAuto.Height - limites.Top
                                + _diametroLlanta * _texturaAuto.Width / 2f;
            return Math.Max(fondoCarroceria, fondoLlanta) * escala;
        }

        /// <summary>
        /// En la carrera: Posicion.X = la cola del auto, Posicion.Y = la línea del piso del carril.
        /// Así todos los autos quedan apoyados en el asfalto aunque sus imágenes midan distinto.
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 puntoPiso = new Vector2(_posicion.X + AnchoDeseadoEnPantalla / 2f, _posicion.Y);
            DibujarSobrePiso(spriteBatch, puntoPiso, AnchoDeseadoEnPantalla, Color.White, true);
        }

        /// <summary>
        /// Dibuja el auto con su carrocería de "ancho" píxeles, centrado en X sobre
        /// puntoPiso y con las ruedas tocando puntoPiso.Y.
        /// </summary>
        public void DibujarSobrePiso(SpriteBatch spriteBatch, Vector2 puntoPiso, float ancho, Color tinte, bool conSombra = true)
        {
            float escala = EscalaParaAncho(ancho);
            float alto = AlturaHastaPiso(escala);

            if (conSombra)
            {
                // Sombra ovalada debajo del auto: lo "pega" al piso.
                UI.Resplandor(spriteBatch, new Vector2(puntoPiso.X, puntoPiso.Y - 2f),
                    ancho * 0.58f, Math.Max(8f, ancho * 0.065f), Color.Black * 0.65f);
            }

            Vector2 esquina = new Vector2(puntoPiso.X - ancho / 2f, puntoPiso.Y - alto);
            DibujarCompleto(spriteBatch, esquina, escala, tinte, 1f);
        }

        /// <summary>
        /// Dibuja el auto lo más grande posible dentro de "area", centrado y apoyado
        /// en el borde de abajo. Ideal para miniaturas (concesionario) y el taller.
        /// </summary>
        public void DibujarEnArea(SpriteBatch spriteBatch, Rectangle area, Color tinte, bool conSombra = true)
        {
            Rectangle limites = LimitesOpacos;
            float escalaPorAncho = area.Width / (float)limites.Width;
            float escalaPorAlto = area.Height / AlturaHastaPiso(1f);
            float escala = Math.Min(escalaPorAncho, escalaPorAlto);

            DibujarSobrePiso(spriteBatch, new Vector2(area.Center.X, area.Bottom), limites.Width * escala, tinte, conSombra);
        }

        /// <summary>
        /// Se mantiene por compatibilidad con el código anterior:
        /// "posicion" es la esquina de la imagen COMPLETA (con su borde transparente).
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Vector2 posicion, float escalaDibujo, float escalaLlantaDibujo)
        {
            Rectangle limites = LimitesOpacos;
            Vector2 esquina = posicion + new Vector2(_texturaAuto.Width - limites.Right, limites.Top) * escalaDibujo;
            DibujarCompleto(spriteBatch, esquina, escalaDibujo, Color.White, escalaLlantaDibujo);
        }

        /// <summary>Carrocería + 2 llantas. "esquina" = esquina sup. izq. de la parte visible de la carrocería.</summary>
        private void DibujarCompleto(SpriteBatch spriteBatch, Vector2 esquina, float escala, Color tinte, float multiplicadorLlanta)
        {
            // Dibujamos solo la parte visible (LimitesOpacos) y dada vuelta para que mire a la derecha.
            spriteBatch.Draw(_texturaAuto, esquina, LimitesOpacos, tinte, 0f, Vector2.Zero, escala,
                SpriteEffects.FlipHorizontally, 0f);

            if (_llantaDelantera == null || _llantaTrasera == null)
                return;

            float diametro = _diametroLlanta * _texturaAuto.Width * escala * multiplicadorLlanta;

            // Las llantas se dibujan DESPUÉS de la carrocería, así tapan el pasarueda oscuro
            // (y en el Fiat Uno tapan las ruedas que ya venían pintadas en la imagen).
            DibujarLlanta(spriteBatch, _llantaTrasera, CentroLlanta(_posicionLlantaDelanteraX, esquina, escala), diametro, tinte);
            DibujarLlanta(spriteBatch, _llantaDelantera, CentroLlanta(_posicionLlantaTraseraX, esquina, escala), diametro, tinte);
        }

        /// <summary>
        /// Pasa la posición de la llanta (fracción medida sobre la imagen tal como se VE,
        /// o sea ya dada vuelta) a coordenadas de pantalla.
        /// </summary>
        private Vector2 CentroLlanta(float fraccionX, Vector2 esquina, float escala)
        {
            Rectangle limites = LimitesOpacos;
            float xEnImagenDadaVuelta = fraccionX * _texturaAuto.Width;
            float bordeIzquierdoDadoVuelta = _texturaAuto.Width - limites.Right;
            float yEnImagen = _posicionLlantasY * _texturaAuto.Height;

            return esquina + new Vector2(xEnImagenDadaVuelta - bordeIzquierdoDadoVuelta, yEnImagen - limites.Top) * escala;
        }

        private void DibujarLlanta(SpriteBatch spriteBatch, Texture2D llanta, Vector2 centro, float diametro, Color tinte)
        {
            Rectangle limites = UI.LimitesOpacos(llanta);

            // El origen es el centro de la llanta: así gira sobre su eje y no "orbita".
            Vector2 origen = new Vector2(limites.Width / 2f, limites.Height / 2f);

            // Escala distinta en X y en Y: los png de llantas no son cuadrados (ej: 209x226),
            // así forzamos que la rueda quede redonda y no se bambolee al girar.
            Vector2 escala = new Vector2(diametro / limites.Width, diametro / limites.Height);

            spriteBatch.Draw(llanta, centro, limites, tinte, AnguloLlanta, origen, escala, SpriteEffects.None, 0f);

            if (_ultimoGiro > 0.2f)
            {
                // Desenfoque de movimiento: una copia semitransparente un poquito más atrás en el giro.
                spriteBatch.Draw(llanta, centro, limites, tinte * 0.4f, AnguloLlanta - _ultimoGiro * 0.5f, origen, escala, SpriteEffects.None, 0f);
            }
        }
    }
}