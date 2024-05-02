# Sors

It's a card game created with Unity. Networked Host-Client model on LAN with Mirror.

Check out the rules: https://github.com/MattW0/Sors/blob/main/Assets/Resources/Rules/rules.md

## Setup

**Downloads**:
* Unity - Install latest Hub version and with it, the Unity Editor version ?? : https://unity.com/download
* VS Code - latest: https://code.visualstudio.com/download 
* .NET Sdk - v6.0+: https://dotnet.microsoft.com/en-us/download
* Project files - branch 'dev': https://github.com/MattW0/Sors
~~~
git clone -b dev https://github.com/MattW0/Sors.git
~~~

**Open the project**:
* Open Unity Hub, click Open and select the project folder you just created. Select the Unity version specified above and open the project (this might prompt you that the project must update from an older version, click continue anyways).
* The project will open in safe mode with errors, because there are missing packages: Mirror and DotWeen. Install and import them from here. Move the created folders to Assets/Imported Assets after importing:
    * Mirror: https://assetstore.unity.com/packages/tools/network/mirror-129321
    * DotWeen: https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676$
    * ParrelSync (for testing multiplayer locally): https://github.com/VeriorPies/ParrelSync
* Once all errors are gone, switch to scene 'Menu' (in Assets/_scenes/). Import TMP Essentials if prompted.
* Press Ctrl + Shift + B to open build settings and click 'Add Open Scene'. Switch to scene 'Game' and do the same.
* Now you should be able to run the game starting from the scene 'Menu'.

**Utilities (optional)**:
* VS Code - Install the C# and Unity extensions: https://code.visualstudio.com/docs/other/unity
* VS Code - Disable .meta files in explorer: https://answers.unity.com/questions/1260730/how-to-hide-meta-and-csmeta-files-in-visual-studio.html


### Have fun!
