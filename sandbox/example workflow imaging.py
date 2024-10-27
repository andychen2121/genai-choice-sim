# Full Example Notebook: Filter LAION-400M Dataset for Fantasy/Adventure Images and Fine-Tune Stable Diffusion
# DO NOT COPY CODE DIRECTLY -- IT WILL NOT WORK AND DEBUGGING WILL BE HARD. THIS IS MORESO A FRAMEWORK / WORKFLOW.

# 1. Importing Libraries
import requests
from datasets import load_dataset
from diffusers import StableDiffusionPipeline, DDPMScheduler
from torch.optim import AdamW
from torchvision import transforms
from PIL import Image
import torch

# 2. Load LAION-400M Dataset
# Note: This will take some time depending on your internet connection and compute resources
print("Loading LAION-400M dataset...")
dataset = load_dataset("laion/laion400m", split="train")

# 3. Define Keywords for Filtering
# We'll search for image-text pairs with the following fantasy/adventure keywords
keywords = ["fantasy", "dragon", "castle", "wizard", "magic", "adventure", "sword", "elf"]

# 4. Filter the Dataset
# This function checks if any of the keywords are in the text description
def filter_fantasy_adventure(example):
    text = example['TEXT'].lower()  # Make sure the text is lowercase
    return any(keyword in text for keyword in keywords)

# Applying the filter to the dataset
print("Filtering the dataset based on fantasy/adventure keywords...")
fantasy_dataset = dataset.filter(filter_fantasy_adventure)

# 5. Download and Preprocess Filtered Images
# We'll resize the images to 512x512 to match the input size of the Stable Diffusion model.
def download_and_preprocess_images(dataset, num_images=10):
    preprocess = transforms.Compose([
        transforms.Resize((512, 512)),
        transforms.ToTensor(),
    ])
    
    images = []
    
    for i, sample in enumerate(dataset.select(range(num_images))):  # Download and process 'num_images' images
        image_url = sample['URL']
        try:
            response = requests.get(image_url, timeout=10)
            if response.status_code == 200:
                # Load image from URL
                img = Image.open(requests.get(image_url, stream=True).raw).convert("RGB")
                img = preprocess(img)  # Resize and transform
                images.append(img)
                print(f"Downloaded Image {i + 1}: {image_url}")
            else:
                print(f"Failed to download Image {i + 1}: {image_url}")
        except Exception as e:
            print(f"Error downloading Image {i + 1}: {e}")
    
    return torch.stack(images)

# Download and preprocess 10 images for fine-tuning
print("\nDownloading and preprocessing images...")
image_data = download_and_preprocess_images(fantasy_dataset, num_images=10)
print(f"Downloaded {image_data.size(0)} images.")

# 6. Load Pre-trained Stable Diffusion Model
print("\nLoading pre-trained Stable Diffusion model...")
model_id = "CompVis/stable-diffusion-v1-4"  # Example model ID from Hugging Face
pipe = StableDiffusionPipeline.from_pretrained(model_id)
pipe.to("cuda")  # Move the model to GPU if available

# 7. Set Up the Fine-Tuning Process
# Use a custom DDPM (Denoising Diffusion Probabilistic Model) scheduler for the fine-tuning process
scheduler = DDPMScheduler()

# Set up the optimizer
optimizer = AdamW(pipe.unet.parameters(), lr=1e-5)

# 8. Define Fine-Tuning Loop
num_epochs = 3  # You can increase this for longer training
batch_size = 2  # Adjust based on your GPU memory

# Convert images to latent space
def encode_images(pipe, images):
    latents = []
    for img in images:
        latent = pipe.vae.encode(img.unsqueeze(0).to("cuda")).latent_dist.sample()
        latents.append(latent)
    return torch.cat(latents)

# Forward pass through UNet and compute loss
def fine_tune_step(images):
    # Step 1: Encode images into latent space
    latents = encode_images(pipe, images)
    
    # Step 2: Denoise using the model and compute loss
    noise = torch.randn_like(latents).to("cuda")
    noisy_latents = scheduler.add_noise(latents, noise, torch.tensor([0.1]).to("cuda"))  # Add noise
    pred_noise = pipe.unet(noisy_latents)["sample"]
    
    # Step 3: Compute the mean squared error (MSE) loss between predicted and actual noise
    loss = torch.nn.functional.mse_loss(pred_noise, noise)
    return loss

# 9. Training Loop
print("\nStarting fine-tuning process...")
for epoch in range(num_epochs):
    epoch_loss = 0.0
    for i in range(0, len(image_data), batch_size):
        batch_images = image_data[i:i + batch_size]
        
        # Zero gradients
        optimizer.zero_grad()
        
        # Forward pass and loss computation
        loss = fine_tune_step(batch_images)
        
        # Backward pass
        loss.backward()
        
        # Update parameters
        optimizer.step()
        
        epoch_loss += loss.item()
    
    print(f"Epoch {epoch + 1}/{num_epochs}, Loss: {epoch_loss:.4f}")

# 10. Save the Fine-Tuned Model
print("\nSaving the fine-tuned model...")
pipe.save_pretrained("fine_tuned_stable_diffusion_fantasy")

# 11. Generate Fantasy-Themed Image Using Fine-Tuned Model
prompt = "A majestic dragon flying over a castle at sunset"
print(f"\nGenerating image for prompt: '{prompt}'")
generated_image = pipe(prompt).images[0]
generated_image.show()

print("\nFine-tuning complete!")
