using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using La35Tunning.Entidades;
using La35Tunning.Sistemas;
using La35Tunning.Modelos;

namespace La35Tunning.Escenas
{
    public class PantallaTaller
    {
        private Taller _taller;
        private Jugador _jugador;

        private int _indiceSeleccionado = 0;
        private int _indiceLlantaSeleccionada = 0;
        private bool _modoLlantas = false;
        private KeyboardState _tecladoAnterior;
        private MouseState _mouseAnterior;

        private Texture2D _fondoTaller;

        private Vector2 _posicionDibujoAuto = new Vector2(700, 700);
        private float _escalaDibujoAuto = 1.0f;
        private float _escalaLlantasTaller = 2.0f;

        // ---- NUEVO ----
        // Íconos de piezas (ya estaban en Content/, ahora se cargan en el Content.mgcb).
        private Texture2D _iconoMotor;
        private Texture2D _iconoTurbo;
        private Texture2D _iconoNeumatico;

        private int _anchoPantalla = 1920;     // se actualizan en Draw() con el tamaño real
        private int _altoPantalla = 1080;
        private Point _posicionMouse;
        private bool _entradaInicializada;
        private float _rotacionLlantaVitrina;  // la llanta elegida gira en la vitrina
        private string _mensaje = "";
        private bool _mensajeEsError;
        private float _tiempoMensaje;
        private Rectangle _rectVolver;

        // Posición de la mancha oscura del piso dentro de FondoTaller.jpg (medida sobre la imagen).
        private const float PisoU = 0.503f;
        private const float PisoV = 0.80f;
        private const float AnchoAutoSobreFondo = 0.32f;   // el auto ocupa el 32% del ancho de la imagen

        /// <summary>NUEVO: true cuando tocan VOLVER (Game1 vuelve al menú).</summary>
        public bool DebeVolver { get; private set; }

        public PantallaTaller(ContentManager content, Jugador jugador)
        {
            _taller = new Taller(content);
            _jugador = jugador;
            _fondoTaller = content.Load<Texture2D>("FondoTaller");

            _iconoMotor = CargarOpcional(content, "motor");
            _iconoTurbo = CargarOpcional(content, "turbo");
            _iconoNeumatico = CargarOpcional(content, "neumatico");
        }

        /// <summary>Si la imagen no está en el Content.mgcb devolvemos null en vez de romper el juego.</summary>
        private static Texture2D CargarOpcional(ContentManager content, string nombre)
        {
            try
            {
                return content.Load<Texture2D>(nombre);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"No se pudo cargar {nombre}: {ex.Message}");
                return null;
            }
        }

        /// <summary>NUEVO: evita que el clic que nos trajo desde el menú instale una pieza sin querer.</summary>
        public void Reiniciar()
        {
            _entradaInicializada = false;
            DebeVolver = false;
            _tiempoMensaje = 0f;
        }

