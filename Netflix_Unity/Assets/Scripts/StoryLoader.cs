using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

public class StoryLoader : MonoBehaviour
{
    public static StoryLoader Instance { get; private set; }

    public StoryData StoryData { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadStoryData(string jsonContent)
    {
        Debug.Log("Starting to load story data...");

        try
        {
            // Preprocess JSON to replace empty keys
            if (jsonContent.Contains("\"\":"))
            {
                jsonContent = jsonContent.Replace("\"\":", "\"root\":");
                Debug.Log("Replaced empty keys with 'root'.");
            }

            // Deserialize the JSON into a dictionary for initial parsing
            Debug.Log("Deserializing JSON into raw data...");
            var rawData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);
            if (rawData == null)
            {
                Debug.LogError("Deserialization failed: rawData is null.");
                return;
            }

            Debug.Log("Deserialization successful.");

            if (!rawData.ContainsKey("root"))
            {
                Debug.LogError("Invalid JSON structure: Missing 'root' key.");
                return;
            }

            Debug.Log("Found 'root' key.");

            // Deserialize 'root' separately into StoryData.Root
            StoryData = new StoryData
            {
                Root = JsonConvert.DeserializeObject<RootData>(JsonConvert.SerializeObject(rawData["root"]))
            };
            Debug.Log($"Root data loaded: {StoryData.Root.PlotSummary}");

            // Deserialize story nodes into StoryNodes
            foreach (var key in rawData.Keys)
            {
                Debug.Log($"Processing key: {key}");

                if (key == "root") continue; // Skip the root key

                if (int.TryParse(key, out _)) // Node keys are integers in string format
                {
                    var nodeJson = JsonConvert.SerializeObject(rawData[key]);
                    var node = JsonConvert.DeserializeObject<StoryNode>(nodeJson);
                    StoryData.StoryNodes[key] = node;

                    Debug.Log($"Loaded node ID: {key}, Story Continuation: {node.StoryContinuation}");
                }
                else
                {
                    Debug.LogWarning($"Skipped non-integer key: {key}");
                }
            }

            Debug.Log("Story data loaded successfully!");
        }
        catch (JsonException ex)
        {
            Debug.LogError($"JSON parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error in LoadStoryData: {ex.Message}");
        }
    }
}
