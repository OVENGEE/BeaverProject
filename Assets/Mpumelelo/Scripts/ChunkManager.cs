using UnityEngine;
using System.Collections.Generic;
public class ChunkManager : MonoBehaviour
{
    // Tracks every terrain chunk managed by this object.
    List<TilemapColliderGenerator> m_chunks = new();

    // Adds a chunk to the internal list so it can be queried later.
    public void AddChunk(TilemapColliderGenerator chunk)
    {
        m_chunks.Add(chunk);
    }

    // Finds the chunks closest to a world position and returns up to four results.
    public List<TilemapColliderGenerator> GetClosestChunks(Vector2 worldPosition)
    {
        List<(float distance, TilemapColliderGenerator chunk)> closestChunks = new();
        foreach (TilemapColliderGenerator chunk in m_chunks)
        {
            float distance = Vector3.Distance(chunk.Center, worldPosition);
            closestChunks.Add((distance, chunk));
        }

        closestChunks.Sort((a, b) => a.distance.CompareTo(b.distance));
        
        List<TilemapColliderGenerator> ClosestChunksToRemove = new();
        int count = Mathf.Min(4, closestChunks.Count);
        for(int i = 0; i < count; i++)
        {
            ClosestChunksToRemove.Add(closestChunks[i].chunk);
        }
        return ClosestChunksToRemove;
    } 

    // Draws the chunk bounds in the editor to help debug terrain layout.
    public void DrawGizmos(Vector3 chunkSize , Vector3 halfSize)
    {
        if (m_chunks.Count > 0)
        {
            foreach (TilemapColliderGenerator chunk in m_chunks)
            {
                Gizmos.DrawWireCube(chunk.transform.position + halfSize, chunkSize);
                Gizmos.DrawSphere(chunk.transform.position, 0.1f);
            }
        }
    }
}
