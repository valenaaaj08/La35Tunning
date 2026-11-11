using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace La35Tunning.Sistemas
{
    /// <summary>
    /// Conexión del juego con el servidor (ver La35Tunning.Servidor/Program.cs).
    ///
    /// Idea clave: leer de la red "bloquea" (el programa se queda esperando hasta que
    /// llegue algo). Si leyéramos en Update() el juego se congelaría. Por eso un HILO
    /// aparte lee los mensajes y los deja en una cola, y el juego los va sacando de a uno
    /// en cada Update() con TryRecibir().
    /// </summary>
    public class ClienteRed
    {
        public const int Puerto = 35035;

        private TcpClient _conexion;
        private StreamWriter _escritor;

        // Cola "segura para hilos": el hilo de red agrega y el juego saca, sin pisarse.
        private readonly ConcurrentQueue<string> _recibidos = new ConcurrentQueue<string>();

        public bool Conectado { get; private set; }

        /// <summary>Intenta conectarse (espera hasta 3 segundos). Si falla, "error" dice por qué.</summary>
        public bool Conectar(string ip, out string error)
        {
            try
            {
                _conexion = new TcpClient();
                if (!_conexion.ConnectAsync(ip, Puerto).Wait(3000))
                {
                    _conexion.Close();
                    error = "El servidor no responde. ¿Está abierto? ¿La IP es correcta?";
                    return false;
                }

                NetworkStream flujo = _conexion.GetStream();
                _escritor = new StreamWriter(flujo, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };
                StreamReader lector = new StreamReader(flujo, Encoding.UTF8);

                Conectado = true;
                new Thread(() => Escuchar(lector)) { IsBackground = true }.Start();

                error = "";
                return true;
            }
            catch (Exception)
            {
                error = "No se pudo conectar. Revisá la IP y que el servidor esté abierto.";
                return false;
            }
        }

        /// <summary>Corre en el hilo aparte: lee líneas y las encola hasta que se corta la conexión.</summary>
        private void Escuchar(StreamReader lector)
        {
            try
            {
                string linea;
                while ((linea = lector.ReadLine()) != null)
                {
                    _recibidos.Enqueue(linea);
                }
            }
            catch (Exception) { /* se cortó la conexión */ }

            Conectado = false;
            _recibidos.Enqueue("DESCONECTADO");   // mensaje inventado por nosotros para avisarle al juego
        }

        public void Enviar(string mensaje)
        {
            if (!Conectado)
                return;

            try { _escritor.WriteLine(mensaje); }
            catch (Exception) { Conectado = false; }
        }

        /// <summary>Saca el próximo mensaje de la cola. Devuelve false si no hay ninguno.</summary>
        public bool TryRecibir(out string mensaje)
        {
            return _recibidos.TryDequeue(out mensaje);
        }

        public void Cerrar()
        {
            Conectado = false;
            try { _conexion?.Close(); } catch (Exception) { }
        }

        // Los números viajan siempre con punto ("3.25"). Windows en español usa coma,
        // por eso NO usamos float.ToString() ni float.Parse() normales.
        public static string Numero(float valor)
        {
            return valor.ToString("0.###", CultureInfo.InvariantCulture);
        }

        public static float LeerNumero(string texto)
        {
            return float.Parse(texto, CultureInfo.InvariantCulture);
        }
    }
}
