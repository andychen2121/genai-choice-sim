import firebase_admin
from firebase_admin import credentials
from firebase_admin import firestore

# Your Firebase project credentials (replace with your actual values)
cred = credentials.Certificate('netflixcyoa-firebase-adminsdk-1a3hr-79f3ce291f.json')
firebase_admin.initialize_app(cred)

db = firestore.client()

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

# Add the JSON data as a document in Firestore
doc_ref = db.collection('myData').document()
doc_ref.set(json_data)

print(f"Data added to Firestore: {json_data}")
