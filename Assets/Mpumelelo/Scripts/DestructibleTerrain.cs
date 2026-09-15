using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DestructibleTerrain : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer m_spriteRenderer;

    private ModifiableTexture m_modifiableTexture;

    [SerializeField]
    private TilemapColliderGenerator m_colliderGenerator;
    private TilemapColliderGenerator m_nonChunkedCollider;

    [SerializeField]
    private Grid m_grid;

    private void Reset()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_grid = GetComponent<Grid>();
    }

    private void Awake()
    {
        if (m_spriteRenderer == null)
            m_spriteRenderer = GetComponent<SpriteRenderer>();

        if (m_spriteRenderer == null)
        {
            Debug.LogError($"{nameof(DestructibleTerrain)} requires a SpriteRenderer on the same GameObject.", this);
            enabled = false;
            return;
        }

        if (m_spriteRenderer.sprite == null)
        {
            Debug.LogError($"{nameof(DestructibleTerrain)} requires a Sprite assigned to the SpriteRenderer.", this);
            enabled = false;
            return;
        }

        m_modifiableTexture = ModifiableTexture.CreateFromSprite(m_spriteRenderer.sprite);
        m_spriteRenderer.sprite = m_modifiableTexture.Sprite;
        float pixelSize = 1f / m_modifiableTexture.Sprite.pixelsPerUnit;
        m_grid.cellSize = new Vector2(pixelSize, pixelSize);

        Vector2 size = m_modifiableTexture.Sprite.bounds.size;
        Vector2 bottomLeftLocal = -size * m_modifiableTexture.Pivot;
        Vector2 bottomLeftWorld = m_spriteRenderer.transform.TransformPoint(bottomLeftLocal);

        m_nonChunkedCollider = Instantiate(m_colliderGenerator, bottomLeftWorld, Quaternion.identity, m_grid.transform);
        m_nonChunkedCollider.PrepareCollider(m_modifiableTexture.GetPixelsState());
    }

    public void RemoveTerrainAt(Vector2 worldPosition, float radius)
    {
        if (m_modifiableTexture == null || m_spriteRenderer == null || radius <= 0f)
            return;

        float pixelSize = 1f / m_modifiableTexture.Sprite.pixelsPerUnit;
        int radiusInPixels = Mathf.RoundToInt(radius / pixelSize);
        List<Vector2Int> affectedPixelAsOffset = GetCircleOffsets(radiusInPixels);

        Vector2Int circleCenterInPixelSpace = m_modifiableTexture.WorldToTexturePosition(worldPosition, m_spriteRenderer.transform);
        ModifyTextureAt(circleCenterInPixelSpace, Color.clear, affectedPixelAsOffset);

        m_nonChunkedCollider.DestroyCollider(worldPosition, affectedPixelAsOffset);
    }

    private void ModifyTextureAt(Vector2Int circleCenterInPixelSpace, Color color, List<Vector2Int> affectedPixelAsOffset)
    {
        foreach (Vector2Int offset in affectedPixelAsOffset)
        {
            Vector2Int pixelPosition = circleCenterInPixelSpace + offset;
            m_modifiableTexture.SetPixel(pixelPosition, color);
        }

        m_modifiableTexture.ApplyChanges();
    }

    private List<Vector2Int> GetCircleOffsets(int radiusInPixels)
    {
        List<Vector2Int> affectedPixelAsOffset = new List<Vector2Int>();
        for (int x = -radiusInPixels; x <= radiusInPixels; x++)
        {
            for (int y = -radiusInPixels; y <= radiusInPixels; y++)
            {
                if (x * x + y * y <= radiusInPixels * radiusInPixels)
                {
                    affectedPixelAsOffset.Add(new Vector2Int(x, y));
                }
            }
        }

        return affectedPixelAsOffset;
    }
}
