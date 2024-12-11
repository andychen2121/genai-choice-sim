import firebase_admin
from firebase_admin import credentials
from firebase_admin import firestore

# Your Firebase project credentials (replace with your actual values)
cred = credentials.Certificate('netflixcyoa-firebase-adminsdk-1a3hr-79f3ce291f.json')
firebase_admin.initialize_app(cred)

db = firestore.client()

def get_content():
    # Get a reference to the collection and document
    doc_ref = db.collection('services').document('tasks').collection('unity').document('2')

    # Retrieve the document
    doc = doc_ref.get()

    # Check if the document exists
    if doc.exists:
        # Extract the "content" field
        content = doc.to_dict()['content']
        return content
    else:
        return "Document not found"

# Call the function to retrieve the content
content = get_content()

print(f"Content: {content}")
