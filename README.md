# Conversational Speaker
The Conversational Speaker, informally known as "Friend Bot", uses a Raspberry Pi to enable a spoken conversation with OpenAI large language models. This implementation listens to speech, processes the conversation through the OpenAI service, and responds back.

For more information on the prompt engine used for maintaining conversation context, go here: [python](https://github.com/microsoft/prompt-engine-py), [typescript](https://github.com/microsoft/prompt-engine), [dotnet](https://github.com/microsoft/prompt-engine-dotnet).

For more information about prompt design in general, checkout OpenAI's documentation on the subject: https://beta.openai.com/docs/guides/completion/prompt-design.

This project is written in .NET 6 which supports Linux/Raspbian, macOS, and Windows.

## Usage
- I recommend setting context for yourself by startingn with "Hello, my name is Adrian and I live in Redmond, Washington."
- To start a new conversation, say "Start a new conversation". 
- You can switch to text input by changing the `System:TextListener` setting in `./src/ConversationalSpeaker/configuration.json` to `true`.

## Estimated Costs
### Hardware
- Raspberry PI 4 Model B: $35+
  - Link: https://www.raspberrypi.com/products/raspberry-pi-4-model-b/
- USB Omnidirectional Speakerphone: $30+
  - I'm using this one, though any USB speaker and microphone should work: https://www.amazon.com/dp/B098DKS637 
### Software
- Azure Cognitive Speech Services
  - Free tier: 5 audio hours free per month, 1 concurrent request
  - New Azure accounts include $200 in free credit that can be used during the first 30 days. For more details on Azure Cognitive Services pricing: https://azure.microsoft.com/en-us/pricing/details/cognitive-services/speech-services/
- OpenAI: 
    - Davinci models (most powerful): $0.02 / ~750 words
  - Curie models (still pretty good with faster response time): $0.002 / ~750 words
  - New OpenAI accounts include $18 in free credit that can be used during your first 90 days. For more details: https://openai.com/api/pricing/

## Setup
You will need an instance of Azure Cognitive Services for speech-to-text and text-to-speech, as well as an OpenAI account in which to have a conversation. Let's set those up first...

### Azure
The conversational speaker uses Azure Cognitive Service for speach-to-text and text-to-speach. Below are the steps to create an Azure account and an instance of Azure Cognitive Services.
#### Create a Azure account (if you have not already)
1. In a web browser, navigate to http://www.azure.com and click on `Try Azure for Free`.
1. Click on `Start Free` to start creating a free Azure account.
1. Sign in with your Microsoft or Github account.
1. After signing in, you'll be prompted to enter some information.
1. Even though this is a free account, Azure still requires credit card information. You will not be charged unless you change settings later.
1. After your account setup is complete, navigate to https://portal.azure.com

#### Create an instance of Azure Cognitive Services
1. Sign into your account at https://portal.azure.com.
1. In the search bar at the top, enter `Cognitive Services` and under `Marketplace` select `Cognitive Services` (it may take a moment to populate).
1. Verify the correct subscription is selected, then under `Resource Group` select `Create New` and enter a resource group name (e.g. `conv-speak-rg`)
1. Select a region and a name for your instance of Azure Cognitive Services (e.g. `my-conv-speak-cog-001`)
   - For more details on pricing, see https://azure.microsoft.com/en-us/pricing/details/cognitive-services/, select your region, and select "Language".
1. Click on `Review + Create` and after validation passes, click `Create`.
1. When deployment has completed you can click "Go to resource" to view your Azure Cognitive Services resource.
1. On the left side navigation bar, select `Keys and Endpoints` under `Resource Management`.
   - Here you are able to view and copy your Cognitive Services API keys.

### OpenAI
The conversational speaker uses OpenAI's models to hold a friendly conversation. Below are the steps to create a new account and access the AI models.
#### Create an OpenAI account (if your have not already)
1. In a web browser, navigate to https://openai.com/api/ and click `Sign up`
1. You can use a Google account, Microsoft account, or email to create a new account.
1. Complete the sign-up process (e.g. create a password, verify your email, etc).
   - If you are new to OpenAI, please review the usage guidelines (https://beta.openai.com/docs/usage-guidelines).
1. In the top-right corner click on your account, then `View API keys`.
   - Here you are able to view and copy your OpenAI API keys.

_If you are curious to play with the models directly, check out the `Playground` at the top of the page._

#### Configuring API keys for desktop development
1. If you have not already, download and install the .NET 6 SDK for your platform here: https://dotnet.microsoft.com/en-us/download/dotnet/6.0.
1. Open a command-line terminal and change directory to `./src/ConversationalSpeaker`.
1. Run `dotnet user-secrets set "AzureCognitiveServices:Key" "****` and replace **** with one of the keys from your Azure Cognitive Services instance.
1. Run `dotnet user-secrets set "OpenAI:Key" "****` and replace **** with one of the keys from your OpenAI account.

Optionally you can set key values directly in `./src/ConversationalSpeaker/configuration.json`. 
Though, **THIS IS DANGEROUS** and you should never check-in your API keys to a git repository - even locally.

### __If you accidentally checked in an API key to git:__
1. Regenerate your OpenAI key: 
   1. In a web browser, navigate to https://openai.com/api.
   1. In the top-right corner click on your account, then "View API keys".
   1. Click the garbage can next to the exposed API Key to delete it.
   1. Click "+ Create new secret key" to regenerate a new one.
1. Regenerate your Azure Cognitive Services key:
   1. In a web browser, navigate to https://portal.azure.com and go to your instance of Azure Cognitive Services (e.g. `my-conv-speak-cog-001`)
   1. On the left side navigation bar, select `Keys and Endpoints` under `Resource Management`.
   1. Near the top of the screen, click either `Regenerate Key1` or `Regenerate Key2`, whichever one was checked in to git (regenerating both is also acceptable).

## Building and Running
1. If you have not already, download and install the .NET 6 SDK for your platform here: https://dotnet.microsoft.com/en-us/download/dotnet/6.0.
1. After you have installed the .NET 6 SDK, open a command-line terminal and go to the `./src` directory.
1. Run `dotnet build .` to build the project.
1. After building, go to `./src/ConversationalSpeaker` and run `dotnet run` to start the project.

## Contributing
This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the
[Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the
[Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com)
with any additional questions or comments.

## Trademarks
This project may contain trademarks or logos for projects, products, or services. Authorized use
of Microsoft trademarks or logos is subject to and must follow
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion
or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
