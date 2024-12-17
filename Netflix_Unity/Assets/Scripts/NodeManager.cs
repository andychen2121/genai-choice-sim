using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using FirebaseNetworkKit;

public class NodeManager : MonoBehaviour
{
    [System.Serializable]
    public class StoryNode
    {
        public string NodeText;
        public string[] Choices;
        public string CurrentNodeId;
    }

    public StoryNode CurrentStoryNode;

    public TextMeshProUGUI PlotText; // Story text display
    public Button[] ChoiceButtons;  // Buttons for choices
    public TextMeshProUGUI[] ChoiceTexts; // Text on choice buttons
    public GameObject StoryUI; // Reference to the StoryUI GameObject

    private PythonListener pythonListener; // Reference to PythonListener

    private void Start()
    {
        // Disable StoryUI at the start
        if (StoryUI != null)
        {
            StoryUI.SetActive(false);
        }

        // Find the PythonListener GameObject
        pythonListener = GameObject.FindObjectOfType<PythonListener>();

        if (pythonListener == null)
        {
            Debug.LogError("PythonListener GameObject not found in the scene.");
        }
    }

    public void StartStoryFromSelection()
    {
        Debug.Log("Starting story from initial selection...");

        // Enable StoryUI when starting the story
        if (StoryUI != null)
        {
            StoryUI.SetActive(true);
        }

        LoadNode("1"); // Assuming the first node ID is "1"
    }
    
    

    public void LoadNode(string nodeId)
    {
        Debug.Log($"Loading Node ID: {nodeId}");
        if (!StoryLoader.Instance.StoryData.StoryNodes.TryGetValue(nodeId, out var node))
        {
            Debug.LogError($"Story node with ID {nodeId} not found!");
            return;
        }

        CurrentStoryNode = new StoryNode
        {
            NodeText = node.StoryContinuation,
            Choices = new string[node.Choices.Count],
            CurrentNodeId = nodeId
        };

        for (int i = 0; i < node.Choices.Count; i++)
        {
            CurrentStoryNode.Choices[i] = node.Choices[i].Action;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (CurrentStoryNode == null)
        {
            Debug.LogError("CurrentStoryNode is null. UI cannot be updated.");
            return;
        }

        Debug.Log($"Updating UI with Node Text: {CurrentStoryNode.NodeText}");
        Debug.Log($"Choices Count: {CurrentStoryNode.Choices.Length}");

        PlotText.text = CurrentStoryNode.NodeText;

        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            if (i < CurrentStoryNode.Choices.Length)
            {
                ChoiceButtons[i].gameObject.SetActive(true);
                ChoiceTexts[i].text = CurrentStoryNode.Choices[i];

                int choiceIndex = i; // Local variable for closure
                ChoiceButtons[i].onClick.RemoveAllListeners();
                ChoiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
            }
            else
            {
                ChoiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= CurrentStoryNode.Choices.Length)
        {
            Debug.LogError($"Invalid choice index: {choiceIndex}");
            return;
        }

        // Determine the next node ID
        string nextNodeId = CurrentStoryNode.CurrentNodeId + (choiceIndex + 1).ToString();
        string choiceContent = CurrentStoryNode.Choices[choiceIndex];
        Debug.Log($"Next Node ID: {nextNodeId}, Choice Content: {choiceContent}");

        // Check if the next node exists
        if (!StoryLoader.Instance.StoryData.StoryNodes.ContainsKey(nextNodeId))
        {
            Debug.LogWarning($"Node ID {nextNodeId} does not exist. Uploading invalid choice data to Firestore.");

            // Find the PythonListener and call the upload method
            PythonListener pythonListener = FindObjectOfType<PythonListener>();
            if (pythonListener != null)
            {
                pythonListener.UploadInvalidChoiceToUnityCollection(nextNodeId, choiceIndex, choiceContent);
            }
            else
            {
                Debug.LogError("PythonListener not found. Cannot upload invalid choice data.");
            }
            return;
        }

        // Load the next node if it exists
        LoadNode(nextNodeId);
    }


}
