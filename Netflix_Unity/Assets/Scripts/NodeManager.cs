using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NodeManager : MonoBehaviour
{
    [System.Serializable]
    public class StoryNode
    {
        public string nodeText;
        public string[] choices;
        public string backgroundImage;
        public int[] nextNodeIndices;
    }

    public TextMeshProUGUI plotText;
    public Button[] choiceButtons;
    public TextMeshProUGUI[] choiceTexts;
    public Image backgroundImageUI;

    private StoryNode[] storyNodes;
    private int currentNodeIndex = 0;

    public void InitializeStoryNodes(StoryNode[] loadedNodes)
    {
        if (loadedNodes == null || loadedNodes.Length == 0)
        {
            Debug.LogError("Loaded story nodes array is empty or null.");
            return;
        }

        storyNodes = loadedNodes;
        currentNodeIndex = 0;
        LoadNode(currentNodeIndex);
    }

    private void LoadNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= storyNodes.Length)
        {
            Debug.LogError($"Invalid node index: {nodeIndex}.");
            return;
        }

        StoryNode currentNode = storyNodes[nodeIndex];
        plotText.text = currentNode.nodeText;

        if (backgroundImageUI != null)
        {
            Sprite newBackground = Resources.Load<Sprite>(currentNode.backgroundImage);
            if (newBackground != null)
            {
                backgroundImageUI.sprite = newBackground;
            }
        }

        for (int i = 0; i < choiceTexts.Length; i++)
        {
            if (i < currentNode.choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceTexts[i].text = currentNode.choices[i];
                int choiceIndex = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
                choiceTexts[i].text = "";
            }
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= storyNodes[currentNodeIndex].nextNodeIndices.Length)
        {
            Debug.LogError($"Invalid choice index: {choiceIndex}.");
            return;
        }

        currentNodeIndex = storyNodes[currentNodeIndex].nextNodeIndices[choiceIndex];
        LoadNode(currentNodeIndex);
    }
}
