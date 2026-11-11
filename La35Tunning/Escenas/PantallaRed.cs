using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using La35Tunning.Modelos;
using La35Tunning.Sistemas;

namespace La35Tunning.Escenas
{
    /// <summary>
    /// NUEVA: pantalla "JUGAR EN RED". Escribís la IP del servidor, te conectás
    /// y esperás a que se conecte otro jugador. Cuando el servidor manda INICIO,
    /// CarreraLista pasa a true y Game1 arranca la carrera.
    /// </summary>
    public class PantallaRed
    {
        private readonly Jugador _jugador;
        private ClienteRed _cliente;               // null mientras no estemos conectados
        private string _ip = "127.0.0.1";
        private string _nombre = "";               // NUEVO: nombre que elige el jugador
        private bool _editandoNombre = true;       // qué caja recibe lo que se escribe
        private Rectangle _rectCajaNombre;
        private Rectangle _rectCajaIp;
        private Rectangle _rectBuscar;
        // NUEVO: la búsqueda espera ~1,5 s la respuesta del servidor. La hacemos en una
        // Task (otro hilo) para que el juego no se congele mientras tanto.
        private Task<string> _busqueda;
        private string _estado = "";
        private bool _estadoEsError;
        private float _tiempo;

        private KeyboardState _tecladoAnterior;
        private MouseState _mouseAnterior;
        private bool _entradaInicializada;
        private Point _posicionMouse;
        private Rectangle _rectConectar;
        private Rectangle _rectVolver;

        public bool DebeVolver { get; private set; }
        public bool CarreraLista { get; private set; }
        public string NombreRival { get; private set; }
        public string ModeloRival { get; private set; }
        public float EsperaVerde { get; private set; }

        public PantallaRed(Jugador jugador)
        {
            _jugador = jugador;
        }

        public void Reiniciar()
        {
            _cliente?.Cerrar();
            _cliente = null;
            DebeVolver = false;
            CarreraLista = false;
            _nombre = _jugador.Nombre;
            _editandoNombre = true;
            _estado = "Escribí tu nombre y la IP de la PC donde está abierto el servidor.";
            _estadoEsError = false;
            _entradaInicializada = false;
            EmpezarBusqueda();   // NUEVO: al entrar, busca el servidor solo
        }

        /// <summary>Game1 se queda con la conexión para usarla en la carrera.</summary>
        public ClienteRed TomarCliente()
        {
            ClienteRed cliente = _cliente;
            _cliente = null;
            CarreraLista = false;
            return cliente;
        }

