using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OpenAI.Samples.Chat;
using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;

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
    public Image BackgroundImage; // Background UI image
    public GameObject StoryUI; // Story UI GameObject
    public GameObject LoadingScreen; // Loading screen GameObject
    public TextMeshProUGUI LoadingText; // Loading text (optional)

    private ChatBehaviour chatBehaviour; // Reference to ChatBehaviour
    private Coroutine loadingAnimationCoroutine; // Reference to the loading animation coroutine

    private FirebaseFirestore db; // Firestore instance for uploading invalid nodes

    private void Start()
    {
        // Find ChatBehaviour in the scene
        chatBehaviour = FindObjectOfType<ChatBehaviour>();
        if (chatBehaviour == null)
        {
            Debug.LogError("ChatBehaviour not found in the scene.");
        }

        // Initialize Firestore
        db = FirebaseFirestore.DefaultInstance;

        // Disable the StoryUI and LoadingScreen at the start
        if (StoryUI != null)
        {
            StoryUI.SetActive(false);
        }
        if (LoadingScreen != null)
        {
            LoadingScreen.SetActive(false);
        }
    }

    public void StartStoryFromSelection()
    {
        Debug.Log("Starting story from the initial node...");

        // Log all available node IDs
        foreach (var key in StoryLoader.Instance.StoryData.StoryNodes.Keys)
        {
            Debug.Log($"Available node ID: {key}");
        }

        LoadNode("1"); // Replace "1" with your actual starting node ID
    }

    public void LoadNode(string nodeId)
    {
        Debug.Log($"Loading Node ID: {nodeId}");
        if (!StoryLoader.Instance.StoryData.StoryNodes.TryGetValue(nodeId, out var node))
        {
            Debug.LogError($"Story node with ID {nodeId} not found!");

            // Upload to Firestore with missing node details
            UploadInvalidNodeToUnityCollection(nodeId, "Node not found", "Unknown choice text");
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

        // Show the loading screen while generating the background
        if (LoadingScreen != null)
        {
            LoadingScreen.SetActive(true);
            if (LoadingText != null)
            {
                // Start the loading animation
                if (loadingAnimationCoroutine != null)
                {
                    StopCoroutine(loadingAnimationCoroutine);
                }
                loadingAnimationCoroutine = StartCoroutine(LoadingAnimation());
            }
        }

        // Disable the StoryUI while the background image is being generated
        if (StoryUI != null)
        {
            StoryUI.SetActive(false);
        }

        // Generate a new background image based on the node's text
        GenerateBackground(CurrentStoryNode.NodeText);
    }
    
    private Coroutine typewriterCoroutine;

    private IEnumerator TypewriterEffect(string fullText, TextMeshProUGUI textMesh, System.Action onComplete, float typingSpeed = 0.01f)
    {
        textMesh.text = ""; // Clear the text box
        foreach (char c in fullText)
        {
            textMesh.text += c; // Add one character at a time
            yield return new WaitForSeconds(typingSpeed); // Wait before adding the next character
        }
        onComplete?.Invoke(); // Notify that the typewriter effect is complete
    }
    
    private IEnumerator FadeInPanelAndText(GameObject panel, TextMeshProUGUI textMesh, float duration = 0.5f)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0;
        panel.SetActive(true);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1; // Ensure it's fully visible
    }



    
    private IEnumerator FadeInButton(Button button, TextMeshProUGUI buttonText, float duration = 0.5f)
    {
        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0;
        button.gameObject.SetActive(true);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1; // Ensure the button is fully visible
    }


    private void UpdateUI()
    {
        if (CurrentStoryNode == null)
        {
            Debug.LogError("CurrentStoryNode is null. UI cannot be updated.");
            return;
        }

        Debug.Log($"Updating UI with Node Text: {CurrentStoryNode.NodeText}");

        // Stop any ongoing typewriter effect
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        // Start the typewriter effect for the plot text
        typewriterCoroutine = StartCoroutine(TypewriterEffect(CurrentStoryNode.NodeText, PlotText, () =>
        {
            // Fade in each choice button one by one after typewriter finishes
            StartCoroutine(FadeInChoices());
        }));
    }

    private IEnumerator FadeInChoices()
    {
        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            if (i < CurrentStoryNode.Choices.Length)
            {
                ChoiceTexts[i].text = CurrentStoryNode.Choices[i];
                ChoiceButtons[i].onClick.RemoveAllListeners();
                int choiceIndex = i; // Local variable for closure
                ChoiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));

                // Get the panel for this choice button
                GameObject panel = ChoiceButtons[i].transform.parent.gameObject;

                // Fade in the panel and text
                yield return FadeInPanelAndText(panel, ChoiceTexts[i]);
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
        string selectedChoiceText = CurrentStoryNode.Choices[choiceIndex];

        Debug.Log($"Next Node ID: {nextNodeId}, Selected Choice Text: {selectedChoiceText}");

        // If the next node is not found, upload the current choice text
        if (!StoryLoader.Instance.StoryData.StoryNodes.ContainsKey(nextNodeId))
        {
            Debug.LogWarning($"Node ID {choiceIndex} not found. Uploading missing choice to Firestore.");
            UploadInvalidNodeToUnityCollection((choiceIndex + 1).ToString(), CurrentStoryNode.NodeText, selectedChoiceText);
            return;
        }

        LoadNode(nextNodeId);
    }

    private void GenerateBackground(string plotText)
    {
        if (chatBehaviour == null)
        {
            Debug.LogError("ChatBehaviour is not set.");
            return;
        }

        chatBehaviour.GenerateBackgroundImage(plotText, texture =>
        {
            if (texture != null)
            {
                // Apply the generated texture to the BackgroundImage UI element
                BackgroundImage.sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
                Debug.Log("Background image updated successfully.");
            }
            else
            {
                Debug.LogError("Failed to generate background image.");
            }

            // Hide the loading screen and reveal the StoryUI
            if (LoadingScreen != null)
            {
                LoadingScreen.SetActive(false);
            }
            if (StoryUI != null)
            {
                StoryUI.SetActive(true);
                UpdateUI(); // Refresh the UI after reactivating StoryUI
            }

            // Stop the loading animation
            if (loadingAnimationCoroutine != null)
            {
                StopCoroutine(loadingAnimationCoroutine);
                loadingAnimationCoroutine = null;
            }
        });
    }

    private IEnumerator LoadingAnimation()
    {
        string baseText = "Loading";
        int dotCount = 0;

        while (true)
        {
            LoadingText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4; // Cycle through 0, 1, 2, 3 dots
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UploadInvalidNodeToUnityCollection(string nodeId, string nodeText, string choiceText)
    {
        Debug.Log($"Uploading invalid node to Unity collection: Node ID = {nodeId}, Node Text = {nodeText}, Choice Text = {choiceText}");

        var invalidNodeData = new Dictionary<string, object>
        {
            { "type", "choice" },
            { "choiceId", nodeId },
            { "content", choiceText }
        };

        string documentName = System.Guid.NewGuid().ToString(); // Generate a UUID for the document

        db.Collection("services").Document("tasks").Collection("unity")
            .Document(documentName)
            .SetAsync(invalidNodeData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log($"Invalid node uploaded successfully with UUID: {documentName}");
                }
                else
                {
                    Debug.LogError($"Failed to upload invalid node: {task.Exception}");
                }
            });
    }
}
