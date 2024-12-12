import mediapy as media
import random
import sys
import torch
import time
import os
import requests
from PIL import Image
from io import BytesIO
from datasets import load_dataset
from torch import nn
from torch.optim import AdamW
from torch.utils.data import DataLoader, Dataset
from tqdm.auto import tqdm

from diffusers import StableDiffusionPipeline

# to scale, compute can rely on cloud compute, e.g. AWS clusters
device = "cuda" if torch.cuda.is_available() else "cpu"

def initialize_image_model():
    # can fine-tune based on IP if wanted here; may be more suitable for internal game dev
    # e.g. dataset = ... --> fine-tune using code from sandbox/image_model_playground.ipynb

    model_id = "runwayml/stable-diffusion-v1-5"
    pipe = StableDiffusionPipeline.from_pretrained(model_id, torch_dtype=torch.float16).to(device)

    # Change the safety_checker line to return a list for each image:
    pipe.safety_checker = lambda images, clip_input: (images, [False]*len(images))

    print("Pipeline loaded and safety checker modified.")

    return pipe


def generate_image(pipe, prompt):
    generator = torch.manual_seed(1234)

    # can adjust settings for height and width
    images = pipe(prompt=prompt, num_inference_steps=50, guidance_scale=7.5, generator=generator, height=512, width=1024).images
    return images[0]