        public void Update(GameTime gameTime)
        {
            _tiempo += (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState teclado = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            _posicionMouse = mouse.Position;

            if (!_entradaInicializada)
            {
                _tecladoAnterior = teclado;
                _mouseAnterior = mouse;
                _entradaInicializada = true;
                return;
            }

            bool hizoClic = mouse.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released;

            if (_busqueda != null && _busqueda.IsCompleted)
                TerminarBusqueda();

            if (_cliente == null)
            {
                if (hizoClic && _rectBuscar.Contains(_posicionMouse))
                    EmpezarBusqueda();

                // TAB o clic en una caja: elegir si escribimos el nombre o la IP
                if (teclado.IsKeyDown(Keys.Tab) && _tecladoAnterior.IsKeyUp(Keys.Tab))
                    _editandoNombre = !_editandoNombre;
                if (hizoClic && _rectCajaNombre.Contains(_posicionMouse)) _editandoNombre = true;
                if (hizoClic && _rectCajaIp.Contains(_posicionMouse)) _editandoNombre = false;

                if (_editandoNombre)
                    EscribirNombre(teclado);
                else
                    EscribirIp(teclado);

                bool apretoEnter = teclado.IsKeyDown(Keys.Enter) && _tecladoAnterior.IsKeyUp(Keys.Enter);
                if (apretoEnter || (hizoClic && _rectConectar.Contains(_posicionMouse)))
                    Conectar();
            }
            else
            {
                LeerMensajesDelServidor();
            }

            bool apretoEsc = teclado.IsKeyDown(Keys.Escape) && _tecladoAnterior.IsKeyUp(Keys.Escape);
            if (apretoEsc || (hizoClic && _rectVolver.Contains(_posicionMouse)))
            {
                _cliente?.Cerrar();
                _cliente = null;
                DebeVolver = true;
            }

            _tecladoAnterior = teclado;
            _mouseAnterior = mouse;
        }

        /// <summary>Solo números y puntos (una IP es tipo 192.168.0.15). Borrar con Backspace.</summary>
        private void EscribirIp(KeyboardState teclado)
        {
            foreach (Keys tecla in teclado.GetPressedKeys())
            {
                if (_tecladoAnterior.IsKeyDown(tecla))
                    continue;   // ya estaba apretada el cuadro anterior: no la repetimos

                if (tecla == Keys.Back && _ip.Length > 0)
                    _ip = _ip.Substring(0, _ip.Length - 1);
                else if (_ip.Length >= 15)
                    continue;
                else if (tecla >= Keys.D0 && tecla <= Keys.D9)
                    _ip += (char)('0' + (tecla - Keys.D0));
                else if (tecla >= Keys.NumPad0 && tecla <= Keys.NumPad9)
                    _ip += (char)('0' + (tecla - Keys.NumPad0));
                else if (tecla == Keys.OemPeriod || tecla == Keys.Decimal)
                    _ip += '.';
            }
        }

        /// <summary>NUEVO: letras, números y espacios, hasta 12 caracteres. Con SHIFT van en mayúscula.</summary>
        private void EscribirNombre(KeyboardState teclado)
        {
            bool mayuscula = teclado.IsKeyDown(Keys.LeftShift) || teclado.IsKeyDown(Keys.RightShift);

            foreach (Keys tecla in teclado.GetPressedKeys())
            {
                if (_tecladoAnterior.IsKeyDown(tecla))
                    continue;

                if (tecla == Keys.Back && _nombre.Length > 0)
                    _nombre = _nombre.Substring(0, _nombre.Length - 1);
                else if (_nombre.Length >= 12)
                    continue;
                else if (tecla >= Keys.A && tecla <= Keys.Z)
                {
                    char letra = (char)('a' + (tecla - Keys.A));
                    _nombre += (mayuscula || _nombre.Length == 0) ? char.ToUpper(letra) : letra;
                }
                else if (tecla >= Keys.D0 && tecla <= Keys.D9)
                    _nombre += (char)('0' + (tecla - Keys.D0));
                else if (tecla >= Keys.NumPad0 && tecla <= Keys.NumPad9)
                    _nombre += (char)('0' + (tecla - Keys.NumPad0));
                else if (tecla == Keys.Space && _nombre.Length > 0)
                    _nombre += ' ';
            }
        }

        /// <summary>NUEVO: arranca la búsqueda del servidor por broadcast (si no hay una en curso).</summary>
        private void EmpezarBusqueda()
        {
            if (_busqueda != null)
                return;

            _estado = "Buscando el servidor en la red";
            _estadoEsError = false;
            _busqueda = Task.Run(() => ClienteRed.BuscarServidor(1500));
        }

        /// <summary>NUEVO: cuando la búsqueda terminó, pone la IP encontrada en la caja.</summary>
        private void TerminarBusqueda()
        {
            string ipEncontrada = _busqueda.Result;
            _busqueda = null;

            if (_cliente != null)
                return;   // mientras buscaba ya se conectó a mano

            if (ipEncontrada != null)
            {
                _ip = ipEncontrada;
                _estado = $"Servidor encontrado en {ipEncontrada}. Tocá CONECTAR.";
                _estadoEsError = false;
            }
            else
            {
                _estado = "No se encontró ningún servidor. Tocá BUSCAR o escribí la IP a mano.";
                _estadoEsError = true;
            }
        }

        private void Conectar()
        {
            if (_nombre.Trim().Length == 0)
            {
                _estado = "Escribí un nombre antes de conectarte.";
                _estadoEsError = true;
                _editandoNombre = true;
                return;
            }
            _jugador.CambiarNombre(_nombre);

            _cliente = new ClienteRed();

            if (!_cliente.Conectar(_ip, out string error))
            {
                _cliente = null;
                _estado = error;
                _estadoEsError = true;
                return;
            }

            _cliente.Enviar($"HOLA|{_jugador.Nombre}|{_jugador.AutoActual.Modelo}");
            _estado = "Conectado. Esperando a otro jugador";
            _estadoEsError = false;
        }

        private void LeerMensajesDelServidor()
        {
            while (_cliente != null && _cliente.TryRecibir(out string mensaje))
            {
                string[] partes = mensaje.Split('|');

                if (partes[0] == "ESPERANDO")
                {
                    _estado = "Conectado. Esperando a otro jugador";
                }
                else if (partes[0] == "INICIO" && partes.Length == 4)
                {
                    NombreRival = partes[1];
                    ModeloRival = partes[2];
                    EsperaVerde = ClienteRed.LeerNumero(partes[3]);
                    CarreraLista = true;
                    return;   // el resto de los mensajes los lee la carrera
                }
                else if (partes[0] == "DESCONECTADO")
                {
                    _cliente = null;
                    _estado = "Se cortó la conexión con el servidor.";
                    _estadoEsError = true;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int ancho = spriteBatch.GraphicsDevice.Viewport.Width;
            int alto = spriteBatch.GraphicsDevice.Viewport.Height;

            UI.Rect(spriteBatch, new Rectangle(0, 0, ancho, alto), UI.FondoOscuro);
            UI.Resplandor(spriteBatch, new Vector2(ancho / 2f, 0), ancho * 0.5f, alto * 0.6f, UI.Azul * 0.15f);

            Rectangle panel = new Rectangle(ancho / 2 - 320, alto / 2 - 250, 640, 500);
            UI.Panel(spriteBatch, panel, 0.97f, UI.Azul);

            UI.TextoCentrado(spriteBatch, UI.Titulo, "JUGAR EN RED", new Vector2(panel.Center.X, panel.Y + 60), UI.TextoClaro);
            UI.TextoCentrado(spriteBatch, UI.Fuente, "Picada 1 vs 1 con otro jugador de la misma red (LAN)", new Vector2(panel.Center.X, panel.Y + 106), UI.TextoSecundario);

            // Cajas de texto: nombre e IP (la que está activa tiene borde amarillo)
            bool escribiendo = _cliente == null;
            _rectCajaNombre = new Rectangle(panel.X + 60, panel.Y + 168, panel.Width - 120, 50);
            _rectCajaIp = new Rectangle(panel.X + 60, panel.Y + 262, panel.Width - 120, 50);
            DibujarCaja(spriteBatch, "TU NOMBRE", _nombre, _rectCajaNombre, escribiendo && _editandoNombre);
            DibujarCaja(spriteBatch, "IP DEL SERVIDOR", _ip, _rectCajaIp, escribiendo && !_editandoNombre);

            // Estado (con puntitos animados mientras espera)
            string estado = _estado;
            bool esperando = (_cliente != null || _busqueda != null) && !_estadoEsError;
            if (esperando)
                estado += new string('.', 1 + (int)(_tiempo * 2) % 3);
            UI.TextoCentrado(spriteBatch, UI.Fuente, estado, new Vector2(panel.Center.X, panel.Y + 342),
                _estadoEsError ? UI.Rojo : (_cliente != null ? UI.Verde : UI.TextoSecundario));

            _rectVolver = new Rectangle(panel.X + 60, panel.Y + 376, 160, 50);
            _rectBuscar = new Rectangle(panel.Center.X - 80, panel.Y + 376, 160, 50);
            _rectConectar = new Rectangle(panel.Right - 220, panel.Y + 376, 160, 50);
            UI.Boton(spriteBatch, _rectVolver, "< VOLVER", _rectVolver.Contains(_posicionMouse));
            bool puedeBuscar = escribiendo && _busqueda == null;
            UI.Boton(spriteBatch, _rectBuscar, "BUSCAR", puedeBuscar && _rectBuscar.Contains(_posicionMouse), puedeBuscar, new Color(36, 92, 160));
            UI.Boton(spriteBatch, _rectConectar, "CONECTAR", escribiendo && _rectConectar.Contains(_posicionMouse), escribiendo, new Color(28, 120, 70));

            UI.TextoCentrado(spriteBatch, UI.Fuente, "TAB cambia de caja  ·  El servidor se busca solo (broadcast UDP)",
                new Vector2(panel.Center.X, panel.Bottom - 34), UI.TextoSecundario, 0.8f);
        }

        private void DibujarCaja(SpriteBatch spriteBatch, string etiqueta, string texto, Rectangle caja, bool activa)
        {
            UI.Texto(spriteBatch, UI.Negrita, etiqueta, new Vector2(caja.X, caja.Y - 30), UI.TextoClaro);
            UI.Rect(spriteBatch, caja, Color.Black * 0.5f);
            UI.Borde(spriteBatch, caja, 2, activa ? UI.Acento : UI.BordePanel);
            bool cursorVisible = activa && (int)(_tiempo * 2) % 2 == 0;   // parpadea 2 veces por segundo
            UI.Texto(spriteBatch, UI.Negrita, texto + (cursorVisible ? "_" : ""), new Vector2(caja.X + 18, caja.Y + 12), UI.TextoClaro);
        }
    }
}
