using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace La35Tunning.Pantallas
{
    public interface IPantalla
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
    }
}