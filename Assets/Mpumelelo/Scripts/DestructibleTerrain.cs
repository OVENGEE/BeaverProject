using System.Collections.Generic;
using Unity.VisualScripting;
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

    [SerializeField]
    private ChunkManager m_chunkManager;

    [SerializeField]
    private Vector2Int m_chunkSize = new (300, 300);

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

        //Vector2 size = m_modifiableTexture.Sprite.bounds.size;
        Vector2 bottomLeftPixel = - m_modifiableTexture.Sprite.pivot;

        Vector2Int chunkGridSize = SplitTextureIntoChunks(m_modifiableTexture.Texture.width, m_modifiableTexture.Texture.height, m_chunkSize);

        bool[][] pixels = m_modifiableTexture.GetPixelsState();

        PrepareColliderChunks(chunkGridSize, m_chunkSize, bottomLeftPixel, pixels);
        // Vector2 bottomLeftWorld = m_spriteRenderer.transform.TransformPoint(bottomLeftLocal);

        // m_nonChunkedCollider = Instantiate(m_colliderGenerator, bottomLeftWorld, Quaternion.identity, m_grid.transform);
        // m_nonChunkedCollider.PrepareCollider(m_modifiableTexture.GetPixelsState());

    }

    private void PrepareColliderChunks(Vector2Int chunkGridSize, Vector2Int mChunkSize, Vector2 bottomLeftLocal, bool[][] pixels)
    {
        for (int x = 0; x < chunkGridSize.x; x++)
        {
            for (int y = 0; y < chunkGridSize.y; y++)
            {
                Vector3Int offset = new Vector3Int(x * mChunkSize.x, y * mChunkSize.y, 0);
                Vector3 bottomLeftCorner = (Vector3)bottomLeftLocal + offset;
                bottomLeftCorner.Scale(m_grid.cellSize);
                bottomLeftCorner = m_spriteRenderer.transform.TransformPoint(bottomLeftCorner);

                TilemapColliderGenerator colliderGenerator = Instantiate(m_colliderGenerator, bottomLeftCorner, Quaternion.identity, m_grid.transform);

                colliderGenerator.gameObject.name = $"Chunk_{x}_{y}";
                m_chunkManager.AddChunk(colliderGenerator);

                bool[][] chunkPixels = SliceArray(pixels, offset.y, offset.x, mChunkSize.y, mChunkSize.x);
                colliderGenerator.PrepareCollider(chunkPixels);
            }
        }
    }

    private bool[][] SliceArray(bool[][] pixels, int startRow, int startCol, int numRows, int numCols)
    {
        int sourceWidth = pixels.Length;
        int sourceHeight = pixels[0].Length;

        int actualWidth = Mathf.Min(numCols, sourceWidth - startRow);
        int actualHeight = Mathf.Min(numRows, sourceHeight - startCol);

        actualWidth = Mathf.Max(0, actualWidth);
        actualHeight = Mathf.Max(0, actualHeight);

        bool[][] result = new bool[actualHeight][];
        for (int row = 0; row < actualHeight; row++)
        {
            result[row] = new bool[actualWidth];
            for (int col = 0; col < actualWidth; col++)
            {
                result[row][col] = pixels[startRow + row][startCol + col];
            }
        }

        return result;
    }

    private Vector2Int SplitTextureIntoChunks(int width, int height, Vector2Int mChunkSize)
    {
        int chunkCountRight = Mathf.CeilToInt((float)width / mChunkSize.x);
        int chunkCountUp = Mathf.CeilToInt((float)height / mChunkSize.y);

        return new Vector2Int(chunkCountRight, chunkCountUp);
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

        //m_nonChunkedCollider.DestroyCollider(worldPosition, affectedPixelAsOffset);

        List<TilemapColliderGenerator> chunksToModify = m_chunkManager.GetClosestChunks(worldPosition);
        foreach(var chunk in chunksToModify)
        {
            chunk.DestroyCollider(worldPosition, affectedPixelAsOffset);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            if(m_chunkManager != null)
            {
                Vector3 chunkSize = new(m_chunkSize.x * m_grid.cellSize.x, m_chunkSize.y * m_grid.cellSize.y);
                Vector3 halfSize = chunkSize / 2f;
                m_chunkManager.DrawGizmos(chunkSize, halfSize);
            }
        }
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