        public void Update(GameTime gameTime)
        {
            var teclado = Keyboard.GetState();
            var mouse = Mouse.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _posicionMouse = mouse.Position;
            _rotacionLlantaVitrina += dt * 1.6f;
            if (_tiempoMensaje > 0f) _tiempoMensaje -= dt;

            if (!_entradaInicializada)
            {
                _tecladoAnterior = teclado;
                _mouseAnterior = mouse;
                _entradaInicializada = true;
                return;
            }

            bool hizoClic = mouse.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released;

            if (hizoClic && _rectVolver.Contains(_posicionMouse))
            {
                DebeVolver = true;
            }

            if (_jugador.AutoActual != null)
            {
                if (teclado.IsKeyDown(Keys.P) && _tecladoAnterior.IsKeyUp(Keys.P))
                {
                    _modoLlantas = false;
                }
                if (teclado.IsKeyDown(Keys.L) && _tecladoAnterior.IsKeyUp(Keys.L))
                {
                    _modoLlantas = true;
                }

                // NUEVO: también se puede cambiar de pestaña con el mouse.
                if (hizoClic && RectPestanaPiezas().Contains(_posicionMouse)) _modoLlantas = false;
                if (hizoClic && RectPestanaLlantas().Contains(_posicionMouse)) _modoLlantas = true;

                if (_modoLlantas)
                {
                    if ((teclado.IsKeyDown(Keys.Right) || teclado.IsKeyDown(Keys.Down)) &&
                        ( _tecladoAnterior.IsKeyUp(Keys.Right) && _tecladoAnterior.IsKeyUp(Keys.Down)))
                    {
                        _indiceLlantaSeleccionada++;
                        if (_indiceLlantaSeleccionada >= _taller.CatalogoLlantas.Count)
                            _indiceLlantaSeleccionada = 0;
                    }
                    else if ((teclado.IsKeyDown(Keys.Left) || teclado.IsKeyDown(Keys.Up)) &&
                        (_tecladoAnterior.IsKeyUp(Keys.Left) && _tecladoAnterior.IsKeyUp(Keys.Up)))
                    {
                        _indiceLlantaSeleccionada--;
                        if (_indiceLlantaSeleccionada < 0)
                            _indiceLlantaSeleccionada = _taller.CatalogoLlantas.Count - 1;
                    }
                    else if (teclado.IsKeyDown(Keys.Enter) && _tecladoAnterior.IsKeyUp(Keys.Enter))
                    {
                        PonerLlantaSeleccionada();
                    }

                    if (hizoClic)
                    {
                        for (int i = 0; i < _taller.CatalogoLlantas.Count; i++)
                        {
                            // ANTES: new Rectangle(50, 270 + (i * 25), 250, 22) -> no coincidía con lo dibujado.
                            if (RectLlanta(i).Contains(_posicionMouse))
                            {
                                _indiceLlantaSeleccionada = i;
                                PonerLlantaSeleccionada();
                            }
                        }
                    }
                }
                else
                {
                    if (teclado.IsKeyDown(Keys.Down) && _tecladoAnterior.IsKeyUp(Keys.Down))
                    {
                        _indiceSeleccionado = (int)MathHelper.Clamp(_indiceSeleccionado + 1, 0, _taller.CatalogoPiezas.Count - 1);
                    }
                    else if (teclado.IsKeyDown(Keys.Up) && _tecladoAnterior.IsKeyUp(Keys.Up))
                    {
                        _indiceSeleccionado = (int)MathHelper.Clamp(_indiceSeleccionado - 1, 0, _taller.CatalogoPiezas.Count - 1);
                    }
                    else if (teclado.IsKeyDown(Keys.Enter) && _tecladoAnterior.IsKeyUp(Keys.Enter))
                    {
                        InstalarPiezaSeleccionada();
                    }

                    if (hizoClic)
                    {
                        for (int i = 0; i < _taller.CatalogoPiezas.Count; i++)
                        {
                            // ANTES: new Rectangle(50, 270 + (i * 25), 350, 22)
                            if (RectFilaPieza(i).Contains(_posicionMouse))
                            {
                                _indiceSeleccionado = i;
                                InstalarPiezaSeleccionada();
                            }
                        }
                    }
                }
            }

            _tecladoAnterior = teclado;
            _mouseAnterior = mouse;
        }

        /// <summary>NUEVO: instala y además avisa qué pasó (antes no había ningún mensaje).</summary>
        private void InstalarPiezaSeleccionada()
        {
            Componente pieza = _taller.CatalogoPiezas[_indiceSeleccionado];

            if (_jugador.AutoActual.TienePiezaComprada(pieza))
            {
                MostrarMensaje($"El {pieza.Nombre} ya está instalado.", true);
                return;
            }

            bool instalada = _taller.InstalarPieza(_jugador.AutoActual, _indiceSeleccionado, _jugador);
            if (instalada)
                MostrarMensaje($"¡Instalaste {pieza.Nombre}!", false);
            else
                MostrarMensaje("No te alcanza la plata para esa pieza.", true);
        }

        private void PonerLlantaSeleccionada()
        {
            _taller.CambiarLlantas(_jugador.AutoActual, _indiceLlantaSeleccionada, _indiceLlantaSeleccionada);
            MostrarMensaje($"Llantas {_taller.NombresLlantas[_indiceLlantaSeleccionada]} puestas.", false);
        }

