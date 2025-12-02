using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;
using QFSW;
using QFSW.QC;
using VInspector;
public class ChunkManager : MonoBehaviour
{
    [SerializeField] public static ChunkManager Instance;
    [SerializeField] private Transform llmChunk;
    [SerializeField] private Transform[] chunks;
    [SerializeField] private GameObject treePrefab, bushPrefab, housePrefab; // a generic prefab to represent loaded objects

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this);
    }

    [Button("Save Chunk", color = "green")]
    public void _SaveChunk()
    {
        JsonFileSaver("chunk0", SaveChunk(llmChunk));
    }

    [Button("Load Chunk", color = "blue")]
    public void _LoadChunk()
    {
        LoadChunk("LLM_Chunk");
    }

    [Button("Save All Chunk", color = "purple")]
    public void _SaveAllChunks()
    {
        SaveAllChunks(chunks);
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

    [Command]
    public void LoadChunk(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath + "\\chunks", filename);
        if (!File.Exists(path))
        {
            Debug.LogWarning("Chunk file not found!");
            return;
        }

        string json = File.ReadAllText(path);
        Chunk chunk = JsonUtility.FromJson<Chunk>(json);

        foreach (Transform child in this.llmChunk)
            Destroy(child.gameObject);

        foreach (ObjectData data in chunk.objects)
        {
            GameObject obj = Instantiate(GetPrefab(data.name), this.llmChunk.position + data.position, data.rotation, this.llmChunk);
            obj.name = data.name;
            obj.transform.localScale = data.size;
        }

        Debug.Log($"Loaded {chunk.objects.Count} objects from {filename}");
    }

    [Command]
    public string SaveChunk(Transform saveChunk)
    {
        Chunk chunk = new Chunk();
        foreach (Transform child in saveChunk)
        {
            String newName ="";
            switch (child.name)
            {
                case string a when a.Contains("Tree"):
                  newName = "Tree";
                    break;
                case string a when a.Contains("Bush"):
                  newName = "Bush";
                    break;  
                case string a when a.Contains("House"):
                    newName = "House";
                    break;
                default:
                    Debug.LogWarning($"Unsupported object type: {child.name}. Skipping.");
                    continue;
            }
            ObjectData data = new ObjectData
            {
                name = newName,
                position = child.position,
                rotation = child.rotation,
                size = child.localScale
            };

            chunk.objects.Add(data);
        }
        string json = JsonUtility.ToJson(chunk, true);
        return json;
    }

    public void SaveAllChunks(Transform[] chunks)
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            JsonFileSaver("chunk" + i, SaveChunk(chunks[i]));
        }
    }
    public void JsonFileSaver(string filename, string json)
    {
        string path = Path.Combine(Application.streamingAssetsPath + "\\chunks", filename);
        File.WriteAllText(path, json);
    }
}

[Serializable]
public class Chunk
{
    public List<ObjectData> objects = new();
}

[Serializable]
public class ObjectData
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 size;
}