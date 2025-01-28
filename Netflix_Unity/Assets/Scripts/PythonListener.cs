using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Storage;

namespace FirebaseNetworkKit
{
    public class PythonListener : MonoBehaviour
    {
        public static PythonListener Instance { get; private set; } // Singleton instance

        private FirebaseFirestore db;
        private FirebaseStorage storage;
        private StorageReference storageRef;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Persist this object across scenes
            }
            else
            {
                Destroy(gameObject); // Destroy duplicates
            }
        }

        void Start()
        {
            // Initialize Firestore
            db = FirebaseFirestore.DefaultInstance;

            // Initialize Firebase Storage
            storage = FirebaseStorage.DefaultInstance;
            storageRef = storage.GetReferenceFromUrl("gs://netflixcyoa.firebasestorage.app");

            // Listen for the latest ID
            ListenForMostRecentPythonTask();
        }

       public void FetchJsonFromStorage(string selection)
{
    Debug.Log($"Fetching JSON for selected story: {selection}");

    // Check if the selection is of type "json" before fetching
    CollectionReference pythonCollection = db.Collection("services").Document("tasks").Collection("python");

    pythonCollection.Document(selection).GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompletedSuccessfully)
        {
            var documentSnapshot = task.Result;

            if (documentSnapshot.Exists && documentSnapshot.GetValue<string>("type") == "json")
            {
                string jsonFileName = $"{selection}.json";
                Debug.Log($"Generated JSON file name: {jsonFileName}");
                StorageReference jsonFileRef = storageRef.Child(jsonFileName);

                string localPath = Path.Combine(Application.persistentDataPath, jsonFileName);
                jsonFileRef.GetFileAsync(localPath).ContinueWithOnMainThread(fileTask =>
                {
                    if (fileTask.IsCompletedSuccessfully)
                    {
                        Debug.Log($"JSON file downloaded to: {localPath}");

                        // Read the JSON file content
                        string jsonContent = File.ReadAllText(localPath);

                        // Debug log the contents of the JSON file
                        Debug.Log($"JSON Content: {jsonContent}");

                        // Send JSON content to StoryLoader
                        StoryLoader.Instance.LoadStoryData(jsonContent);

                        Debug.Log("sel story");
                        // Notify NodeManager to start the story
                        FindObjectOfType<NodeManager>().StartStoryFromSelection();
                    }
                    else
                    {
                        Debug.LogError($"Failed to download JSON file: {fileTask.Exception}");
                    }
                });
            }
            else
            {
                Debug.LogWarning($"Document {selection} is not of type 'json'. Skipping file load.");
            }
        }
        else
        {
            Debug.LogError($"Failed to fetch document {selection} metadata: {task.Exception}");
        }
    });
}

        
       
public void UploadInvalidChoiceToUnityCollection(string nodeId, int choiceIndex, string choiceContent)
{
    Debug.Log($"Uploading invalid choice data to Firestore (unity collection). Node ID: {nodeId}, Choice Index: {choiceIndex}, Content: {choiceContent}");

    // Reference the unity collection
    CollectionReference unityCollection = db.Collection("services").Document("tasks").Collection("unity");

    // Prepare the data for the new document
    var invalidChoiceData = new Dictionary<string, object>
    {
        { "type", "choice" },
        { "choiceId", nodeId },
        { "content", choiceContent }
    };

    // Generate a UUID for the new document name
    string documentName = System.Guid.NewGuid().ToString();

    // Add the document to Firestore
    unityCollection.Document(documentName).SetAsync(invalidChoiceData).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompletedSuccessfully)
        {
            Debug.Log($"Invalid choice data uploaded to Firestore successfully. Document Name: {documentName}");
        }
        else
        {
            Debug.LogError($"Failed to upload invalid choice data to Firestore: {task.Exception}");
        }
    });
}

        
        public void UploadChoiceAndLoadJson(string choiceId, string choiceContent)
        {
            Debug.Log($"Uploading selected choice to Firestore and loading its JSON. Choice ID: {choiceId}, Content: {choiceContent}");
    
            // Reference the Python collection
            CollectionReference pythonCollection = db.Collection("services").Document("tasks").Collection("python");

            // Prepare the data for the new document
            var choiceData = new Dictionary<string, object>
            {
                { "id", choiceId },
                { "type", "choice" },
                { "content", choiceContent }
            };

            // Generate a UUID for the new document name
            string documentName = System.Guid.NewGuid().ToString();

            // Add the document to Firestore
            pythonCollection.Document(documentName).SetAsync(choiceData).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log($"Choice uploaded to Firestore successfully. Document Name: {documentName}");

                    // After upload, fetch the JSON file for the choice content
                    FetchJsonFromStorage(choiceContent);
                }
                else
                {
                    Debug.LogError($"Failed to upload choice to Firestore: {task.Exception}");
                }
            });
        }

        
        public void UploadChoiceToFirestore(string choiceId, string choiceContent)
        {
            Debug.Log($"Uploading selected choice to Firestore. Choice ID: {choiceId}, Content: {choiceContent}");
    
            // Reference the Python collection
            CollectionReference pythonCollection = db.Collection("services").Document("tasks").Collection("python");

            // Prepare the data for the new document
            var choiceData = new Dictionary<string, object>
            {
                { "id", choiceId },
                { "type", "choice" },
                { "content", choiceContent }
            };

            // Generate a UUID for the new document name
            string documentName = System.Guid.NewGuid().ToString();

            // Add the document to Firestore
            pythonCollection.Document(documentName).SetAsync(choiceData).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log($"Choice uploaded to Firestore successfully. Document Name: {documentName}");
                }
                else
                {
                    Debug.LogError($"Failed to upload choice to Firestore: {task.Exception}");
                }
            });
        }



        private void ListenForMostRecentPythonTask()
        {
            CollectionReference pythonCollection = db.Collection("services").Document("tasks").Collection("python");

            pythonCollection.Listen(snapshot =>
            {
                if (snapshot != null && snapshot.Documents.Any())
                {
                    // Get the most recent document (assumes last added document is most recent)
                    var mostRecentDocument = snapshot.Documents.Last();

                    if (mostRecentDocument.Exists)
                    {
                        string id = mostRecentDocument.GetValue<string>("id");
                        string type = mostRecentDocument.GetValue<string>("type");

                        Debug.Log($"Most recent ID in python collection: {id}, Type: {type}");

                        if (type == "json")
                        {
                            // Fetch the corresponding JSON file from Firebase Storage
                            FetchJsonFromStorage(id);
                        }
                        else
                        {
                            Debug.LogWarning($"Document with ID {id} is not of type 'json'. Skipping file load.");
                        }
                    }
                }
                else
                {
                    Debug.Log("No documents found in the python collection.");
                }
            });
        }


        public void UploadNewNodeToFirestore(string nodeId, int choiceIndex, string choiceContent)
        {
            Debug.Log($"Uploading new node to Firestore with Node ID: {nodeId}");
            CollectionReference unityCollection = db.Collection("services").Document("tasks").Collection("unity");

            // Prepare the data for the new document
            var newNodeData = new Dictionary<string, object>
            {
                { "choiceId", choiceIndex }, // Convert index to 1-based
                { "type", "choice" },
                { "content", choiceContent }
            };

            // Add the document with the specified nodeId
            unityCollection.Document(nodeId).SetAsync(newNodeData).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log($"New document created in Firestore for Node ID {nodeId}.");
                }
                else
                {
                    Debug.LogError($"Failed to create document for Node ID {nodeId}: {task.Exception}");
                }
            });
        }
    }
    
    
}
