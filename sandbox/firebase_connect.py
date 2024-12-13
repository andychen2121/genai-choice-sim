import json
import firebase_admin
from firebase_admin import credentials
from firebase_admin import firestore
from firebase_admin import storage

# Your Firebase project credentials (replace with your actual values)
cred = credentials.Certificate('netflixcyoa-firebase-adminsdk-1a3hr-79f3ce291f.json')
firebase_admin.initialize_app(cred, {
    'storageBucket': 'netflixcyoa.firebasestorage.app'
})

db = firestore.client()
bucket = storage.bucket()

def my_function():
    # Your function logic to generate JSON data
    data = {
        "name": "John Doe",
        "age": 30,
        "city": "New York"
    }
    return data

# Call your function and get the JSON data
json_data = my_function()

def add_to_firestore():
    # Add the JSON data as a document in Firestore
    doc_ref = db.collection("services").document("tasks").collection("python").document()
    doc_ref.set(json_data)

    print(f"Data added to Firestore: {json_data}")

def add_to_storage():
    # Convert the JSON data to a string
    json_string = json.dumps(json_data)

    # Upload the JSON string directly to Firebase Storage
    blob = bucket.blob('data.json')  # Specify the storage path
    blob.upload_from_string(json_string, content_type='application/json')

    print(f"Data uploaded to Firebase Storage at: {blob.public_url}: {json_data}")

add_to_storage()
add_to_firestore()