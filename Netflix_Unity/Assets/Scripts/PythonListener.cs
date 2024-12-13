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

        private void FetchJsonFromStorage(string id)
        {
            string jsonFileName = $"{id}.json"; // File named <id>.json
            StorageReference jsonFileRef = storageRef.Child(jsonFileName);

            string localPath = Path.Combine(Application.persistentDataPath, jsonFileName);
            jsonFileRef.GetFileAsync(localPath).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log($"JSON file downloaded to: {localPath}");

                    // Read the JSON file content
                    string jsonContent = File.ReadAllText(localPath);

                    // Debug log the contents of the JSON file
                    Debug.Log($"JSON Content: {jsonContent}");

                    // Send JSON content to StoryLoader
                    StoryLoader.Instance.LoadStoryData(jsonContent);
                }
                else
                {
                    Debug.LogError($"Failed to download JSON file: {task.Exception}");
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
                        Debug.Log($"Most recent ID in python collection: {id}");

                        // Fetch the corresponding JSON file from Firebase Storage
                        FetchJsonFromStorage(id);
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
