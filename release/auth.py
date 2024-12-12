import json
import os
import threading
from enum import Enum
from typing import Callable
import firebase_admin
from firebase_admin import credentials, firestore
from coordinator import Coordinator

from LLM_plot_gen import create_story, generate_continuation, assert_valid_json, MAX_RETRIES

# Initialize Firebase Admin SDK if not already done.
cred = credentials.Certificate('netflixcyoa-firebase-adminsdk-1a3hr-79f3ce291f.json')
firebase_admin.initialize_app(cred)

class TaskType(Enum):
    INITIAL_STORY = 'initialStory'
    CHOICE = 'choice'

class FirebaseCoordinator:
    def __init__(self, service_id: str):
        self.service_id = service_id
        self.db = firestore.client()
        self.coordinator = Coordinator()

    def connect(self):
        unity_tasks_ref = self.db.collection("service").document(self.service_id).collection("tasks").document("unity")
        doc_watch = unity_tasks_ref.on_snapshot(self.on_unity_snapshot)

    def on_unity_snapshot(self, snapshot, changes, read_time):
        """
        This callback is triggered whenever there's an update to the unity task document.
        We check the 'type' field and process accordingly.
        """
        for doc in snapshot:
            data = doc.to_dict()
            doc_ref = self.db.collection("service").document(self.service_id).collection("tasks").document("python")

            # Expecting fields: type, content, choiceId
            if "type" not in data:
                continue

            action_type = data["type"]

            if action_type == TaskType.INITIAL_STORY.value:
                IP = data.get("content", "DefaultStory")
                json_story = self.coordinator.initialize_storyline(IP)

                doc_ref.collection("responses").add(json_story)

            elif action_type == TaskType.CHOICE.value:
                choice_id = str(data.get("choiceId", "1"))
                result = self.coordinator.continue_story(choice_id)

                
                doc_ref.collection("responses").add(result)


if __name__ == '__main__':
    # Example usage
    # Before running this, make sure you've initialized Firebase admin:
    # cred = credentials.Certificate('/path/to/your/serviceAccountKey.json')
    # firebase_admin.initialize_app(cred)

    fc = FirebaseCoordinator(service_id="your_service_id_here")
    fc.connect()

    while True:
        pass