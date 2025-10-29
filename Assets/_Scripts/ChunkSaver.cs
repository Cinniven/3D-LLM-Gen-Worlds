using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ChunkSaver : MonoBehaviour
{
    [SerializeField] private Transform chunkToSave;

    [ContextMenu("Save Selected Chunk")]
    public void SaveChunk()
    {
        string filename = "ExampleJson";
        Chunk chunk = new Chunk();

        foreach(Transform child in chunkToSave)
        {
            ObjectData data = new ObjectData
            {
                name = child.name,
                position = child.position,
                rotation = child.rotation,
                size = child.localScale
            };

            chunk.objects.Add(data);
        }

        string json = JsonUtility.ToJson(chunk, true);
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllText(path, json);

        Debug.Log($"Chunk saved to: {path}");
    }
}
