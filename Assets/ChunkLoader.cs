using System.IO;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    [SerializeField] private Transform chunkToLoad;
    [SerializeField] private GameObject treePrefab, bushPrefab, housePrefab; // a generic prefab to represent loaded objects

    public void LoadChunk(string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (!File.Exists(path))
        {
            Debug.LogWarning("Chunk file not found!");
            return;
        }

        string json = File.ReadAllText(path);
        Chunk chunk = JsonUtility.FromJson<Chunk>(json);

        foreach (Transform child in chunkToLoad)
            Destroy(child.gameObject);

        foreach (ObjectData data in chunk.objects)
        {
            GameObject obj = Instantiate(GetPrefab(data.name), data.position, data.rotation, chunkToLoad);
            obj.name = data.name;
            obj.transform.localScale = data.size;
        }

        Debug.Log($"Loaded {chunk.objects.Count} objects from {filename}");
    }

    [ContextMenu("Load Selected Chunk")]
    public void LoadChunk()
    {
        string filename = "ExampleJson";
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (!File.Exists(path))
        {
            Debug.LogWarning("Chunk file not found!");
            return;
        }

        string json = File.ReadAllText(path);
        Chunk chunk = JsonUtility.FromJson<Chunk>(json);

        foreach (Transform child in chunkToLoad)
            Destroy(child.gameObject);

        foreach (ObjectData data in chunk.objects)
        {
            GameObject obj = Instantiate(GetPrefab(data.name), data.position, data.rotation, chunkToLoad);
            obj.name = data.name;
            obj.transform.localScale = data.size;
        }

        Debug.Log($"Loaded {chunk.objects.Count} objects from {filename}");
    }

    public GameObject GetPrefab(string name)
    {
        switch (name)
        {
            case "Tree":
                return treePrefab;
            case "Bush":
                return bushPrefab;
            case "House":
                return housePrefab;
            default:
                return null;
        }
    }
}
