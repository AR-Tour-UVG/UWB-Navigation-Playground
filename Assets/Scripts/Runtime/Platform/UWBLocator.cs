using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;


// ----- Data Structures -----
/// <summary>
/// Represents a 2D position in Unity's space.
/// </summary>
[Serializable]
public struct Coordinate
{
    public float x; // x-coordinate in Unity's space
    public float y; // y-coordinate in Unity's space
}

// ----- Native Bridge -----
/// <summary>
/// Provides access to Ultra Wideband (UWB) native plugin and maps them to Unity coordinates.
/// </summary>
public static class UWBLocator
{
    // Log-once guard for non-iOS/editor runs
    private static bool hasWarned = false;
    private static bool isInitialized = false;

    // Check if the platform is iOS and import the required native functions
#if UNITY_IOS && !UNITY_EDITOR
        // Returns pointer to a null-terminated JSON string allocated with strdup (must be freed).
        [DllImport("__Internal")] private static extern IntPtr getCoords();

        // Frees the JSON string allocated by getCoords().
        [DllImport("__Internal")] private static extern void freeCString(IntPtr ptr);

        // Sets the anchor map in the native plugin.
        [DllImport("__Internal")] private static extern void setAnchorMap(string jsonUtf8);

        // Configure plugin to use uwb anchor map
        [DllImport("__Internal", EntryPoint = "start")] private static extern void uwb_start();

#else
    // Stubs implementation for non-iOS platforms
    private static IntPtr getCoords() => IntPtr.Zero; // Always returns null pointer
    private static void freeCString(IntPtr ptr) { } // No-op for non-iOS platforms
    private static void setAnchorMap(string jsonUtf8) { } // No-op for non-iOS platforms
    private static void uwb_start() { } // No-op for non-iOS platforms

#endif

    /// <summary>
    /// Attempts to retrieve the latest UWB position from the native plugin.
    /// If successful, the position will be returned transformed into Unity's coordinate space.
    /// </summary>
    /// <param name="position">The retrieved UWB position in Unity's coordinate space.</param>
    /// <returns>True if the position was successfully retrieved and parsed; otherwise, false.</returns>
    public static bool TryGetPosition(out Vector3 position)
    {
        position = default; // Initialize position

        // Check if the platform is iOS before attempting to retrieve the position
#if !UNITY_IOS || UNITY_EDITOR
        // Warn only the firts time it runs on non iOS device
        if (!hasWarned)
        {
            Debug.LogWarning("UWBLocator: Real time positioning is supported only on iOS device builds.");
            hasWarned = true; // Set the flag to true after the first warning
        }
        return false; // Not running on iOS, return false
#else
        IntPtr coordsPtr = getCoords(); // Call the native function to get the coordinates
        // Validate the pointer 
        if (coordsPtr == IntPtr.Zero)
        {
            Debug.LogWarning("UWBLocator: getCoords() returned null pointer.");
            return false; // Failed to get coordinates
        }

        // Try to extract coordinate values from JSON
        try
        {
            // Read the JSON string from the pointer
            string json = Marshal.PtrToStringAnsi(coordsPtr);
            Debug.Log($"UWBLocator: JSON from plugin: {json}");

            // Filter invalid JSON or null coordinate cases
            if (string.IsNullOrEmpty(json) || json == "{}" || json.Contains("null"))
            {
                Debug.LogWarning("UWBLocator: Received invalid JSON or null coordinates from UWB plugin.");
                return false; // Invalid JSON or null coordinates
            }

            // Parse the JSON string into a Coordinate object
            Coordinate uwbPosition = JsonUtility.FromJson<Coordinate>(json);
            // Map plugin X -> Unity X, plugin Y -> Unity Z, ignore plugin Z -> Unity Y
            position = new Vector3(uwbPosition.x, 0f, uwbPosition.y);
            return true; // Successfully retrieved and parsed position
        }
        catch (Exception ex)
        {
            Debug.LogError($"UWBLocator: Failed to parse UWB position JSON. - {ex.Message}");
            return false; // Failed to parse JSON
        }
        finally
        {
            // Always free the allocated string
            freeCString(coordsPtr); 
        }
#endif
    }


/// <summary>
/// Initializes the anchor map for the UWB plugin.
/// </summary>
/// <param name="anchorMap">The anchor map JSON string.</param>
    public static void InitializeAnchorMap(string anchorMap)
    {
        if (isInitialized)
        {
            // Return early if already initialized
            return;
        }

        // Check if the platform is iOS before attempting to initialize plugin
#if !UNITY_IOS || UNITY_EDITOR

        Debug.LogWarning("UWBLocator: Anchor map initialization is supported only on iOS device builds.");
        hasWarned = true; // Set the flag to true after the first warning
        // Set initialized flag
        return;
#else   
        // Check if the anchor map is null or empty
        if (string.IsNullOrWhiteSpace(anchorMap))
        {
            Debug.LogWarning("UWBLocator: Anchor map is null or empty.");
            return;
        }

        try
        {
            setAnchorMap(anchorMap); // Set the anchor map
            uwb_start(); // Start the UWB plugin
            isInitialized = true;
            Debug.Log($"UWBLocator: Plugin initialized with {anchors?.Count ?? 0} anchors.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"UWBLocator: Error initializing plugin: {ex.Message}");
            isInitialized = false;
        }
#endif
    }
}