using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
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
        public const int Puerto = 35035;                  // TCP: la carrera
        public const int PuertoDescubrimiento = 35036;    // UDP: buscar el servidor

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

        // ==================================================================
        // NUEVO: BUSCAR EL SERVIDOR POR BROADCAST (UDP)
        // ==================================================================

        /// <summary>
        /// Manda "LA35_BUSCAR" a TODA la red local (broadcast UDP) y espera hasta
        /// "esperaMs" milisegundos una respuesta "LA35_SERVIDOR". Devuelve la IP de
        /// quien contestó, o null si nadie contestó.
        /// Se usa UDP porque TCP necesita saber a quién conectarse; un broadcast UDP
        /// le llega a todas las PCs de la red sin saber ninguna IP.
        /// </summary>
        public static string BuscarServidor(int esperaMs)
        {
            try
            {
                using (UdpClient udp = new UdpClient())
                {
                    udp.EnableBroadcast = true;                 // sin esto Windows no deja mandar broadcast
                    IgnorarRebotes(udp);

                    byte[] pregunta = Encoding.UTF8.GetBytes("LA35_BUSCAR");
                    foreach (IPAddress destino in DireccionesDeBroadcast())
                    {
                        try { udp.Send(pregunta, pregunta.Length, new IPEndPoint(destino, PuertoDescubrimiento)); }
                        catch (Exception) { /* alguna placa de red puede no aceptar: seguimos con las otras */ }
                    }

                    // Esperamos respuestas hasta que se cumpla el tiempo.
                    DateTime limite = DateTime.Now.AddMilliseconds(esperaMs);
                    while (DateTime.Now < limite)
                    {
                        udp.Client.ReceiveTimeout = Math.Max(1, (int)(limite - DateTime.Now).TotalMilliseconds);
                        try
                        {
                            IPEndPoint quienContesto = new IPEndPoint(IPAddress.Any, 0);
                            byte[] respuesta = udp.Receive(ref quienContesto);   // se queda esperando la respuesta

                            if (Encoding.UTF8.GetString(respuesta) == "LA35_SERVIDOR")
                                return quienContesto.Address.ToString();     // la IP del servidor es la de quien contestó
                        }
                        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
                        {
                            // Rebotó un mensaje (en esa dirección nadie escuchaba): seguimos esperando.
                        }
                    }
                }
            }
            catch (Exception) { /* se cumplió el tiempo y nadie contestó */ }

            return null;
        }

        /// <summary>
        /// En Windows, si un datagrama UDP "rebota" (en ese destino nadie escuchaba),
        /// el siguiente Receive() tira un error en lugar de seguir esperando.
        /// Esta línea le dice a Windows que no avise esos rebotes (opción SIO_UDP_CONNRESET).
        /// </summary>
        public static void IgnorarRebotes(UdpClient udp)
        {
            try
            {
                const int SIO_UDP_CONNRESET = -1744830452;
                udp.Client.IOControl(SIO_UDP_CONNRESET, new byte[] { 0 }, null);
            }
            catch (Exception) { /* en Linux o Mac no existe: no hace falta */ }
        }

        /// <summary>
        /// A dónde mandar el broadcast: 255.255.255.255 (toda la red), el broadcast de
        /// cada placa de red de esta PC (ej: 192.168.0.255) y 127.0.0.1 (por si el
        /// servidor está en esta misma PC).
        /// </summary>
        private static List<IPAddress> DireccionesDeBroadcast()
        {
            List<IPAddress> destinos = new List<IPAddress> { IPAddress.Broadcast, IPAddress.Loopback };

            foreach (NetworkInterface placa in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (placa.OperationalStatus != OperationalStatus.Up || placa.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;   // la de loopback (127.x) ya la agregamos arriba

                foreach (UnicastIPAddressInformation direccion in placa.GetIPProperties().UnicastAddresses)
                {
                    if (direccion.Address.AddressFamily != AddressFamily.InterNetwork || direccion.IPv4Mask == null)
                        continue;

                    // Broadcast = IP con todos los bits de "equipo" en 1. Ej: 192.168.0.17 / 255.255.255.0 -> 192.168.0.255
                    byte[] ip = direccion.Address.GetAddressBytes();
                    byte[] mascara = direccion.IPv4Mask.GetAddressBytes();
                    byte[] broadcast = new byte[4];
                    for (int i = 0; i < 4; i++)
                        broadcast[i] = (byte)(ip[i] | ~mascara[i]);

                    destinos.Add(new IPAddress(broadcast));
                }
            }

            return destinos;
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
