using System.Collections.Generic;
using UnityEngine;

public class TerrainStreamer : MonoBehaviour
{
    // Collider that determines which chunks should be loaded
    public Collider GenCircle;
    // Width and depth of each terrain chunk
    public int chunkSize = 50;
    // Chunk prefab that will be instantiated
    public GameObject terrainChunkPrefab;

    // Tracks which chunks are loaded and their GameObjects
    private Dictionary<Vector2Int, GameObject> loadedChunks
        = new Dictionary<Vector2Int, GameObject>();

    void Update()
    {
        Bounds bounds = GenCircle.bounds;

        // Compute chunk grid range intersecting the circle
        Vector2Int minChunk = WorldToChunkCoord(bounds.min);
        Vector2Int maxChunk = WorldToChunkCoord(bounds.max);

        // 1) Load any new chunks in range
        for (int x = minChunk.x; x <= maxChunk.x; x++)
        {
            for (int z = minChunk.y; z <= maxChunk.y; z++)
            {
                Vector2Int coord = new Vector2Int(x, z);
                if (loadedChunks.ContainsKey(coord))
                    continue;

                Bounds chunkBounds = new Bounds(
                    ChunkToWorldPosition(coord) + new Vector3(chunkSize / 2f, 0f, chunkSize / 2f),
                    new Vector3(chunkSize, 1000f, chunkSize));

                if (bounds.Intersects(chunkBounds))
                {
                    LoadChunk(coord);
                }
            }
        }

        // 2) Unload any chunks that are now outside the circle
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, GameObject> kvp in loadedChunks)
        {
            Vector2Int coord = kvp.Key;
            Bounds chunkBounds = new Bounds(
                ChunkToWorldPosition(coord) + new Vector3(chunkSize / 2f, 0f, chunkSize / 2f),
                new Vector3(chunkSize, 1000f, chunkSize));

            if (!bounds.Intersects(chunkBounds))
            {
                Destroy(kvp.Value);
                toRemove.Add(coord);
            }
        }

        // Remove unloaded keys
        foreach (Vector2Int coord in toRemove)
        {
            loadedChunks.Remove(coord);
        }
    }

    private void LoadChunk(Vector2Int chunkCoord)
    {
        Vector3 worldPos = ChunkToWorldPosition(chunkCoord);
        GameObject chunkGO = Instantiate(
            terrainChunkPrefab,
            worldPos,
            Quaternion.identity);

        loadedChunks.Add(chunkCoord, chunkGO);
    }

    // Converts a world position to chunk grid coordinates
    private Vector2Int WorldToChunkCoord(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / chunkSize);
        int z = Mathf.FloorToInt(position.z / chunkSize);
        return new Vector2Int(x, z);
    }

    // Converts a chunk coordinate back to its world-space origin point
    private Vector3 ChunkToWorldPosition(Vector2Int chunkCoord)
    {
        return new Vector3(
            chunkCoord.x * chunkSize,
            0f,
            chunkCoord.y * chunkSize);
    }
}