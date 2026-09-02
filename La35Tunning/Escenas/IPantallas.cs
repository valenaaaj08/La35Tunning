using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace La35Tunning.Escenas
{
    public interface IPantallas
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
    }
}