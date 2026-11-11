using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace La35Tunning.Servidor
{
    /// <summary>
    /// SERVIDOR DE LA35TUNNING
    ///
    /// Qué hace:
    ///   1. Espera a que se conecten 2 jugadores.
    ///   2. Les manda a los dos el MISMO tiempo de semáforo (así se ponen en verde juntos).
    ///   3. Durante la carrera, lo que manda uno se lo reenvía al otro (la posición del rival).
    ///   4. Decide quién ganó y se lo avisa a los dos. Si alguien se desconecta, gana el otro.
    ///
    /// Los mensajes son texto, una línea por mensaje, con los datos separados por "|":
    ///
    ///   Juego -> Servidor                Servidor -> Juego
    ///   HOLA|nombre|modelo               ESPERANDO
    ///   POS|x                            INICIO|nombreRival|modeloRival|esperaVerde
    ///   SALIDA_ANTICIPADA                RIVAL|x
    ///   META|tiempo                      RESULTADO|GANASTE o PERDISTE|motivo
    ///
    /// Cómo correrlo (desde la carpeta del repo):   dotnet run --project La35Tunning.Servidor
    /// </summary>
    public static class Program
    {
        public const int Puerto = 35035;

        public static void Main()
        {
            TcpListener servidor = new TcpListener(IPAddress.Any, Puerto);
            servidor.Start();

            Console.WriteLine("=== Servidor La35Tunning ===");
            Console.WriteLine($"Escuchando en el puerto {Puerto}. En el juego escribí una de estas IP:");
            MostrarMisIps();

            int numeroCarrera = 1;
            while (true)
            {
                Console.WriteLine($"\n[Carrera {numeroCarrera}] Esperando 2 jugadores...");

                JugadorConectado jugadorA = EsperarJugador(servidor);
                jugadorA.Enviar("ESPERANDO");
                JugadorConectado jugadorB = EsperarJugador(servidor);

                // Se corre una carrera por vez: simple y suficiente para una red local.
                Sala sala = new Sala(jugadorA, jugadorB);
                sala.Correr();

                numeroCarrera++;
            }
        }

        /// <summary>Espera una conexión y lee su saludo "HOLA|nombre|modelo".</summary>
        private static JugadorConectado EsperarJugador(TcpListener servidor)
        {
            while (true)
            {
                TcpClient conexion = servidor.AcceptTcpClient();   // se queda acá hasta que alguien se conecta
                JugadorConectado jugador = new JugadorConectado(conexion);

                string saludo = jugador.LeerLinea();
                string[] partes = saludo == null ? new string[0] : saludo.Split('|');

                if (partes.Length == 3 && partes[0] == "HOLA")
                {
                    jugador.Nombre = partes[1];
                    jugador.Modelo = partes[2];
                    Console.WriteLine($"  + Se conectó {jugador.Nombre} con un {jugador.Modelo}");
                    return jugador;
                }

                Console.WriteLine("  ! Conexión con saludo inválido, se descarta.");
                jugador.Cerrar();
            }
        }

        private static void MostrarMisIps()
        {
            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)   // solo IPv4 (ej: 192.168.0.15)
                    Console.WriteLine("   " + ip);
            }
            Console.WriteLine("   127.0.0.1  (si el juego corre en esta misma PC)");
        }

        /// <summary>Los números viajan siempre con punto (3.25), aunque Windows esté en español.</summary>
        public static string Numero(float valor)
        {
            return valor.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>Un jugador conectado: su conexión y lo que nos dijo al saludar.</summary>
    public class JugadorConectado
    {
        private readonly TcpClient _conexion;
        private readonly StreamReader _lector;
        private readonly StreamWriter _escritor;
        private readonly object _candado = new object();

        public string Nombre { get; set; } = "?";
        public string Modelo { get; set; } = "?";

        public JugadorConectado(TcpClient conexion)
        {
            _conexion = conexion;
            NetworkStream flujo = conexion.GetStream();
            _lector = new StreamReader(flujo, Encoding.UTF8);
            _escritor = new StreamWriter(flujo, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };
        }

        /// <summary>Devuelve la próxima línea, o null si el jugador se desconectó.</summary>
        public string LeerLinea()
        {
            try { return _lector.ReadLine(); }
            catch (Exception) { return null; }
        }

        public void Enviar(string mensaje)
        {
            // "lock": dos hilos podrían querer mandarle algo al mismo tiempo; así no se mezclan.
            lock (_candado)
            {
                try { _escritor.WriteLine(mensaje); }
                catch (Exception) { /* si ya se desconectó, no pasa nada */ }
            }
        }

        public void Cerrar()
        {
            try { _conexion.Close(); } catch (Exception) { }
        }
    }

    /// <summary>Una carrera entre dos jugadores.</summary>
    public class Sala
    {
        // Nadie hace 400 metros en menos de 4 segundos: si alguien dice eso, está haciendo trampa.
        private const float TiempoMinimoValido = 4f;

        private readonly JugadorConectado _jugadorA;
        private readonly JugadorConectado _jugadorB;
        private readonly object _candado = new object();
        private readonly ManualResetEvent _fin = new ManualResetEvent(false);
        private bool _terminada;

        public Sala(JugadorConectado a, JugadorConectado b)
        {
            _jugadorA = a;
            _jugadorB = b;
        }

        public void Correr()
        {
            // El servidor elige cuánto espera el semáforo antes del verde, y es el mismo para los dos.
            float esperaVerde = 1.6f + (float)new Random().NextDouble() * 1.9f;

            _jugadorA.Enviar($"INICIO|{_jugadorB.Nombre}|{_jugadorB.Modelo}|{Program.Numero(esperaVerde)}");
            _jugadorB.Enviar($"INICIO|{_jugadorA.Nombre}|{_jugadorA.Modelo}|{Program.Numero(esperaVerde)}");
            Console.WriteLine($"  > Largan {_jugadorA.Nombre} vs {_jugadorB.Nombre}");

            // Un hilo por jugador para escuchar lo que manda cada uno al mismo tiempo.
            new Thread(() => Escuchar(_jugadorA, _jugadorB)) { IsBackground = true }.Start();
            new Thread(() => Escuchar(_jugadorB, _jugadorA)) { IsBackground = true }.Start();

            _fin.WaitOne();   // esperamos hasta que la carrera tenga resultado

            Thread.Sleep(500);  // un ratito para que lleguen los últimos mensajes
            _jugadorA.Cerrar();
            _jugadorB.Cerrar();
        }

        private void Escuchar(JugadorConectado yo, JugadorConectado rival)
        {
            string linea;
            while ((linea = yo.LeerLinea()) != null)
            {
                string[] partes = linea.Split('|');

                // try/catch: si llega un mensaje mal armado, lo ignoramos en vez de que se caiga el servidor.
                try
                {
                    switch (partes[0])
                    {
                        case "POS":
                            rival.Enviar("RIVAL|" + partes[1]);   // le paso al rival dónde estoy
                            break;

                        case "SALIDA_ANTICIPADA":
                            Terminar(rival, yo, $"Salida anticipada de {yo.Nombre}");
                            break;

                        case "META":
                            float tiempo = float.Parse(partes[1], CultureInfo.InvariantCulture);
                            if (tiempo < TiempoMinimoValido)
                                Terminar(rival, yo, $"Tiempo inválido de {yo.Nombre}");
                            else
                                Terminar(yo, rival, $"{yo.Nombre} llegó primero en {tiempo.ToString("0.000")} s");
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"  ! Mensaje inválido de {yo.Nombre}: {linea}");
                }
            }

            // Si salimos del while es porque este jugador se desconectó.
            Terminar(rival, yo, $"{yo.Nombre} abandonó la carrera");
        }

        /// <summary>Solo vale el PRIMER resultado: si ya terminó, no hace nada.</summary>
        private void Terminar(JugadorConectado ganador, JugadorConectado perdedor, string motivo)
        {
            lock (_candado)
            {
                if (_terminada)
                    return;
                _terminada = true;
            }

            ganador.Enviar("RESULTADO|GANASTE|" + motivo);
            perdedor.Enviar("RESULTADO|PERDISTE|" + motivo);
            Console.WriteLine($"  * Ganó {ganador.Nombre}. {motivo}");
            _fin.Set();
        }
    }
}
