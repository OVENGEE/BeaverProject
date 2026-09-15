using UnityEngine;

public class ModifiableTexture
{
    private Texture2D m_texture;
    private Sprite m_sprite;
    private Vector2 m_pivot;
    private float m_pixelsPerUnit;

    public Texture2D Texture => m_texture;
    public Sprite Sprite => m_sprite;

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

    private ModifiableTexture(Texture2D texture, Vector2 pivot, float pixelsPerUnit)
    {
        m_texture = texture;
        m_pivot = pivot;
        m_pixelsPerUnit = pixelsPerUnit;
        RecreateSprite();
    }

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

    public Vector2Int WorldToTexturePosition(Vector2 worldPosition, Transform spriteTransform)
    {
        Vector2 localPosition = spriteTransform.InverseTransformPoint(worldPosition);
        int x = Mathf.FloorToInt(localPosition.x * m_pixelsPerUnit + m_sprite.pivot.x);
        int y = Mathf.FloorToInt(localPosition.y * m_pixelsPerUnit + m_sprite.pivot.y);
        return new Vector2Int(x, y);
    }

    public bool IsValidTexturePosition(Vector2Int texturePosition)
    {
        return texturePosition.x >= 0 && texturePosition.x < m_texture.width &&
               texturePosition.y >= 0 && texturePosition.y < m_texture.height;
    }

    public bool SetPixel(Vector2Int texturePosition, Color color)
    {
        if (!IsValidTexturePosition(texturePosition))
            return false;

        m_texture.SetPixel(texturePosition.x, texturePosition.y, color);
        return true;
    }

    public void ApplyChanges()
    {
        m_texture.Apply();
    }
}
