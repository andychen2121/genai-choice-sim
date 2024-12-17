# Choose Your Own Adventure Game

This interactive **GenAI x Netflix** Choose Your Own Adventure Game leverages generative AI models to deliver branching immersive storytelling experiences.

---

## 🚀 Project Overview

This game combines a **language model (LLM)** for generating branching narratives, an **image generation model** for visual storytelling, and **real-time synchronization** via Firebase and Unity. Players make choices that shape the story, unlocking unique outcomes and personalized adventures.

---

# Step 1: Set Up Firebase Credentials
# - Obtain the Firebase service account JSON file for your project.
# - Place the JSON file in the project directory (e.g., firebase_credentials.json).
# - Ensure the Firebase project has Firestore and Firebase Storage enabled.

# Step 2: Install Python Dependencies
# - Make sure Python 3.8+ is installed on your system.
pip install -r requirements.txt

# Step 3: Run the Backend
# - Start the authentication script to initialize Firebase and ensure the backend is set up.
python auth.py

# Step 4: Set Up Unity Frontend
# - Open the Unity project in the `release/` folder using Unity Hub.
# - Ensure Unity's Firebase SDK is correctly configured:
#   1. Import the Firebase Unity SDK into the Unity project.
#   2. Replace the default `google-services.json` (Android) or `GoogleService-Info.plist` (iOS) 
#      with your project's configurations from the Firebase Console.
# - Verify that the Unity project successfully connects to Firebase:
#   - Check that Firestore and Storage integration scripts are enabled.
#   - Confirm that placeholder assets update dynamically.

# Step 5: Run Unity
# - Click Play in the Unity Editor to start the game.
# - Watch as the game synchronizes real-time narrative and visuals using Firebase.

# Step 6: Test Gameplay
# - Interact with the game to verify:
#   - Narrative choices update in real-time.
#   - AI-generated visuals are correctly loaded and displayed.

---

## 🔧 Key Features

### 1. **Interactive Storytelling with LLM**
- At each stage, the LLM generates:
  - A detailed *situation*
  - Four *choices* for players to decide the story's direction
- All story data is saved and propagated to Firebase/Firestore for real-time access.

### 2. **Visual Storytelling with Image Generation**
- AI-generated images depict each narrative moment using Textual Inversion techniques.
- Seamless integration ensures each choice is accompanied by rich, contextual visuals.

### 3. **Real-Time Synchronization via Firebase**
- Narrative and image data are stored in Firestore and Firebase Storage.
- Unity dynamically updates the game environment based on real-time data changes.

### 4. **Unity Frontend**
- Unity uses Firebase data to populate placeholder assets, creating a fluid and immersive gameplay experience.

---

## 🛠️ Technical Implementation

### LLM
- **Input:** Wikipedia plot summaries for initial seeds.
- **Output:** A recursive, branching narrative structure in JSON format.
- **Features:**
  - Unique, hierarchical indexing (e.g., `1`, `11`, `111`).
  - Real-time generation for player choices.
  - Hybrid approach combining static pre-generated layers and dynamic, on-demand branching.

### Image Generation
- **Technique:** Textual Inversion for high-quality, IP-specific imagery.
- **Workflow:**
  1. Generate descriptive text for each scenario.
  2. AI model creates corresponding visuals.
  3. Images are compressed, uploaded to Firebase, and dynamically loaded into Unity.

### Backend + Unity Integration
- Python backend handles narrative and image generation.
- Firebase serves as the communication bridge:
  - Firestore: Stores narrative JSONs and metadata.
  - Firebase Storage: Hosts large assets (images, videos).
- Unity listens for data changes via Firestore's change streams and updates the game in real-time.

---

## 📂 Folder Structure

- **`data/`**: Organizes game data (e.g., story seeds, generated narratives).
- **`release/`**: Contains production-ready assets:
  - Python scripts for LLM and image generation.
  - Unity assets for frontend integration.
- **`sandbox/`**: Proof-of-concept prototypes for LLM and image model fine-tuning.
- **`.gitignore`**: Ensures sensitive files (e.g., Firebase credentials) are excluded.
- **`requirements.txt`**: Python dependencies for the backend.

---

## 🚧 Development Workflow

1. **Static Story Generation**:
   - Generates the initial narrative tree with multiple layers for immediate exploration.
   - Example: Root node → 4 choices → 16 sub-nodes.
2. **Dynamic Real-Time Expansion**:
   - New nodes are created dynamically as players progress.
   - Ensures seamless branching with low latency.

3. **Real-Time Synchronization**:
   - Firebase ensures that both narrative and visuals update instantly in Unity.

---

## 🔮 Future Improvements

- **Enhanced Image Generation**:
  - Experiment with models like DALL-E 3 or Midjourney for artistic depth.
  - Refine Netflix-specific datasets for more granular style representations.
- **Advanced Narrative Features**:
  - Add dynamic checkpoints, failure scenarios, and probabilistic branching.
- **Optimized Communication**:
  - Investigate WebSocket-based protocols for even lower latency.
- **Expanded Visual Depth**:
  - Generate multiple images per scenario for richer storytelling.
