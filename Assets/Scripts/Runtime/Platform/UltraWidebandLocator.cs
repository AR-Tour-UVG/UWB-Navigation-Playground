using System;
using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a 2D position in Unity's space.
/// </summary>
[Serializable]
public struct Coordinate
{
    public float x; // x-coordinate in Unity's space
    public float y; // y-coordinate in Unity's space
}

/// <summary>
/// Provides access to Ultra Wideband (UWB) native plugin and maps them to Unity coordinates.
/// </summary>
public static class UltraWidebandLocator
{
    // Log-once guard for non-iOS/editor runs
    private static bool warned = false;

    // Check if the platform is iOS and import the required native functions
#if UNITY_IOS && !UNITY_EDITOR
        // Returns pointer to a null-terminated JSON string allocated with strdup (must be freed).
        [DllImport("__Internal")] private static extern IntPtr getCoords();

        // Frees the JSON string allocated by getCoords().
        [DllImport("__Internal")] private static extern void freeCString(IntPtr ptr);
#else
    // Stubs implementation for non-iOS platforms
    private static IntPtr getCoords() => IntPtr.Zero; // Always returns null pointer
    private static void freeCString(IntPtr ptr) { } // No-op for non-iOS platforms
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
        if (!warned)
        {
            Debug.LogWarning("UWB: Real time positioning is supported only on iOS device builds.");
            warned = true; // Set the flag to true after the first warning
        }
        return false; // Not running on iOS, return false
#else
        IntPtr coordsPtr = getCoords(); // Call the native function to get the coordinates
        // Validate the pointer 
        if (coordsPtr == IntPtr.Zero)
        {
            Debug.LogWarning("UWB: getCoords() returned null pointer.");
            return false; // Failed to get coordinates
        }

        // Try to extract coordinate values from JSON
        try
        {
            // Read the JSON string from the pointer
            string json = Marshal.PtrToStringAnsi(coordsPtr);
            Debug.Log($"UWB: JSON from plugin: {json}");

            // Filter invalid JSON or null coordinate cases
            if (string.IsNullOrEmpty(json) || json == "{}" || json.Contains("null"))
            {
                Debug.LogWarning("UWB: Received invalid JSON or null coordinates from UWB plugin.");
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
            Debug.LogError($"UWB: Failed to parse UWB position JSON. - {ex.Message}");
            return false; // Failed to parse JSON
        }
        finally
        {
            // Always free the allocated string
            freeCString(coordsPtr); 
        }
#endif
    }
}