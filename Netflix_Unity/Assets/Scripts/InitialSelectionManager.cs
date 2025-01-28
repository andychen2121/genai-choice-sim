using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using FirebaseNetworkKit;

public class InitialSelectionManager : MonoBehaviour
{
    public GameObject InitialSelectionScreen; // Reference to the UI screen for initial selection
    public GameObject LoadingScreen; // Reference to the loading screen UI
    public Button LeagueOfLegendsButton; // Button for "League of Legends"
    public Button DraculaButton; // Button for "Dracula"
    public Text LoadingText; // Optional: Text on the loading screen for animation

    private FirebaseFirestore db;

    public GameObject CoroutineHandler; // Reference to the CoroutineHandler GameObject

    private Coroutine loadingAnimationCoroutine;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        // Assign button listeners
        LeagueOfLegendsButton.onClick.AddListener(() => OnSelectionMade("League of Legends"));
        DraculaButton.onClick.AddListener(() => OnSelectionMade("Dracula"));
    }

    private void OnSelectionMade(string selection)
{
    Debug.Log($"User selected: {selection}");

    if (CoroutineHandler == null)
    {
        Debug.LogError("CoroutineHandler is not assigned or active.");
        return;
    }

    if (LoadingScreen == null)
    {
        Debug.LogError("LoadingScreen is not assigned.");
    }

    CoroutineHandler.GetComponent<CoroutineHandler>().StartCoroutine(ProcessSelectionWithLoadingScreen(selection));
}

private IEnumerator ProcessSelectionWithLoadingScreen(string selection)
{
    Debug.Log($"Processing selection: {selection}");

    // Show the loading screen
    if (LoadingScreen != null)
    {
        Debug.Log("Activating LoadingScreen...");
        LoadingScreen.SetActive(true);

        // Start the loading animation
        if (LoadingText != null)
        {
            Debug.Log("Starting LoadingAnimation...");
            loadingAnimationCoroutine = CoroutineHandler.GetComponent<CoroutineHandler>().StartCoroutine(LoadingAnimation());
        }
    }
    else
    {
        Debug.LogWarning("LoadingScreen is null.");
    }

    // Delay for 5 seconds
    Debug.Log("Waiting for 5 seconds...");
    yield return new WaitForSeconds(0.5f);

    // Upload the selection to Firestore
    UploadSelectionToFirestore(selection);

    // Notify PythonListener to start processing JSON
    var pythonListener = FindObjectOfType<PythonListener>();
    if (pythonListener != null)
    {
        Debug.Log("Notifying PythonListener to fetch JSON...");
        pythonListener.FetchJsonFromStorage(selection);
    }
    else
    {
        Debug.LogError("PythonListener not found in the scene.");
    }

    // Hide the loading screen
    if (LoadingScreen != null)
    {
        Debug.Log("Deactivating LoadingScreen...");
        LoadingScreen.SetActive(false);
    }

    // Stop the loading animation
    if (loadingAnimationCoroutine != null)
    {
        Debug.Log("Stopping LoadingAnimation...");
        CoroutineHandler.GetComponent<CoroutineHandler>().StopCoroutine(loadingAnimationCoroutine);
        loadingAnimationCoroutine = null;
    }

    // Hide the initial selection screen
    if (InitialSelectionScreen != null)
    {
        Debug.Log("Deactivating InitialSelectionScreen...");
        InitialSelectionScreen.SetActive(false);
    }
}


    private void UploadSelectionToFirestore(string selection)
    {
        // Create a document with the selection
        var documentData = new Dictionary<string, object>
        {
            { "content", selection },
            { "type", "initialStory" }
        };

        // Generate a unique ID for the document
        string uniqueId = System.Guid.NewGuid().ToString();

        db.Collection("services").Document("tasks").Collection("unity").Document(uniqueId)
            .SetAsync(documentData).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"Initial selection '{selection}' uploaded to Firestore with ID: {uniqueId}");
                }
                else
                {
                    Debug.LogError($"Failed to upload selection: {task.Exception}");
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
}
