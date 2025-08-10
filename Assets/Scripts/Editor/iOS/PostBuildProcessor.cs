// Check if the build target is iOS
#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public static class PostBuildProcessor
{
    /// <summary>
    /// Post-process build steps for iOS.
    /// </summary>
    /// <param name="target">The build target.</param>
    /// <param name="path">The path to the built player.</param>
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        // Check if the build target is iOS
        if (target != BuildTarget.iOS) return; // Not an iOS build

        // Step 1: Info.plist usage strings
        string plistPath = Path.Combine(path, "Info.plist"); // Path to the Info.plist file
        var plist = new PlistDocument(); // Create a new PlistDocument
        plist.ReadFromFile(plistPath); // Read the Info.plist file
        var root = plist.root; // Get the root dictionary

        // Add/update the usage descriptions your plugin needs:
        root.SetString("NSNearbyInteractionUsageDescription",
            "Used to perform precise ranging with nearby devices/beacons.");
        root.SetString("NSNearbyInteractionAllowOnceUsageDescription",
            "Used to perform precise ranging with nearby devices/beacons.");
        root.SetString("NSBluetoothAlwaysUsageDescription",
            "Bluetooth is required to communicate with nearby accessories.");
        root.SetString("NSCameraUsageDescription",
            "Camera is used by ARKit for spatial understanding.");

        // Write changes to Info.plist
        File.WriteAllText(plistPath, plist.WriteToString());
    }
}
#endif
