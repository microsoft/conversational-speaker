# Conversational Speaker
The Conversational Speaker, informally known as "Friend Bot", uses a Raspberry Pi to enable a spoken conversation with OpenAI large language models. This implementation listens to speech, processes the conversation through the OpenAI service, and responds back.

For more information on the prompt engine used for maintaining conversation context, go here: [python](https://github.com/microsoft/prompt-engine-py), [typescript](https://github.com/microsoft/prompt-engine), [dotnet](https://github.com/microsoft/prompt-engine-dotnet).

For more information about prompt design in general, checkout OpenAI's documentation on the subject: https://beta.openai.com/docs/guides/completion/prompt-design.

This project is written in .NET 6 which supports Linux/Raspbian, macOS, and Windows.

![Conversational Speaker](/package.png "Conversational Speaker")

**Build time**: 30 minutes

**Read time**: 15 minutes

**Cost**: 
 - Hardware ~$50
   - [Raspberry PI 4 Model B](https://www.raspberrypi.com/products/raspberry-pi-4-model-b)
   - USB Omnidirectional Speakerphone (e.g. [this one](https://www.amazon.com/dp/B08THGFBTV)
- Software
  - Azure Cognitive Speech Services
    - **Free tier** supports 5 audio hours free per month and 1 concurrent request ([Azure Cognitive Services pricing](https://azure.microsoft.com/pricing/details/cognitive-services/speech-services)).
    - New Azure accounts include $200 in free credit that can be used during the first 30 days.
  - OpenAI
    - Davinci models (most powerful): $0.02 / ~750 words, Curie models (still pretty good with faster response time): $0.002 / ~750 words
    - New OpenAI accounts include $18 in free credit that can be used during your first 90 days. For more details: https://openai.com/api/pricing/


# Setup
You will need an instance of Azure Cognitive Services for speech-to-text and text-to-speech, as well as an OpenAI account in which to have a conversation. You can run the software on nearly any platform, but let's start with setting up a Raspberry Pi first...

## Raspberry Pi
_If you are new to Raspberry Pis now would be a good time to check out the [getting started](https://projects.raspberrypi.org/en/projects/raspberry-pi-getting-started)._
### 1. OS
1. Insert an SD card into your PC
1. Go to https://www.raspberrypi.com/software/ then download and run the Raspberry Pi Imager
1. Click `Choose OS` and select the default Raspberry Pi OS (32-bit).
1. Click `Choose Storage`, select the SD card
1. Click `Write` and wait for the imaging to complete.
1. Put the SD card into your Raspberry Pi and connect a keyboard, mouse, and monitor.
1. Complete the initial setup, making sure to configure Wi-Fi.

### 2. USB Speaker/Microphone
1. Plug in the USB speaker/microphone if you have not already
1. Right-click on the volume icon in the top-right of the screen and make sure the USB device is selected.
1. Right-click on the microphone icon in the top-right of the screen and make sure the USB device is selected.

## Azure
The conversational speaker uses Azure Cognitive Service for speech-to-text and text-to-speech. Below are the steps to create an Azure account and an instance of Azure Cognitive Services.
### 1. Create an Azure account (if you have not already)
  1. In a web browser, navigate to http://www.azure.com and click on `Try Azure for Free`.
  1. Click on `Start Free` to start creating a free Azure account.
  1. Sign in with your Microsoft or GitHub account.
  1. After signing in, you'll be prompted to enter some information.
  1. Even though this is a free account, Azure still requires credit card information. You will not be charged unless you change settings later.
  1. After your account setup is complete, navigate to https://portal.azure.com

### 2. Create an instance of Azure Cognitive Services
  1. Sign into your account at https://portal.azure.com.
  1. In the search bar at the top, enter `Cognitive Services` and under `Marketplace` select `Cognitive Services` (it may take a moment to populate).
  1. Verify the correct subscription is selected, then under `Resource Group` select `Create New` and enter a resource group name (e.g. `conv-speak-rg`)
  1. Select a region and a name for your instance of Azure Cognitive Services (e.g. `my-conv-speak-cog-001`). I recommend using either East US, West Europe, or Southeast Asia as those regions tend to support the greatest number of features.
  1. Click on `Review + Create` and after validation passes, click `Create`.
  1. When deployment has completed you can click "Go to resource" to view your Azure Cognitive Services resource.
  1. On the left side navigation bar, select `Keys and Endpoint` under `Resource Management`.
   - Copy either of the two Cognitive Services keys and save in a secure location for later.




## OpenAI
The conversational speaker uses OpenAI's models to hold a friendly conversation. Below are the steps to create a new account and access the AI models.
### 1. Create an OpenAI account (if you have not already)
  1. In a web browser, navigate to https://openai.com/api/ and click `Sign up`
  1. You can use a Google account, Microsoft account, or email to create a new account.
  1. Complete the sign-up process (e.g., create a password, verify your email, etc).
     - If you are new to OpenAI, please review the usage guidelines (https://beta.openai.com/docs/usage-guidelines).
  1. In the top-right corner click on your account, then `View API keys`.
  1. Click `+ Create new secret key`, copy it and save it in a secure location for later.

  _If you are curious to play with the large language models directly, check out the `Playground` at the top of the page._

# The Code
## 1. Get and configure the code.
1. On the Raspberry Pi or your PC, open a command-line terminal
1. Install .NET 6 SDK
   - For Raspberry Pi and Linux:
     ```
     curl -sSL https://dot.net/v1/dotnet-install.sh | bash
     ``` 
     After installing is complete (it may take a few minutes), add dotnet to the command search paths
     ```
     echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
     echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
     source ~/.bashrc
     ```
     You can verify that dotnet was installed successfully by checking the version
     ```
     dotnet --version
     ```
   - For Windows, go to https://dotnet.microsoft.com/download, click `Download .NET SDK x64`, and run the installer.
1. Clone the repo
   ```
   git clone --recursive https://github.com/microsoft/conversational-speaker.git
   ```
1. Set your API keys, replacing `{MyCognitiveServicesKey}` with your Azure Cognitive Services key and `{MyOpenAIKey}` with your OpenAI API key from the sections above.
   ```
   cd ~/conversational-speaker/src/ConversationalSpeaker
   dotnet user-secrets set "AzureCognitiveServices:Key" "{MyCognitiveServicesKey}"
   dotnet user-secrets set "OpenAI:Key" "{MyOpenAIKey}"
   ```
1. Build and run the code!
   ```
   cd ~/conversational-speaker/src/ConversationalSpeaker
   dotnet build
   dotnet run
   ```

## 2. (Optional) Setup the application to start on boot
There are several ways to run a program when the Raspberry Pi boots. Below is my preferred method which runs the application in a visible terminal window automatically. This allows you to not only see the output but also cancel the application by clicking on the terminal window and pressing CTRL+C. 
1. Create a file `/etc/xdg/autostart/friendbot.desktop`
   ```
   sudo nano /etc/xdg/autostart/friendbot.desktop
   ```
1. Put the following content into the file
   ```
   [Desktop Entry]
   Exec=lxterminal --command "/bin/bash -c '~/.dotnet/dotnet run --project ~/conversational-speaker/src/ConversationalSpeaker; /bin/bash'"
   ```
   Press CTRL+O to save the file and CTRL+X to exit. This will run the application in a terminal window after the Raspberry Pi has finished booting.
1. To test out the changes you can reboot simply by running 
   ```
   reboot
   ```
## 3. (Optional) Create a custom wake word
The code base has a default wake word (i.e. "Hey, Computer.") already, which I suggest you use first. If you want to create your own (free!) custom wake word, then follow the steps below.
  1. Create a custom keyword model using the directions here: https://aka.ms/hackster/microsoft/wakeword. 
  1. Download the model, extract the `.table` file, and overwrite `src/ConversationalSpeaker/Handlers/WakeWordModel.table`.
  1. Rebuild and run the project to use your custom wake word.

## 3. Usage
- The current state of the prompt engine usually remains stable for short conversations. Sometimes during longer conversations, though, the AI may start responding with not only its own response but what it thinks you might say next.
- It is recommended to set context by starting with "Hello, my name is Jordan and I live in Redmond, Washington."
- To start a new conversation, say "Start a new conversation". 
- Take a look at the `~/conversational-speaker/src/ConversationalSpeaker/configuration.json`. 
  - Change the AI's name (`PromptEngine:OutputPrefix`), 
  - Change the AI's voice (`AzureCognitiveServices:SpeechSynthesisVoiceName`)
  - Change the AI's personality (`PromptEngine:Description`)
  - Switch to text input by changing the `System:TextListener` to `true` (good for testing changes).
  
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
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion
or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
