import json
import firebase_admin
from firebase_admin import credentials
from firebase_admin import firestore
from firebase_admin import storage

cred = credentials.Certificate('netflixcyoa-firebase-adminsdk-1a3hr-79f3ce291f.json')
firebase_admin.initialize_app(cred, {
    'storageBucket': 'netflixcyoa.firebasestorage.app'
})

db = firestore.client()

def get_content_from_firestore():
    doc_ref = db.collection('services').document('tasks').collection('unity').document('2')

    doc = doc_ref.get()

    if doc.exists:
        content = doc.to_dict()['content']
        return content
    else:
        return "Document not found"
    
def get_content_from_storage():
    bucket = storage.bucket()
    blob = bucket.blob('1')

    # access string; convert to JSON
    json_string = blob.download_as_text()
    data = json.loads(json_string)
    if data:
        return data
    else:
        return "Document not found"

# content = get_content_from_firestore()
content = get_content_from_storage()

print(f"Content: {content}")
