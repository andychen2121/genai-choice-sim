import firebase_admin
from firebase_admin import credentials
from firebase_admin import firestore

cred = credentials.Certificate('netflixcyoa-firebase-adminsdk-1a3hr-79f3ce291f.json')
firebase_admin.initialize_app(cred)

db = firestore.client()

def get_content():
    doc_ref = db.collection('services').document('tasks').collection('unity').document('2')

    doc = doc_ref.get()

    if doc.exists:
        content = doc.to_dict()['content']
        return content
    else:
        return "Document not found"

content = get_content()

print(f"Content: {content}")
