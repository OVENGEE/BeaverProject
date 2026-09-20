using UnityEngine;

public class ModifiableTexture
{
    private Texture2D m_texture;
    private Sprite m_sprite;
    private Vector2 m_pivot;
    private float m_pixelsPerUnit;

    public Texture2D Texture => m_texture;
    public Sprite Sprite => m_sprite;

    public Vector2 Pivot => m_pivot;

    // Creates a writable copy of a sprite so pixel changes can be applied safely.
    public static ModifiableTexture CreateFromSprite(Sprite sprite)
    {
        if (sprite == null)
            throw new System.ArgumentNullException(nameof(sprite));

        Rect rect = sprite.rect;
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
        texture.filterMode = FilterMode.Point;

        int sourceX = Mathf.RoundToInt(rect.xMin);
        int sourceY = Mathf.RoundToInt(rect.yMin);
        Texture2D sourceTexture = sprite.texture;
        Color32[] sourcePixels = sourceTexture.GetPixels32();
        Color32[] copiedPixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int sourceIndex = (sourceY + y) * sourceTexture.width + (sourceX + x);
                int destIndex = y * width + x;
                copiedPixels[destIndex] = sourcePixels[sourceIndex];
            }
        }

        texture.SetPixels32(copiedPixels);
        texture.Apply();

        Vector2 normalizedPivot = new Vector2(sprite.pivot.x / rect.width, sprite.pivot.y / rect.height);
        return new ModifiableTexture(texture, normalizedPivot, sprite.pixelsPerUnit);
    }

    // Stores the texture and recreates the sprite used for rendering.
    private ModifiableTexture(Texture2D texture, Vector2 pivot, float pixelsPerUnit)
    {
        m_texture = texture;
        m_pivot = pivot;
        m_pixelsPerUnit = pixelsPerUnit;
        RecreateSprite();
    }

    // Rebuilds the sprite after the underlying texture changes.
    private void RecreateSprite()
    {
        m_sprite = Sprite.Create(
            m_texture,
            new Rect(0, 0, m_texture.width, m_texture.height),
            m_pivot,
            m_pixelsPerUnit,
            extrude: 0,
            SpriteMeshType.FullRect,
            Vector4.zero,
            false);
    }

    // Converts a world point into the matching pixel coordinate on the texture.
    public Vector2Int WorldToTexturePosition(Vector2 worldPosition, Transform spriteTransform)
    {
        Vector2 localPosition = spriteTransform.InverseTransformPoint(worldPosition);
        int x = Mathf.FloorToInt(localPosition.x * m_pixelsPerUnit + m_sprite.pivot.x);
        int y = Mathf.FloorToInt(localPosition.y * m_pixelsPerUnit + m_sprite.pivot.y);
        return new Vector2Int(x, y);
    }

    // Checks whether a pixel coordinate is within the texture bounds.
    public bool IsValidTexturePosition(Vector2Int texturePosition)
    {
        return texturePosition.x >= 0 && texturePosition.x < m_texture.width &&
               texturePosition.y >= 0 && texturePosition.y < m_texture.height;
    }

    // Sets one texture pixel and returns whether the position was valid.
    public bool SetPixel(Vector2Int texturePosition, Color color)
    {
        if (!IsValidTexturePosition(texturePosition))
            return false;

        m_texture.SetPixel(texturePosition.x, texturePosition.y, color);
        return true;
    }

    // Applies pending texture edits so they can be read immediately.
    public void ApplyChanges()
    {
        m_texture.Apply();
    }

    // Reads the alpha state into a grid of solid and empty cells.
    public bool[][] GetPixelsState()
    {
        int width = m_texture.width;
        int height = m_texture.height;
        Color[] data = m_texture.GetPixels();
        bool[][] pixels = new bool[height][];
        for (int i = 0; i < height; i++)
        {
            pixels[i] = new bool[width];
            int row = i * width;
            for (int j = 0; j < width; j++)
            {
                pixels[i][j] = data[row + j].a > 0.5f;
            }
        }
        return pixels;
    }
}
