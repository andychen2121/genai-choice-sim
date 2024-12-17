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
        try
        {
            // Preprocess JSON to replace empty keys
            if (jsonContent.Contains("\"\":"))
            {
                jsonContent = jsonContent.Replace("\"\":", "\"root\":");
                Debug.Log("Replaced empty keys with 'root'.");
            }

            // Deserialize the JSON into a dictionary for initial parsing
            var rawData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);
            if (rawData == null || !rawData.ContainsKey("root"))
            {
                Debug.LogError("Invalid JSON structure: Missing 'root' key.");
                return;
            }

            // Deserialize 'root' separately into StoryData.Root
            StoryData = new StoryData
            {
                Root = JsonConvert.DeserializeObject<RootData>(JsonConvert.SerializeObject(rawData["root"]))
            };

            // Deserialize story nodes into StoryNodes
            foreach (var key in rawData.Keys)
            {
                if (key == "root") continue; // Skip the root key

                if (int.TryParse(key, out _)) // Node keys are integers in string format
                {
                    var nodeJson = JsonConvert.SerializeObject(rawData[key]);
                    var node = JsonConvert.DeserializeObject<StoryNode>(nodeJson);
                    StoryData.StoryNodes[key] = node;
                }
            }

            Debug.Log("Story data loaded successfully!");
            Debug.Log($"Plot Summary: {StoryData.Root.PlotSummary}");

            // Debug log each story node
            foreach (var node in StoryData.StoryNodes)
            {
                Debug.Log($"Node ID: {node.Key}");
                Debug.Log($"Story Continuation: {node.Value.StoryContinuation}");

                foreach (var choice in node.Value.Choices)
                {
                    Debug.Log($"Choice {choice.ChoiceId}: {choice.Action}");
                }
            }
        }
        catch (JsonException ex)
        {
            Debug.LogError($"JSON parsing error: {ex.Message}");
        }
    }
}
