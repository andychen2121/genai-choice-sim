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
        private FirebaseFirestore db;
        private FirebaseStorage storage;
        private StorageReference storageRef;

        void Start()
        {
            Debug.Log("Initializing Python Listener...");

            // Initialize Firestore
            db = FirebaseFirestore.DefaultInstance;

            // Initialize Firebase Storage
            storage = FirebaseStorage.DefaultInstance;
            storageRef = storage.GetReferenceFromUrl("gs://netflixcyoa.firebasestorage.app");

            // Set up listener for the python collection
            ListenForMostRecentPythonTask();
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

        private void FetchJsonFromStorage(string id)
        {
            string jsonFileName = $"{id}.json"; // Assumes the JSON file name matches the ID
            StorageReference jsonFileRef = storageRef.Child(jsonFileName);

            string localPath = Path.Combine(Application.persistentDataPath, jsonFileName);
            jsonFileRef.GetFileAsync(localPath).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log($"JSON file downloaded to: {localPath}");

                    // Read and log the JSON file content
                    string jsonContent = File.ReadAllText(localPath);
                    Debug.Log($"JSON Content: {jsonContent}");

                    // Optionally process the JSON content
                    ProcessJsonContent(jsonContent);
                }
                else
                {
                    Debug.LogError($"Failed to download JSON file: {task.Exception}");
                }
            });
        }

        private void ProcessJsonContent(string jsonContent)
        {
            Debug.Log("Processing JSON content...");
            // Add custom logic to process the JSON content here
        }
    }
}
