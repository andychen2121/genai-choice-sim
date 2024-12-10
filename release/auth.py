import firebase_admin
from firebase_admin import credentials, firestore
import time
from coordinator import Coordinator

# Initialize Firebase app
cred = credentials.Certificate('/path/to/your/serviceAccountKey.json')
firebase_admin.initialize_app(cred)

# Initialize Firestore DB
db = firestore.client()

# Initialize Coordinator
coordinator = Coordinator()


# Callback function to handle changes
def on_snapshot(doc_snapshot, changes, read_time):
    # condition 1: game was just created; check if static generation is necessary and store IP
    if ...:
        IP = ...
        coordinator.initialize_storyline(IP)
    # condition 2: dynamic generation
    elif ...:
        # need to communicate labels / next node
        # get new label from firebase
        continue_story()


# Watch a document
doc_ref = db.collection('your_collection').document('your_document')
doc_watch = doc_ref.on_snapshot(on_snapshot)

# Keep the script running
while True:
    time.sleep(1)