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

    public void LoadNode(string nodeId)
    {
        if (!StoryLoader.Instance.StoryData.StoryNodes.TryGetValue(nodeId, out var node))
        {
            Debug.LogError($"Story node with ID {nodeId} not found!");
            return;
        }

        Debug.Log($"Loading Node ID: {nodeId}");

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
        if (CurrentStoryNode == null) return;

        Debug.Log($"Updating UI for Node ID: {CurrentStoryNode.CurrentNodeId}");

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

        string nextNodeId = CurrentStoryNode.CurrentNodeId + (choiceIndex + 1).ToString();
        Debug.Log($"Next Node ID: {nextNodeId}");

        LoadNode(nextNodeId);
    }
}