        private void MostrarMensaje(string texto, bool esError)
        {
            _mensaje = texto;
            _mensajeEsError = esError;
            _tiempoMensaje = 2.5f;
        }

        // ==================================================================
        // LAYOUT: un solo lugar donde se calculan las posiciones.
        // Lo usan Update() (para los clics) y Draw() (para dibujar),
        // así lo que se ve y lo que se toca SIEMPRE coinciden.
        // ==================================================================

        private Rectangle RectPanelIzquierdo()
        {
            int ancho = (int)MathHelper.Clamp(_anchoPantalla * 0.27f, 380f, 500f);
            return new Rectangle(24, 24, ancho, _altoPantalla - 24 - 84);
        }

        private Rectangle RectPanelDerecho()
        {
            return new Rectangle(_anchoPantalla - 24 - 340, 24, 340, 380);
        }

        private Rectangle RectPestanaPiezas()
        {
            Rectangle panel = RectPanelIzquierdo();
            int ancho = (panel.Width - 48 - 12) / 2;
            return new Rectangle(panel.X + 24, panel.Y + 112, ancho, 44);
        }

        private Rectangle RectPestanaLlantas()
        {
            Rectangle piezas = RectPestanaPiezas();
            return new Rectangle(piezas.Right + 12, piezas.Y, piezas.Width, piezas.Height);
        }

        private int InicioLista()
        {
            return RectPestanaPiezas().Bottom + 18;
        }

        private Rectangle RectFilaPieza(int indice)
        {
            Rectangle panel = RectPanelIzquierdo();
            int disponible = panel.Bottom - 56 - InicioLista();
            int alto = (int)MathHelper.Clamp(disponible / (float)_taller.CatalogoPiezas.Count - 4f, 28f, 44f);
            return new Rectangle(panel.X + 16, InicioLista() + indice * (alto + 4), panel.Width - 32, alto);
        }

        private Rectangle RectLlanta(int indice)
        {
            Rectangle panel = RectPanelIzquierdo();
            const int columnas = 2;
            int filas = (_taller.CatalogoLlantas.Count + columnas - 1) / columnas;
            int disponible = panel.Bottom - 56 - InicioLista();

            int ancho = (panel.Width - 32 - 12) / columnas;
            int alto = (int)MathHelper.Clamp(disponible / (float)filas - 12f, 110f, 190f);

            int columna = indice % columnas;
            int fila = indice / columnas;
            return new Rectangle(panel.X + 16 + columna * (ancho + 12), InicioLista() + fila * (alto + 12), ancho, alto);
        }

