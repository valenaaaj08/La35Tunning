using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace La35Tunning.Sistemas
{
    /// <summary>
    /// Caja de herramientas de dibujo compartida por TODAS las pantallas.
    ///
    /// La idea es no repetir en cada pantalla el mismo código para dibujar un
    /// rectángulo, un borde, un botón o un texto centrado. Si mañana queremos
    /// cambiar cómo se ven los botones, se cambia acá y se actualiza todo el juego.
    ///
    /// Es una clase "static": no se crea con new, se usa directo (UI.Boton(...)).
    /// Se inicializa una sola vez desde Game1.LoadContent().
    /// </summary>
    public static class UI
    {
        // ------------------------------------------------------------------
        // PALETA DE COLORES (un solo lugar para todos los colores del juego)
        // ------------------------------------------------------------------
        public static readonly Color FondoOscuro = new Color(13, 15, 21);
        public static readonly Color ColorPanel = new Color(22, 25, 33);
        public static readonly Color ColorPanelClaro = new Color(38, 43, 56);
        public static readonly Color BordePanel = new Color(64, 70, 88);
        public static readonly Color Acento = new Color(255, 194, 26);       // amarillo "picada"
        public static readonly Color Rojo = new Color(232, 65, 72);
        public static readonly Color Verde = new Color(52, 211, 120);
        public static readonly Color Azul = new Color(64, 156, 230);
        public static readonly Color TextoClaro = new Color(238, 241, 246);
        public static readonly Color TextoSecundario = new Color(150, 158, 176);

        // ------------------------------------------------------------------
        // TEXTURAS GENERADAS POR CÓDIGO (no hace falta ningún .png)
        // ------------------------------------------------------------------
        public static Texture2D Pixel { get; private set; }            // 1x1 blanco: sirve para cualquier rectángulo
        public static Texture2D Circulo { get; private set; }          // círculo lleno con borde suavizado
        public static Texture2D Brillo { get; private set; }           // círculo difuso (glow / sombra)
        public static Texture2D DegradeVertical { get; private set; }  // opaco arriba -> transparente abajo

        // ------------------------------------------------------------------
        // FUENTES
        // ------------------------------------------------------------------
        public static SpriteFont Fuente { get; private set; }   // FuentePrincipal (Arial 16)
        public static SpriteFont Negrita { get; private set; }  // FuenteNegrita (Arial 20 Bold)
        public static SpriteFont Titulo { get; private set; }   // FuenteTitulo (Arial 40 Bold)

        private static readonly Dictionary<Texture2D, Rectangle> _cacheLimites = new Dictionary<Texture2D, Rectangle>();

        private static readonly NumberFormatInfo _formatoPesos = new NumberFormatInfo
        {
            NumberGroupSeparator = ".",
            NumberDecimalSeparator = ","
        };

        /// <summary>
        /// Se llama una vez en Game1.LoadContent(). Si alguna fuente nueva no
        /// cargó (por ejemplo si falta el archivo), se usa la principal en su lugar.
        /// </summary>
        public static void Inicializar(GraphicsDevice graphicsDevice, SpriteFont fuente, SpriteFont negrita, SpriteFont titulo)
        {
            Pixel = new Texture2D(graphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });

            Circulo = CrearCirculo(graphicsDevice, 128, false);
            Brillo = CrearCirculo(graphicsDevice, 128, true);
            DegradeVertical = CrearDegradeVertical(graphicsDevice, 256);

            Fuente = fuente;
            Negrita = negrita ?? fuente;
            Titulo = titulo ?? Negrita;
        }

        // ==================================================================
        // GENERACIÓN DE TEXTURAS
        // ==================================================================

        /// <summary>
        /// Crea un círculo blanco. Con "difuso = true" el borde se desvanece de a poco
        /// (sirve para brillos de luces y sombras debajo de los autos).
        /// IMPORTANTE: MonoGame trabaja con "alfa premultiplicado", por eso usamos
        /// Color.White * alfa (multiplica también R, G y B) y no new Color(255,255,255,alfa).
        /// </summary>
        private static Texture2D CrearCirculo(GraphicsDevice graphicsDevice, int tamano, bool difuso)
        {
            Color[] datos = new Color[tamano * tamano];
            float radio = tamano / 2f;

            for (int y = 0; y < tamano; y++)
            {
                for (int x = 0; x < tamano; x++)
                {
                    float dx = x + 0.5f - radio;
                    float dy = y + 0.5f - radio;
                    float distancia = (float)Math.Sqrt(dx * dx + dy * dy);

                    float alfa;
                    if (difuso)
                    {
                        float t = MathHelper.Clamp(1f - distancia / radio, 0f, 1f);
                        alfa = t * t;
                    }
                    else
                    {
                        alfa = MathHelper.Clamp(radio - distancia, 0f, 1f);
                    }

                    datos[y * tamano + x] = Color.White * alfa;
                }
            }

            Texture2D textura = new Texture2D(graphicsDevice, tamano, tamano);
            textura.SetData(datos);
            return textura;
        }

        private static Texture2D CrearDegradeVertical(GraphicsDevice graphicsDevice, int alto)
        {
            Color[] datos = new Color[alto];
            for (int y = 0; y < alto; y++)
            {
                datos[y] = Color.White * (1f - y / (float)(alto - 1));
            }

            Texture2D textura = new Texture2D(graphicsDevice, 1, alto);
            textura.SetData(datos);
            return textura;
        }

        /// <summary>
        /// Devuelve el rectángulo donde la imagen tiene píxeles visibles (sin el
        /// borde transparente). Muchas imágenes generadas con IA traen mucho "aire"
        /// alrededor y eso hacía que se vieran de distinto tamaño aunque las
        /// dibujáramos en rectángulos iguales. Se calcula una sola vez por textura.
        /// </summary>
        public static Rectangle LimitesOpacos(Texture2D textura)
        {
            if (textura == null)
                return Rectangle.Empty;

            if (_cacheLimites.TryGetValue(textura, out Rectangle guardado))
                return guardado;

            Color[] datos = new Color[textura.Width * textura.Height];
            textura.GetData(datos);

            int minX = textura.Width, minY = textura.Height, maxX = -1, maxY = -1;
            for (int y = 0; y < textura.Height; y++)
            {
                int fila = y * textura.Width;
                for (int x = 0; x < textura.Width; x++)
                {
                    if (datos[fila + x].A > 10)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            Rectangle resultado = maxX < 0
                ? textura.Bounds
                : new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);

            _cacheLimites[textura] = resultado;
            return resultado;
        }

        // ==================================================================
        // FIGURAS BÁSICAS
        // ==================================================================

        public static void Rect(SpriteBatch spriteBatch, Rectangle rectangulo, Color color)
        {
            spriteBatch.Draw(Pixel, rectangulo, color);
        }

        public static void Borde(SpriteBatch spriteBatch, Rectangle r, int grosor, Color color)
        {
            Rect(spriteBatch, new Rectangle(r.X, r.Y, r.Width, grosor), color);
            Rect(spriteBatch, new Rectangle(r.X, r.Bottom - grosor, r.Width, grosor), color);
            Rect(spriteBatch, new Rectangle(r.X, r.Y + grosor, grosor, r.Height - grosor * 2), color);
            Rect(spriteBatch, new Rectangle(r.Right - grosor, r.Y + grosor, grosor, r.Height - grosor * 2), color);
        }

        /// <summary>Sombra suave: varios rectángulos cada vez más grandes y más transparentes.</summary>
        public static void Sombra(SpriteBatch spriteBatch, Rectangle r, int distancia, float opacidad)
        {
            for (int i = distancia; i >= 1; i -= 2)
            {
                Rectangle capa = new Rectangle(r.X - i + 2, r.Y - i + 4, r.Width + i * 2 - 4, r.Height + i * 2 - 4);
                Rect(spriteBatch, capa, Color.Black * (opacidad / distancia * 1.6f));
            }
        }

        /// <summary>Degradé de arriba (color opaco) hacia abajo (transparente) o al revés.</summary>
        public static void Degrade(SpriteBatch spriteBatch, Rectangle r, Color color, bool opacoAbajo = false)
        {
            spriteBatch.Draw(DegradeVertical, r, null, color, 0f, Vector2.Zero,
                opacoAbajo ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
        }

        /// <summary>Brillo circular difuso centrado en un punto (luces, reflectores, sombras).</summary>
        public static void Resplandor(SpriteBatch spriteBatch, Vector2 centro, float radioX, float radioY, Color color)
        {
            Rectangle destino = new Rectangle(
                (int)(centro.X - radioX), (int)(centro.Y - radioY),
                (int)(radioX * 2), (int)(radioY * 2));
            spriteBatch.Draw(Brillo, destino, color);
        }

        public static void CirculoLleno(SpriteBatch spriteBatch, Vector2 centro, float radio, Color color)
        {
            Rectangle destino = new Rectangle(
                (int)(centro.X - radio), (int)(centro.Y - radio),
                (int)(radio * 2), (int)(radio * 2));
            spriteBatch.Draw(Circulo, destino, color);
        }

        // ==================================================================
        // COMPONENTES DE INTERFAZ
        // ==================================================================

        /// <summary>Panel oscuro con sombra, brillo arriba y borde fino.</summary>
        public static void Panel(SpriteBatch spriteBatch, Rectangle r, float opacidad = 0.94f, Color? colorAcento = null)
        {
            Sombra(spriteBatch, r, 12, 0.5f);
            Rect(spriteBatch, r, ColorPanel * opacidad);
            Degrade(spriteBatch, new Rectangle(r.X, r.Y, r.Width, Math.Min(80, r.Height)), Color.White * 0.045f);
            Borde(spriteBatch, r, 1, BordePanel * 0.9f);

            if (colorAcento.HasValue)
            {
                Rect(spriteBatch, new Rectangle(r.X, r.Y, r.Width, 4), colorAcento.Value);
            }
        }

        /// <summary>
        /// Botón con estado: normal, "hover" (mouse encima) y deshabilitado.
        /// </summary>
        public static void Boton(SpriteBatch spriteBatch, Rectangle r, string texto, bool mouseEncima,
            bool habilitado = true, Color? colorBase = null, SpriteFont fuente = null)
        {
            fuente = fuente ?? Negrita;

            Color fondo = colorBase ?? ColorPanelClaro;
            if (!habilitado)
                fondo = new Color(34, 37, 46);
            else if (mouseEncima)
                fondo = Color.Lerp(fondo, Color.White, 0.18f);

            Sombra(spriteBatch, r, 6, 0.4f);
            Rect(spriteBatch, r, fondo);
            Degrade(spriteBatch, new Rectangle(r.X, r.Y, r.Width, r.Height / 2), Color.White * (mouseEncima && habilitado ? 0.16f : 0.07f));
            Rect(spriteBatch, new Rectangle(r.X, r.Bottom - 3, r.Width, 3), Color.Black * 0.3f);

            bool resaltar = mouseEncima && habilitado;
            Borde(spriteBatch, r, resaltar ? 2 : 1, resaltar ? Acento : Color.Black * 0.6f);

            Color colorTexto = habilitado ? TextoClaro : TextoSecundario * 0.6f;
            TextoCentrado(spriteBatch, fuente, texto, new Vector2(r.Center.X, r.Center.Y + 1), colorTexto);
        }

        /// <summary>"Píldora" chiquita con etiqueta y valor (por ejemplo: DINERO  $ 7.000.000).</summary>
        public static Rectangle Chip(SpriteBatch spriteBatch, Vector2 posicion, string etiqueta, string valor, Color colorValor)
        {
            Vector2 medidaEtiqueta = Fuente.MeasureString(etiqueta);
            Vector2 medidaValor = Negrita.MeasureString(valor);

            int ancho = (int)(medidaEtiqueta.X + medidaValor.X + 44);
            int alto = (int)Math.Max(medidaValor.Y, medidaEtiqueta.Y) + 14;
            Rectangle r = new Rectangle((int)posicion.X, (int)posicion.Y, ancho, alto);

            Rect(spriteBatch, r, Color.Black * 0.55f);
            Borde(spriteBatch, r, 1, colorValor * 0.6f);
            Rect(spriteBatch, new Rectangle(r.X, r.Y, 4, r.Height), colorValor);

            Texto(spriteBatch, Fuente, etiqueta, new Vector2(r.X + 16, r.Center.Y - medidaEtiqueta.Y / 2f), TextoSecundario, 1f, false);
            Texto(spriteBatch, Negrita, valor, new Vector2(r.X + 28 + medidaEtiqueta.X, r.Center.Y - medidaValor.Y / 2f), colorValor);
            return r;
        }

        /// <summary>Ancho que va a ocupar un Chip (para poder alinearlo a la derecha).</summary>
        public static int AnchoChip(string etiqueta, string valor)
        {
            return (int)(Fuente.MeasureString(etiqueta).X + Negrita.MeasureString(valor).X + 44);
        }

        /// <summary>Barra de progreso / estadística, valor entre 0 y 1.</summary>
        public static void Barra(SpriteBatch spriteBatch, Rectangle r, float valor, Color color)
        {
            valor = MathHelper.Clamp(valor, 0f, 1f);
            Rect(spriteBatch, r, Color.Black * 0.5f);
            Rect(spriteBatch, new Rectangle(r.X, r.Y, (int)(r.Width * valor), r.Height), color);
            Degrade(spriteBatch, new Rectangle(r.X, r.Y, (int)(r.Width * valor), r.Height), Color.White * 0.25f);
            Borde(spriteBatch, r, 1, Color.Black * 0.6f);
        }

        /// <summary>Tecla dibujada como "tecla de teclado" con su descripción al lado. Devuelve el ancho usado.</summary>
        public static int Tecla(SpriteBatch spriteBatch, Vector2 posicion, string tecla, string descripcion)
        {
            Vector2 medidaTecla = Negrita.MeasureString(tecla);
            Rectangle r = new Rectangle((int)posicion.X, (int)posicion.Y, (int)Math.Max(34, medidaTecla.X + 18), 32);

            Rect(spriteBatch, new Rectangle(r.X, r.Y + 3, r.Width, r.Height), Color.Black * 0.6f);
            Rect(spriteBatch, r, new Color(230, 232, 238));
            Rect(spriteBatch, new Rectangle(r.X, r.Bottom - 4, r.Width, 4), new Color(170, 174, 186));
            TextoCentrado(spriteBatch, Negrita, tecla, new Vector2(r.Center.X, r.Center.Y - 1), new Color(30, 32, 40), 1f, false);

            Vector2 medidaDescripcion = Fuente.MeasureString(descripcion);
            Texto(spriteBatch, Fuente, descripcion, new Vector2(r.Right + 10, r.Center.Y - medidaDescripcion.Y / 2f), TextoClaro);

            return r.Width + 10 + (int)medidaDescripcion.X + 26;
        }

        // ==================================================================
        // TEXTO
        // ==================================================================

        /// <summary>Texto con sombrita abajo a la derecha: se lee bien sobre cualquier fondo.</summary>
        public static void Texto(SpriteBatch spriteBatch, SpriteFont fuente, string texto, Vector2 posicion, Color color,
            float escala = 1f, bool sombra = true)
        {
            // Redondear evita que el texto se vea borroso por quedar "entre píxeles".
            posicion = new Vector2((float)Math.Round(posicion.X), (float)Math.Round(posicion.Y));

            if (sombra)
            {
                float desplazamiento = Math.Max(1f, 2f * escala);
                spriteBatch.DrawString(fuente, texto, posicion + new Vector2(desplazamiento, desplazamiento),
                    Color.Black * 0.65f * (color.A / 255f), 0f, Vector2.Zero, escala, SpriteEffects.None, 0f);
            }

            spriteBatch.DrawString(fuente, texto, posicion, color, 0f, Vector2.Zero, escala, SpriteEffects.None, 0f);
        }

        public static void TextoCentrado(SpriteBatch spriteBatch, SpriteFont fuente, string texto, Vector2 centro, Color color,
            float escala = 1f, bool sombra = true)
        {
            Vector2 medida = fuente.MeasureString(texto) * escala;
            Texto(spriteBatch, fuente, texto, centro - medida / 2f, color, escala, sombra);
        }

        /// <summary>Texto alineado a la derecha: "posicion" es la esquina superior DERECHA.</summary>
        public static void TextoDerecha(SpriteBatch spriteBatch, SpriteFont fuente, string texto, Vector2 posicion, Color color,
            float escala = 1f, bool sombra = true)
        {
            Vector2 medida = fuente.MeasureString(texto) * escala;
            Texto(spriteBatch, fuente, texto, new Vector2(posicion.X - medida.X, posicion.Y), color, escala, sombra);
        }

        /// <summary>Formato argentino: 7000000 -> "$ 7.000.000".</summary>
        public static string Dinero(decimal cantidad)
        {
            return "$ " + cantidad.ToString("N0", _formatoPesos);
        }

        // ==================================================================
        // FONDOS
        // ==================================================================

        /// <summary>
        /// Calcula qué parte de una imagen hay que usar para llenar la pantalla
        /// SIN deformarla (como "background-size: cover" en CSS). Antes los fondos
        /// se estiraban y en monitores que no son 16:9 se veían aplastados.
        /// </summary>
        public static Rectangle RecorteParaCubrir(Texture2D textura, int anchoPantalla, int altoPantalla)
        {
            float aspectoTextura = textura.Width / (float)textura.Height;
            float aspectoPantalla = anchoPantalla / (float)altoPantalla;

            if (aspectoPantalla > aspectoTextura)
            {
                // Pantalla más ancha: usamos todo el ancho y recortamos arriba y abajo.
                int altoFuente = (int)(textura.Width / aspectoPantalla);
                return new Rectangle(0, (textura.Height - altoFuente) / 2, textura.Width, altoFuente);
            }
            else
            {
                // Pantalla más angosta: usamos todo el alto y recortamos los costados.
                int anchoFuente = (int)(textura.Height * aspectoPantalla);
                return new Rectangle((textura.Width - anchoFuente) / 2, 0, anchoFuente, textura.Height);
            }
        }

        public static void FondoCubriendo(SpriteBatch spriteBatch, Texture2D textura, int anchoPantalla, int altoPantalla, Color tinte)
        {
            Rectangle recorte = RecorteParaCubrir(textura, anchoPantalla, altoPantalla);
            spriteBatch.Draw(textura, new Rectangle(0, 0, anchoPantalla, altoPantalla), recorte, tinte);
        }

        /// <summary>
        /// Convierte un punto de la imagen de fondo (u, v entre 0 y 1) a coordenadas de pantalla,
        /// teniendo en cuenta el recorte de FondoCubriendo. Sirve para apoyar el auto
        /// justo sobre la mancha del piso del taller en cualquier resolución.
        /// </summary>
        public static Vector2 PuntoDelFondo(Texture2D textura, int anchoPantalla, int altoPantalla, float u, float v)
        {
            Rectangle recorte = RecorteParaCubrir(textura, anchoPantalla, altoPantalla);
            float escala = anchoPantalla / (float)recorte.Width;
            return new Vector2(
                (u * textura.Width - recorte.X) * escala,
                (v * textura.Height - recorte.Y) * escala);
        }

        /// <summary>Escala que tiene el fondo al cubrir la pantalla (píxeles de pantalla por píxel de imagen).</summary>
        public static float EscalaDelFondo(Texture2D textura, int anchoPantalla, int altoPantalla)
        {
            Rectangle recorte = RecorteParaCubrir(textura, anchoPantalla, altoPantalla);
            return anchoPantalla / (float)recorte.Width;
        }

        /// <summary>Oscurece los bordes superior e inferior de la pantalla (efecto "cine").</summary>
        public static void Vineta(SpriteBatch spriteBatch, int ancho, int alto, float intensidad)
        {
            int franja = alto / 3;
            Degrade(spriteBatch, new Rectangle(0, 0, ancho, franja), Color.Black * intensidad);
            Degrade(spriteBatch, new Rectangle(0, alto - franja, ancho, franja), Color.Black * intensidad, true);
        }

        // ==================================================================
        // UTILIDADES
        // ==================================================================

        /// <summary>Número "al azar" pero siempre igual para el mismo índice (para generar edificios fijos).</summary>
        public static float AzarFijo(int indice, int semilla)
        {
            unchecked
            {
                uint h = (uint)(indice * 374761393 + semilla * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFF) / 65535f;
            }
        }

        /// <summary>Mueve un valor hacia un objetivo de forma suave (para animaciones de hover).</summary>
        public static float Acercar(float actual, float objetivo, float velocidad, float dt)
        {
            float diferencia = objetivo - actual;
            float paso = velocidad * dt;
            if (Math.Abs(diferencia) <= paso)
                return objetivo;
            return actual + Math.Sign(diferencia) * paso;
        }
    }
}
