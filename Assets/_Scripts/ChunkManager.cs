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

    public string folderName = "marcChunks 11";
    public int foldercounter =16;
    public Transform worldPivot;
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
        LoadChunk("chunk8");
    }

    [Button("Save All Chunk", color = "purple")]
    public void _SaveAllChunks()
    {
        SaveAllChunks(chunks);
    }

    [Button("Fix All Chunks Y Position", color = "red")]
    public void _FixAllChunksYPosition()
    {
        FixAllChunksYPosition();
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
        string path = Path.Combine(Application.streamingAssetsPath + "\\marcChunks 44", filename);
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
            String newName = "";
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
                position = child.localPosition,
                rotation = child.localRotation,
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
        foldercounter++;
    }
    public void JsonFileSaver(string filename, string json)
    {
        string roundedJson = System.Text.RegularExpressions.Regex.Replace(
        json,
        @"-?\d+(\.\d+)?",
        match =>
        {
            if (float.TryParse(match.Value, out float f))
            {
                return MathF.Round(f, 2).ToString("0.##");
            }
            return match.Value;
        }
    );

        string path = Path.Combine(Application.streamingAssetsPath + "\\" + folderName + " " + foldercounter, filename);
        File.WriteAllText(path, roundedJson);
        ScreenCapture.CaptureScreenshot(Path.Combine(Application.streamingAssetsPath + "\\" + folderName + " " + foldercounter, filename + ".png"));
    }

[Button("Rotate All Chunk", color = "yellow")]
 public void RotateAllChunks()
 {
    List<(Transform obj, Quaternion originalRotation, Collider[] colliders)> _Allobjects = new();
    
    // First, collect all objects with their original rotations and colliders
    foreach (var chunk in chunks)
    {
        foreach (Transform child in chunk)
        {
            Collider[] colliders = child.GetComponentsInChildren<Collider>();
            _Allobjects.Add((child, child.rotation, colliders));
        }
    }

    // Parent to worldPivot and rotate
    foreach (var (obj, originalRot, colliders) in _Allobjects)
    {
        obj.parent = worldPivot;
    }
    
    // Rotate all objects at once around the world pivot
    worldPivot.Rotate(0, 90, 0);

    // Restore original rotations and use bounds to reassign parents
    foreach (var (obj, originalRot, colliders) in _Allobjects)
    {
        // Restore the original world rotation
        obj.rotation = Quaternion.Euler(0, originalRot.eulerAngles.y + 90, 0);
        
        obj.parent = null; // Unparent first
        
        // Find the closest chunk by checking which chunk's bounds contains this object
        Transform closestChunk = null;
        float closestDistance = float.MaxValue;
        
        foreach (var chunk in chunks)
        {
            // Get the chunk's renderer to check bounds
            Renderer chunkRenderer = chunk.GetComponent<Renderer>();
            if (chunkRenderer != null)
            {
                // Check if object's position is within chunk's bounds
                if (chunkRenderer.bounds.Contains(obj.position))
                {
                    closestChunk = chunk;
                    break;
                }
                
                // If not contained, find the closest chunk
                float distance = Vector3.Distance(obj.position, chunk.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestChunk = chunk;
                }
            }
        }
        
        if (closestChunk != null)
        {
            obj.parent = closestChunk;
            Debug.Log($"{obj.name} assigned to {closestChunk.name}");
        }
        else
        {
            Debug.LogWarning($"{obj.name} couldn't find a parent chunk! Position: {obj.position}");
        }
    }
    
    // Reset world pivot rotation
    worldPivot.rotation = Quaternion.identity;

 }

    public void FixAllChunksYPosition()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        string[] allDirectories = Directory.GetDirectories(streamingAssetsPath);
        
        int totalFixed = 0;
        int totalObjects = 0;
        
        foreach (string dir in allDirectories)
        {
            string dirName = Path.GetFileName(dir);
            if (dirName.StartsWith("marcChunks") || dirName == "chunks")
            {
                Debug.Log($"Processing folder: {dirName}");
                
                for (int i = 0; i <= 8; i++)
                {
                    string chunkPath = Path.Combine(dir, $"chunk{i}");
                    if (File.Exists(chunkPath))
                    {
                        try
                        {
                            string json = File.ReadAllText(chunkPath);
                            Chunk chunk = JsonUtility.FromJson<Chunk>(json);
                            
                            foreach (ObjectData obj in chunk.objects)
                            {
                                // Set y position to half of y scale
                                obj.position = new Vector3(
                                    obj.position.x,
                                    obj.size.y / 2f,
                                    obj.position.z
                                );
                                totalObjects++;
                            }
                            
                            // Write back with rounded formatting
                            string newJson = JsonUtility.ToJson(chunk, true);
                            string roundedJson = System.Text.RegularExpressions.Regex.Replace(
                                newJson,
                                @"-?\d+(\.\d+)?",
                                match =>
                                {
                                    if (float.TryParse(match.Value, out float f))
                                    {
                                        return MathF.Round(f, 2).ToString("0.##");
                                    }
                                    return match.Value;
                                }
                            );
                            File.WriteAllText(chunkPath, roundedJson);
                            totalFixed++;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error processing {chunkPath}: {ex.Message}");
                        }
                    }
                }
            }
        }
        
        Debug.Log($"Fixed {totalObjects} objects across {totalFixed} chunk files!");
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