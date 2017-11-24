# Wrld Unity SDK

This Unity package demonstrates basic use of the Wrld SDK to display beautiful 3D maps via Unity. [Click here](https://docs.wrld3d.com/unity/latest/docs/api/) to access the full documentation.

## API Key
To use the Unity SDK, you will first need to acquire a [Wrld API Key](https://www.wrld3d.com/apikeys/) by [signing up](https://www.wrld3d.com/register/) for a free account. The token is a 32 length string consisting of alpha numeric characters.

### Quickstart
1. Create a new empty 3D Unity Project.
2. Import the Wrld SDK Unity Package into the editor.
3. Navigate to Assets/Wrld/Scenes/ and open the UnityWorld scene.
4. Click on the WrldMap game object and expand the WrldMap script in the Inspector window.
5. Paste your API key in the box provided.
6. Check the `Use Built-in Camera Controls` box under `Camera/View Settings`.
7. In the Unity Editor, click Play and wait a few seconds for the map to stream in. Use the left and right mouse buttons to pan and rotate the map respectively.

To deploy to other platforms checkout the full [documentation website](https://docs.wrld3d.com/unity/latest/docs/api/).

### Requirements & Supported Platforms
*   [Unity 5.5.0](https://unity3d.com/get-unity/download) or newer
*   Android
    *   [Android SDK](https://docs.unity3d.com/Manual/android-sdksetup.html) downloaded and installed
    *   API 23 (Android 6.0) & SDK Tools >= 24.0.3\. Both can be downloaded using the [SDK Manager](https://developer.android.com/studio/intro/update.html#sdk-manager)
    *   [JDK 1.8](http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html)
    *   Setup the Android SDK and JDK [Path in Unity](https://docs.unity3d.com/Manual/android-sdksetup.html)
    *   [Android NDK **r10e**](http://stackoverflow.com/a/28088215) for IL2CPP compilation (default is Mono2x)
*   iOS
    *   macOS >= 10.7
    *   XCode >= 7.3
    *   A valid code signing certificate and development device
*   macOS
    *   macOS >= 10.7
    *   XCode >= 7.3
*   Windows
    *   Windows 7.1 x64 or higher
    *   [Visual Studio](https://www.visualstudio.com/vs/community/) 2015 Community or above

### Updating

If you're updating to this version from an older version of the WRLD SDK, please apply the update to your project before running scenes, and restart the Unity Editor after the update has been applied. We recommend backing up your project beforehand.

### Support

If you have any questions, suggestions, or bug reports, you can open an issue on our [public GitHub repo](https://github.com/wrld3d/unity-api/issues) or contact us by email at `support@wrld3d.com`.
