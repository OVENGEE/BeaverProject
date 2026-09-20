using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapColliderGenerator : MonoBehaviour
{
    [SerializeField] 
    private Tilemap m_tilemap;
    private Tile m_tile;

    public Vector3 Center { get; private set; }

    // Creates the tile used for generated collider blocks.
    private void Awake()
    {
        m_tile = ScriptableObject.CreateInstance<Tile>();
        m_tile.colliderType = Tile.ColliderType.Grid;
    }

    // Fills the chunk with solid tiles based on the provided pixel map.
    public void PrepareCollider(bool[][] pixelState)
    {
        for (int y= 0; y < pixelState.Length; y++)
        {
            for (int x = 0; x < pixelState[y].Length; x++)
            {
                m_tilemap.SetTile(new Vector3Int(x, y, 0),pixelState[y][x] ? m_tile : null);               
            }
        }

        Vector3Int centerTile = new (pixelState[0].Length / 2, pixelState.Length / 2);
        Center = m_tilemap.CellToWorld(centerTile);
    }

    // Removes terrain tiles inside the destruction radius for this chunk.
    public void DestroyCollider(Vector2 originWorldSpace, List<Vector2Int> affectedTilesAsOffset)
    {
        Vector3Int originCell = m_tilemap.WorldToCell(originWorldSpace);
        foreach (Vector2Int cell in affectedTilesAsOffset)
        {
            Vector3Int tilePosition = originCell + (Vector3Int)cell;
            if(m_tilemap.HasTile(tilePosition))
            {
                m_tilemap.SetTile(tilePosition, null);
            }
            
        }
    } 
}
