using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    private bool isInitialized = false;

    private void Start()
    {
        // Ensure initial node is loaded when StoryData is available
        Invoke(nameof(LoadInitialNode), 3f); // Delay to ensure StoryData is loaded
    }

    private void LoadInitialNode()
    {
        if (StoryLoader.Instance != null && StoryLoader.Instance.StoryData != null)
        {
            Debug.Log("Loading initial node...");
            LoadNode("1"); // Replace "1" with your desired starting node ID
            isInitialized = true;
        }
        else
        {
            Debug.LogError("StoryLoader or StoryData is not initialized yet.");
        }
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
                Debug.Log($"Button {i}: {ChoiceTexts[i].text}");

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

        // Calculate the next node ID by appending the choice index to the current node ID
        string nextNodeId = CurrentStoryNode.CurrentNodeId + (choiceIndex + 1).ToString();

        Debug.Log($"Next Node ID: {nextNodeId}");
        LoadNode(nextNodeId);
    }

}
