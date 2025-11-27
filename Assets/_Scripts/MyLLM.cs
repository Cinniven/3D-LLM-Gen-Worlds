using UnityEngine;
using LLMUnity;
using QFSW.QC;
using System.Collections.Generic;
using System.IO;
using VInspector;
using System;

public class MyLLM : MonoBehaviour
{
    public LLMCharacter llmCharacter;
    public string replyString;
    public string promtToSend;

    [Command]
    async void Chat(string promt)
    {
        replyString = await llmCharacter.Chat(promt);
    }

    [Button("Generate LLM Chunk", color = "blue")]
    async public void GenerateChunk()
    {
        float time = 0;
        string promt = "";
        for (int i = 0; i < 8; i++)
        {
            string path = Path.Combine(Application.streamingAssetsPath + "\\chunks", "chunk" + i);
            if (!File.Exists(path))
            {
                Debug.LogWarning("Chunk file not found!");
                return;
            }

            string json = File.ReadAllText(path);
            promt += json + "\n";
        }

        promtToSend = promt;

        replyString = await llmCharacter.Chat(promt);

        time = Time.realtimeSinceStartup;
        Debug.Log(time);

        string path2 = Path.Combine(Application.streamingAssetsPath + "\\chunks", "LLM_Chunk");
        File.WriteAllText(path2, replyString);
        ChunkManager.Instance.LoadChunk("LLM_Chunk");
        Debug.Log("Done!");
    }

    [Button("Cancel Request", color = "red")]
    void CancelRequest()
    {
        llmCharacter.CancelRequests();
    }

    void HandleReply(string reply)
    {

    }

    void ReplyCompleted()
    {
        /*
        string path = Path.Combine(Application.streamingAssetsPath + "/chunks", "LLM_Chunk");
        File.WriteAllText(path, replyString);
        ChunkManager.Instance.LoadChunk("LLM_Chunk");
        */
    }
}