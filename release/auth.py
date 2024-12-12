import json
import os
import time
import threading
from enum import Enum
from typing import Callable
import firebase_admin
from firebase_admin import credentials, firestore, storage
from coordinator import Coordinator

# Initialize Firebase Admin SDK if not already done.
cred = credentials.Certificate('netflixcyoa-firebase-adminsdk-1a3hr-79f3ce291f.json')
firebase_admin.initialize_app(cred, {
    'storageBucket': 'netflixcyoa.firebasestorage.app'
})

class TaskType(Enum):
    INITIAL_STORY = 'initialStory'
    CHOICE = 'choice'

class FirebaseCoordinator:
    def __init__(self, service_id: str):
        self.service_id = service_id
        self.db = firestore.client()
        self.coordinator = Coordinator()
        self.bucket = storage.bucket()

    def connect(self):
        unity_tasks_ref = self.db.collection("services").document("tasks").collection("unity")
        doc_watch = unity_tasks_ref.on_snapshot(self.on_unity_snapshot)

    def on_unity_snapshot(self, snapshot, changes, read_time):
        """
        This callback is triggered whenever there's an update to the unity task document.
        We check the 'type' field and process accordingly.
        """
        for change in changes:
            # We only care about additions
            if change.type.name != "ADDED":
                continue

            doc = change.document
            data = doc.to_dict()

            # Expecting fields: type, content, choiceId
            if "type" not in data:
                continue

            action_type = data["type"]

            if action_type == TaskType.INITIAL_STORY.value:                
                IP = data.get("content", "DefaultStory")
                json_story = self.coordinator.initialize_storyline(IP)
                json_data = json.dumps(json_story)

                # add to storage
                blob = self.bucket.blob(f"{IP}.json")  # Specify the storage path
                blob.upload_from_string(json_data, content_type='application/json')

                # add to firebase
                doc_ref = self.db.collection("services").document("tasks").collection("python").document()
                doc_ref.set({'id': IP, 'type': 'json'})

            elif action_type == TaskType.CHOICE.value:
                choice_id = str(data.get("choiceId", "1"))
                result = self.coordinator.continue_story(choice_id)
                json_data = json.dumps(result)

                # add to storage
                blob = self.bucket.blob(f"{choice_id}.json")  # Specify the storage path
                blob.upload_from_string(json_data, content_type='application/json')

                # add to firebase
                doc_ref = self.db.collection("services").document("tasks").collection("python").document()
                doc_ref.set({'id': choice_id, 'type': 'json'})


if __name__ == '__main__':
    # Example usage
    # Before running this, make sure you've initialized Firebase admin:
    # cred = credentials.Certificate('/path/to/your/serviceAccountKey.json')
    # firebase_admin.initialize_app(cred)

    fc = FirebaseCoordinator(service_id="your_service_id_here")
    fc.connect()

    while True:
        time.sleep(1)