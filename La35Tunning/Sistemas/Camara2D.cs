using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace La35Tunning.Sistemas
{
    public class Camera2D
    {
        public Matrix Transform { get; private set; }
        public float Zoom { get; set; } = 0.5f;
        private GraphicsDevice _graphicsDevice;

        /// <summary>NUEVO: punto del mundo que está en el centro de la pantalla.</summary>
        public Vector2 Centro { get; private set; }

        public Camera2D(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public void Update(Vector2 targetPosition)
        {
            var viewport = _graphicsDevice.Viewport;
            Centro = targetPosition;

            Transform = Matrix.CreateTranslation(new Vector3(-targetPosition.X, -targetPosition.Y, 0)) *
                        Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                        Matrix.CreateTranslation(new Vector3(viewport.Width / 2f, viewport.Height / 2f, 0));
        }

        /// <summary>NUEVO: convierte un punto del mundo a píxeles de pantalla (útil para el fondo con parallax).</summary>
        public Vector2 MundoAPantalla(Vector2 puntoMundo)
        {
            return Vector2.Transform(puntoMundo, Transform);
        }
    }
}