        // ==================================================================
        // DIBUJO
        // ==================================================================

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente)
        {
            if (_jugador == null) return;

            _anchoPantalla = spriteBatch.GraphicsDevice.Viewport.Width;
            _altoPantalla = spriteBatch.GraphicsDevice.Viewport.Height;

            // Fondo sin deformar + oscurecido a la izquierda para que se lea el panel.
            if (_fondoTaller != null)
            {
                UI.FondoCubriendo(spriteBatch, _fondoTaller, _anchoPantalla, _altoPantalla, Color.White);
            }
            UI.Vineta(spriteBatch, _anchoPantalla, _altoPantalla, 0.55f);

            if (_jugador.AutoActual != null)
            {
                DibujarAutoEnElPiso(spriteBatch);
                DibujarPanelIzquierdo(spriteBatch);
                DibujarFichaTecnica(spriteBatch);
            }
            else
            {
                DibujarSinAuto(spriteBatch);
            }

            DibujarBarraInferior(spriteBatch);

            if (_tiempoMensaje > 0f)
            {
                DibujarMensaje(spriteBatch);
            }
        }

        /// <summary>
        /// ANTES: el auto se dibujaba en (700, 700) a escala 1, o sea con el tamaño de
        /// su imagen original (¡el Gol mide 1536 px!) y quedaba fuera de pantalla.
        /// AHORA: se apoya sobre la mancha oscura del piso del fondo, con un tamaño
        /// proporcional a la imagen de fondo, en cualquier resolución.
        /// </summary>
        private void DibujarAutoEnElPiso(SpriteBatch spriteBatch)
        {
            Auto auto = _jugador.AutoActual;

            float escalaFondo = UI.EscalaDelFondo(_fondoTaller, _anchoPantalla, _altoPantalla);
            Vector2 piso = UI.PuntoDelFondo(_fondoTaller, _anchoPantalla, _altoPantalla, PisoU, PisoV);
            float anchoAuto = _fondoTaller.Width * escalaFondo * AnchoAutoSobreFondo;

            // Que no quede tapado por los paneles de los costados.
            float izquierdaLibre = RectPanelIzquierdo().Right + 30;
            float derechaLibre = RectPanelDerecho().X - 30;
            anchoAuto = Math.Min(anchoAuto, derechaLibre - izquierdaLibre);
            piso.X = MathHelper.Clamp(piso.X, izquierdaLibre + anchoAuto / 2f, derechaLibre - anchoAuto / 2f);

            // En pantallas 16:9 el fondo (que es 4:3) se recorta arriba y abajo y el piso
            // queda muy abajo: dejamos lugar para el nombre del auto y la barra inferior.
            piso.Y = Math.Min(piso.Y, _altoPantalla - 64 - 90);

            // Reflector desde arriba
            UI.Resplandor(spriteBatch, new Vector2(piso.X, piso.Y - anchoAuto * 0.2f), anchoAuto * 0.8f, anchoAuto * 0.45f, Color.White * 0.08f);

            auto.DibujarSobrePiso(spriteBatch, piso, anchoAuto, Color.White, true);

            // Cartel con el nombre del auto debajo
            UI.TextoCentrado(spriteBatch, UI.Titulo, auto.Modelo.ToUpper(), new Vector2(piso.X, piso.Y + 46), UI.TextoClaro, 0.8f);
        }

        private void DibujarPanelIzquierdo(SpriteBatch spriteBatch)
        {
            Rectangle panel = RectPanelIzquierdo();
            UI.Panel(spriteBatch, panel, 0.9f, UI.Acento);

            UI.Texto(spriteBatch, UI.Titulo, "TALLER", new Vector2(panel.X + 24, panel.Y + 18), UI.TextoClaro);
            UI.Texto(spriteBatch, UI.Fuente, "Mejorá tu auto para ganar más picadas", new Vector2(panel.X + 26, panel.Y + 72), UI.TextoSecundario);

            DibujarPestana(spriteBatch, RectPestanaPiezas(), "PIEZAS  [P]", !_modoLlantas);
            DibujarPestana(spriteBatch, RectPestanaLlantas(), "LLANTAS  [L]", _modoLlantas);

            if (_modoLlantas == false)
            {
                for (int i = 0; i < _taller.CatalogoPiezas.Count; i++)
                {
                    DibujarFilaPieza(spriteBatch, i);
                }
            }
            else
            {
                for (int i = 0; i < _taller.CatalogoLlantas.Count; i++)
                {
                    DibujarTarjetaLlanta(spriteBatch, i);
                }
            }

            string ayuda = _modoLlantas ? "Las llantas son gratis: probalas todas" : "Clic o ENTER para comprar e instalar";
            UI.TextoCentrado(spriteBatch, UI.Fuente, ayuda, new Vector2(panel.Center.X, panel.Bottom - 30), UI.TextoSecundario);
        }

        private void DibujarPestana(SpriteBatch spriteBatch, Rectangle rect, string texto, bool activa)
        {
            bool encima = rect.Contains(_posicionMouse);
            if (activa)
            {
                UI.Rect(spriteBatch, rect, UI.Acento);
                UI.TextoCentrado(spriteBatch, UI.Negrita, texto, new Vector2(rect.Center.X, rect.Center.Y + 1), new Color(30, 24, 6), 1f, false);
            }
            else
            {
                UI.Boton(spriteBatch, rect, texto, encima);
            }
        }

        private void DibujarFilaPieza(SpriteBatch spriteBatch, int indice)
        {
            Componente pieza = _taller.CatalogoPiezas[indice];
            Rectangle fila = RectFilaPieza(indice);
            Auto auto = _jugador.AutoActual;

            bool seleccionada = indice == _indiceSeleccionado;
            bool encima = fila.Contains(_posicionMouse);
            bool instalada = auto.TienePiezaComprada(pieza);
            bool alcanza = _jugador.Dinero >= pieza.Costo;

            Color fondo = seleccionada ? new Color(60, 52, 24) : (encima ? UI.ColorPanelClaro : Color.Black * 0.25f);
            UI.Rect(spriteBatch, fila, fondo);
            if (seleccionada)
            {
                UI.Rect(spriteBatch, new Rectangle(fila.X, fila.Y, 4, fila.Height), UI.Acento);
            }

            // Ícono
            int tamanoIcono = fila.Height - 8;
            Rectangle icono = new Rectangle(fila.X + 12, fila.Y + 4, tamanoIcono, tamanoIcono);
            DibujarIconoPieza(spriteBatch, pieza, icono);

            float centroY = fila.Center.Y;
            string derecha;
            Color colorDerecha;
            if (instalada)
            {
                derecha = "INSTALADA";
                colorDerecha = UI.Verde;
            }
            else
            {
                derecha = UI.Dinero(pieza.Costo);
                colorDerecha = alcanza ? UI.Acento : UI.Rojo;
            }

            Vector2 medidaDerecha = UI.Negrita.MeasureString(derecha);
            UI.TextoDerecha(spriteBatch, UI.Negrita, derecha, new Vector2(fila.Right - 12, centroY - medidaDerecha.Y / 2f), colorDerecha);

            // Nombre: si es muy largo para el espacio que queda, se achica un poco para no pisar el precio.
            Vector2 medidaNombre = UI.Fuente.MeasureString(pieza.Nombre);
            float espacioNombre = fila.Right - 24 - medidaDerecha.X - (icono.Right + 12);
            float escalaNombre = Math.Min(1f, espacioNombre / medidaNombre.X);
            Color colorNombre = instalada ? UI.TextoSecundario : UI.TextoClaro;
            UI.Texto(spriteBatch, UI.Fuente, pieza.Nombre,
                new Vector2(icono.Right + 12, centroY - medidaNombre.Y * escalaNombre / 2f), colorNombre, escalaNombre);
        }

        private void DibujarIconoPieza(SpriteBatch spriteBatch, Componente pieza, Rectangle destino)
        {
            Texture2D icono = null;
            Color color = UI.TextoSecundario;
            string letra = "?";

            if (pieza is Motor) { icono = _iconoMotor; color = UI.Rojo; letra = "M"; }
            else if (pieza is Turbo) { icono = _iconoTurbo; color = UI.Azul; letra = "T"; }
            else if (pieza is Neumatico) { icono = _iconoNeumatico; color = new Color(120, 120, 130); letra = "N"; }
            else if (pieza is Transmision) { color = new Color(160, 110, 220); letra = "C"; }   // C de caja
            else if (pieza is Intercooler) { color = new Color(80, 200, 220); letra = "I"; }

            UI.Rect(spriteBatch, destino, Color.Black * 0.35f);

            if (icono != null)
            {
                Rectangle limites = UI.LimitesOpacos(icono);
                float escala = Math.Min(destino.Width / (float)limites.Width, destino.Height / (float)limites.Height);
                int ancho = (int)(limites.Width * escala);
                int alto = (int)(limites.Height * escala);
                Rectangle ajustado = new Rectangle(destino.Center.X - ancho / 2, destino.Center.Y - alto / 2, ancho, alto);
                spriteBatch.Draw(icono, ajustado, limites, Color.White);
            }
            else
            {
                UI.Rect(spriteBatch, new Rectangle(destino.X + 3, destino.Y + 3, destino.Width - 6, destino.Height - 6), color * 0.85f);
                UI.TextoCentrado(spriteBatch, UI.Negrita, letra, new Vector2(destino.Center.X, destino.Center.Y + 1), Color.White);
            }
        }

        private void DibujarTarjetaLlanta(SpriteBatch spriteBatch, int indice)
        {
            Rectangle tarjeta = RectLlanta(indice);
            Texture2D llanta = _taller.CatalogoLlantas[indice];

            bool seleccionada = indice == _indiceLlantaSeleccionada;
            bool encima = tarjeta.Contains(_posicionMouse);
            bool puesta = _jugador.AutoActual.LlantaDelantera == llanta;

            UI.Rect(spriteBatch, tarjeta, seleccionada ? new Color(60, 52, 24) : (encima ? UI.ColorPanelClaro : Color.Black * 0.3f));
            UI.Borde(spriteBatch, tarjeta, seleccionada ? 2 : 1, seleccionada ? UI.Acento : Color.Black * 0.5f);

            // Llanta: la seleccionada gira despacito como en una vidriera.
            int diametro = Math.Min(tarjeta.Width - 30, tarjeta.Height - 46);
            Vector2 centro = new Vector2(tarjeta.Center.X, tarjeta.Y + 10 + diametro / 2f);
            Rectangle limites = UI.LimitesOpacos(llanta);
            Vector2 escala = new Vector2(diametro / (float)limites.Width, diametro / (float)limites.Height);
            Vector2 origen = new Vector2(limites.Width / 2f, limites.Height / 2f);
            float rotacion = seleccionada ? _rotacionLlantaVitrina : 0f;

            UI.Resplandor(spriteBatch, centro + new Vector2(0, diametro * 0.45f), diametro * 0.5f, 10f, Color.Black * 0.6f);
            spriteBatch.Draw(llanta, centro, limites, Color.White, rotacion, origen, escala, SpriteEffects.None, 0f);

            UI.TextoCentrado(spriteBatch, UI.Negrita, _taller.NombresLlantas[indice],
                new Vector2(tarjeta.Center.X, tarjeta.Bottom - 20), seleccionada ? UI.Acento : UI.TextoClaro, 0.85f);

            if (puesta)
            {
                Rectangle etiqueta = new Rectangle(tarjeta.Right - 70, tarjeta.Y + 6, 64, 22);
                UI.Rect(spriteBatch, etiqueta, UI.Verde);
                UI.TextoCentrado(spriteBatch, UI.Fuente, "PUESTA", new Vector2(etiqueta.Center.X, etiqueta.Center.Y), new Color(10, 40, 20), 0.75f, false);
            }
        }

        private void DibujarFichaTecnica(SpriteBatch spriteBatch)
        {
            Auto auto = _jugador.AutoActual;
            Rectangle panel = RectPanelDerecho();
            UI.Panel(spriteBatch, panel, 0.9f);

            int x = panel.X + 20;
            int y = panel.Y + 18;

            UI.Texto(spriteBatch, UI.Negrita, "FICHA TÉCNICA", new Vector2(x, y), UI.Acento);
            y += 34;
            UI.Chip(spriteBatch, new Vector2(x, y), "DINERO", UI.Dinero(_jugador.Dinero), UI.Verde);
            y += 50;

            UI.Texto(spriteBatch, UI.Fuente, "Potencia total", new Vector2(x, y), UI.TextoSecundario);
            UI.TextoDerecha(spriteBatch, UI.Negrita, $"x{auto.MultiplicadorMotorTotal:0.00}", new Vector2(panel.Right - 20, y - 2), UI.Acento);
            y += 30;

            UI.Texto(spriteBatch, UI.Fuente, "Velocidad máx.", new Vector2(x, y), UI.TextoSecundario);
            y += 22;
            UI.Barra(spriteBatch, new Rectangle(x, y, panel.Width - 40, 10), auto.VelocidadMaximaReal / 30f, UI.Azul);
            y += 22;

            UI.Texto(spriteBatch, UI.Fuente, "Aceleración", new Vector2(x, y), UI.TextoSecundario);
            y += 22;
            UI.Barra(spriteBatch, new Rectangle(x, y, panel.Width - 40, 10), auto.AceleracionReal / 0.6f, UI.Verde);
            y += 26;

            UI.Rect(spriteBatch, new Rectangle(x, y, panel.Width - 40, 1), UI.BordePanel);
            y += 12;

            UI.Texto(spriteBatch, UI.Negrita, "INSTALADO", new Vector2(x, y), UI.TextoClaro, 0.85f);
            y += 28;

            foreach (Componente componente in auto.ObtenerComponentesInstalados())
            {
                bool esMejora = componente.MultiplicadorRendimiento > 1f;
                UI.CirculoLleno(spriteBatch, new Vector2(x + 5, y + 10), 4, esMejora ? UI.Verde : UI.TextoSecundario * 0.6f);
                UI.Texto(spriteBatch, UI.Fuente, componente.Nombre, new Vector2(x + 18, y), esMejora ? UI.TextoClaro : UI.TextoSecundario, 0.9f, false);
                y += 22;
            }
        }

        private void DibujarSinAuto(SpriteBatch spriteBatch)
        {
            Rectangle caja = new Rectangle(_anchoPantalla / 2 - 320, _altoPantalla / 2 - 110, 640, 200);
            UI.Panel(spriteBatch, caja, 0.95f, UI.Rojo);
            UI.TextoCentrado(spriteBatch, UI.Titulo, "EL TALLER ESTÁ VACÍO", new Vector2(caja.Center.X, caja.Y + 60), UI.TextoClaro, 0.8f);
            UI.TextoCentrado(spriteBatch, UI.Fuente, "No hay ningún auto en el taller.", new Vector2(caja.Center.X, caja.Y + 112), UI.TextoSecundario);
            UI.TextoCentrado(spriteBatch, UI.Fuente, "Comprá y equipá uno en el CONCESIONARIO.", new Vector2(caja.Center.X, caja.Y + 140), UI.Acento);
        }

        private void DibujarBarraInferior(SpriteBatch spriteBatch)
        {
            Rectangle barra = new Rectangle(0, _altoPantalla - 64, _anchoPantalla, 64);
            UI.Rect(spriteBatch, barra, Color.Black * 0.7f);
            UI.Rect(spriteBatch, new Rectangle(0, barra.Y, _anchoPantalla, 1), UI.BordePanel);

            _rectVolver = new Rectangle(24, barra.Y + 10, 150, 44);
            UI.Boton(spriteBatch, _rectVolver, "< VOLVER", _rectVolver.Contains(_posicionMouse));

            float x = _rectVolver.Right + 30;
            float y = barra.Y + 15;
            x += UI.Tecla(spriteBatch, new Vector2(x, y), "P", "Piezas");
            x += UI.Tecla(spriteBatch, new Vector2(x, y), "L", "Llantas");
            x += UI.Tecla(spriteBatch, new Vector2(x, y), "^ v", "Elegir");
            x += UI.Tecla(spriteBatch, new Vector2(x, y), "ENTER", "Instalar");
            UI.Tecla(spriteBatch, new Vector2(x, y), "ESC", "Volver al menú");
        }

        private void DibujarMensaje(SpriteBatch spriteBatch)
        {
            float opacidad = MathHelper.Clamp(_tiempoMensaje / 0.4f, 0f, 1f);
            Color color = _mensajeEsError ? UI.Rojo : UI.Verde;

            Vector2 medida = UI.Negrita.MeasureString(_mensaje);
            float centroX = (RectPanelIzquierdo().Right + RectPanelDerecho().X) / 2f;
            Rectangle caja = new Rectangle((int)(centroX - medida.X / 2f - 24), 40, (int)medida.X + 48, 48);

            UI.Rect(spriteBatch, caja, Color.Black * 0.8f * opacidad);
            UI.Rect(spriteBatch, new Rectangle(caja.X, caja.Y, 4, caja.Height), color * opacidad);
            UI.Borde(spriteBatch, caja, 1, color * 0.6f * opacidad);
            UI.TextoCentrado(spriteBatch, UI.Negrita, _mensaje, new Vector2(caja.Center.X, caja.Center.Y + 1), UI.TextoClaro * opacidad);
        }
    }
}
