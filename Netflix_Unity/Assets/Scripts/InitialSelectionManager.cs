using System.Collections.Generic;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using FirebaseNetworkKit;

public class InitialSelectionManager : MonoBehaviour
{
    public GameObject InitialSelectionScreen; // Reference to the UI screen for initial selection
    public Button LeagueOfLegendsButton; // Button for "League of Legends"
    public Button DraculaButton; // Button for "Dracula"

    private FirebaseFirestore db;

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
        
        // Upload the selection to Firestore
        UploadSelectionToFirestore(selection);

        // Hide the initial selection screen
        InitialSelectionScreen.SetActive(false);

        // Notify PythonListener to start processing JSON
        FindObjectOfType<PythonListener>().FetchJsonFromStorage(selection);
    }

    private void UploadSelectionToFirestore(string selection)
    {
        // Create a document with the selection
        var documentData = new Dictionary<string, object>
        {
            { "type", "initialStory" },
            { "content", selection }
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
}
