using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;

public class SteamDataWrapper
{
    public static async Task<Texture2D> GetTextureFromSteamIdAsync(SteamId id)
    {
        var img = await SteamFriends.GetLargeAvatarAsync(id);
        return GetTextureFromImage(img.Value);
    }

    public static Texture2D GetTextureFromImage(Image image)
    {
        Texture2D texture = new Texture2D((int)image.Width, (int)image.Height);

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                var p = image.GetPixel(x, y);
                texture.SetPixel(x, (int)image.Height - y, new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
            }
        }
        texture.Apply();
        return texture;
    }
}
