using System.Linq; // Required for LINQ methods
using UnityEngine;

public class StoryLoader : MonoBehaviour
{
    public TextAsset jsonFile; // Assign your JSON file in the Unity Inspector
    public StoryData storyData; // Holds the parsed story data
    public NodeManager nodeManager; // Reference to the NodeManager script

    void LoadStoryData()
    {
        if (jsonFile == null)
        {
            Debug.LogError("JSON file not assigned. Please assign a valid JSON file in the Inspector.");
            return;
        }

        storyData = JsonUtility.FromJson<StoryData>(jsonFile.text);

        if (storyData == null || storyData.storyNodes.Length == 0)
        {
            Debug.LogError("Story data is empty or invalid. Check the JSON format.");
            return;
        }

        Debug.Log($"Story loaded. Total Nodes: {storyData.storyNodes.Length}");

        if (nodeManager != null)
        {
            // Convert StoryData.StoryNode[] to NodeManager.StoryNode[]
            NodeManager.StoryNode[] convertedNodes = storyData.storyNodes.Select(node => new NodeManager.StoryNode
            {
                nodeText = node.nodeText,
                choices = node.choices,
                backgroundImage = node.backgroundImage,
                nextNodeIndices = node.nextNodeIndices
            }).ToArray();

            nodeManager.InitializeStoryNodes(convertedNodes);
        }
        else
        {
            Debug.LogError("NodeManager is not assigned.");
        }
    }
}