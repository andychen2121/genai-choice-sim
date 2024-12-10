import firebase_admin
from firebase_admin import credentials, firestore, storage
import json
import uuid
import time
from coordinator import Coordinator

# Initialize Firebase app
cred = credentials.Certificate('/path/to/your/serviceAccountKey.json')
firebase_admin.initialize_app(cred, {
    'storageBucket': 'your-bucket-name.appspot.com'
})

# Initialize Firestore DB and Storage
db = firestore.client()
bucket = storage.bucket()

# Initialize Coordinator
coordinator = Coordinator()

# Keeps track of current choice
curr_choice = ""

def on_snapshot(doc_snapshot, changes, read_time):
    for change in changes:
        doc = change.document
        data = doc.to_dict()
        
        # Check what type of change occurred
        if change.type.name == 'initialStory':
            # Condition 1: Game was just created.
            # For example, you might check if a field 'status' == 'new_game'
            if data.get('status') == 'new_game':
                # Extract IP from the doc (assuming it's stored in 'IP' field)
                IP = data.get('IP', '127.0.0.1')
                
                # Initialize storyline
                story_json = coordinator.initialize_storyline(IP)
                
                # Create a unique ID for the storyline
                story_id = str(uuid.uuid4())
                
                # Upload the JSON to Cloud Storage
                blob = bucket.blob(f'storylines/{story_id}.json')
                blob.upload_from_string(json.dumps(story_json), content_type='application/json')
                
                # Update Firestore document with the storyline ID (and maybe a status change)
                doc.reference.update({
                    'storyline_id': story_id,
                    'status': 'initialized'  # Mark that static generation is done
                })
        
        elif change.type.name == 'choice':
            # Condition 2: Dynamic generation triggered.
            # For example, check if a field 'action' == 'continue_story' or some field changes.
            if data.get('action') == 'continue_story':
                # Call a method to continue the storyline
                new_part = coordinator.continue_story(data.get('storyline_id'))
                
                # Generate a new partial ID
                partial_id = str(uuid.uuid4())
                
                # Upload partial storyline JSON
                blob = bucket.blob(f'storylines/{partial_id}.json')
                blob.upload_from_string(json.dumps(new_part), content_type='application/json')
                
                # Update Firestore with partial storyline id and reset action
                doc.reference.update({
                    'partial_storyline_id': partial_id,
                    'action': firestore.DELETE_FIELD  # or set it to None
                })

                curr_choice += choiceID

        # If you want to handle REMOVED or other cases, do so here.


# Set up a listener on a specific document
doc_ref = db.collection('your_collection').document('your_document')
doc_watch = doc_ref.on_snapshot(on_snapshot)

# Keep the script running
while True:
    time.sleep(1)