using OpenAI.Chat;
using OpenAI.Images;
using OpenAI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace OpenAI.Samples.Chat
{
    [RequireComponent(typeof(AudioSource))]
    public class ChatBehaviour : MonoBehaviour
    {
        [SerializeField] private OpenAIConfiguration configuration;

        [SerializeField] private bool enableDebug;

        private OpenAIClient openAI;

        private void Awake()
        {
            openAI = new OpenAIClient(configuration)
            {
                EnableDebug = enableDebug
            };
        }

        /// <summary>
        /// Generates a background image based on the provided plot text.
        /// </summary>
        /// <param name="plotText">The plot text to use as a prompt for the image generation.</param>
        /// <param name="onImageGenerated">Callback to handle the generated texture.</param>
        public async Task GenerateBackgroundImage(string plotText, Action<Texture2D> onImageGenerated)
        {
            Debug.Log($"Generating landscape background image for plot: {plotText}");

            try
            {
                string plot =
                    "Generate a scene that describes this scenario. Ensure that subsequent images are within the same art style an theme: " +
                    plotText;
                // Create an image generation request with a landscape size
                var request = new ImageGenerationRequest(
                    prompt: plot,
                    model: Models.Model.DallE_3 // Use DALL-E 3 model
                    
                );

                // Generate the image
                var imageResults = await openAI.ImagesEndPoint.GenerateImageAsync(request);

                if (imageResults != null && imageResults.Count > 0)
                {
                    foreach (var result in imageResults)
                    {
                        Debug.Log($"Generated Image Result: {result}");
                        Assert.IsNotNull(result.Texture); // Ensure the texture is not null

                        // Pass the texture back via the callback
                        onImageGenerated?.Invoke(result.Texture);
                    }
                }
                else
                {
                    Debug.LogError("Failed to generate an image or no image results returned.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while generating background image: {ex.Message}");
            }
        }

    }
}
