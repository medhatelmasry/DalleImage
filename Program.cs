using System.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToImage;

// Get configuration settings from App.config
string _endpoint = ConfigurationManager.AppSettings["endpoint"]!;
string _apiKey = ConfigurationManager.AppSettings["api-key"]!;
string _dalleDeployment = ConfigurationManager.AppSettings["dalle-deployment"]!;
string _gptDeployment = ConfigurationManager.AppSettings["gpt-deployment"]!;

// Create a kernel builder
var builder = Kernel.CreateBuilder();

// Add OpenAI services to the kernel
builder.AddAzureOpenAITextToImage(_dalleDeployment, _endpoint, _apiKey);
builder.AddAzureOpenAIChatCompletion(_gptDeployment, _endpoint, _apiKey);

// Build the kernel
var kernel = builder.Build();

// Get AI service instance used to generate images
var dallE = kernel.GetRequiredService<ITextToImageService>();

var prompt = @"
Think about an artificial object that represents {{$input}}.";

// create execution settings for the prompt
var executionSettings = new OpenAIPromptExecutionSettings {
    MaxTokens = 256,
    Temperature = 1
};

// create a semantic functioin from the prompt
var genImgFunction = kernel.CreateFunctionFromPrompt(prompt, executionSettings);

// Get a phrase from the user
Console.WriteLine("Enter a phrase to generate an image from: ");
string? phrase = Console.ReadLine();

if (string.IsNullOrEmpty(phrase))
{
    Console.WriteLine("No phrase entered.");
    return;
}

// Invoke the semantic function to generate an image description
var imageDescResult = await kernel.InvokeAsync(genImgFunction, new() { ["input"] = phrase });
var imageDesc = imageDescResult.ToString();

// Use DALL-E 3 to generate an image. 
// In this case, OpenAI returns a URL (though you can ask to return a base64 image)
var imageUrl = await dallE.GenerateImageAsync(imageDesc.Trim(), 1024, 1024);

Console.WriteLine($"Image URL:\n\n{imageUrl}");
