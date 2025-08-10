using System;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
public struct UWBPosition
{
    public float x; // x-coordinate in Unity's space
    public float y; // y-coordinate in Unity's space
}

// This script bridges Unity with the UWB system to retrieve position data
public static class UWBBridge
{
// Check if the platform is iOS and import the required native functions
    #if UNITY_IOS && !UNITY_EDITOR
        // Import the native function from the iOS plugin
        // The function returns a pointer to a JSON string containing the coordinates
        [DllImport("__Internal")] 
        private static extern IntPtr getCoords();

        // Import the native function to free the allocated string
        // The function takes a pointer to the string to be freed
        [DllImport("__Internal")]
        private static extern void freeCString(IntPtr ptr);
    #else
        // Stub implementation for non-iOS platforms
        private static IntPtr getCoords() => IntPtr.Zero; // Always returns null pointer
        private static void freeCString(IntPtr ptr) { } // No-op for non-iOS platforms
        private static bool warned = false; // Flag to indicate if the warning has been shown
    #endif

    /// <summary>
    /// Attempts to retrieve the latest UWB position from the native plugin.
    /// If successful, the position will be returned transformed into Unity's coordinate space.
    /// </summary>
    /// <returns>A Vector3 representing the UWB position, or null if the position cannot be retrieved or parsed.</returns>
    public static bool GetUWBPosition(out Vector3 position)
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
            Debug.Log($"UWB: Received JSON from UWB plugin: {json}");

            // Filter invalid JSON or null coordinate cases
            if (string.IsNullOrEmpty(json) || json == "{}" || json.Contains("null"))
            {
                Debug.LogWarning("UWB: Received invalid JSON or null coordinates from UWB plugin.");
                return false; // Invalid JSON or null coordinates
            }
            
            // Parse the JSON string into a UWBPosition object
            UWBPosition uwbPosition = JsonUtility.FromJson<UWBPosition>(json);
            // Map plugin X -> Unity X, plugin Y -> Unity Z, ignore plugin Z -> Unity Y
            position = new Vector3(uwbPosition.x, float.NaN, uwbPosition.y);
            return true; // Successfully retrieved and parsed position
            }
            catch (Exception ex)
            {
                Debug.LogError($"UWB: Failed to parse UWB position JSON. - {ex.Message}");
                return false; // Failed to parse JSON
            }
            finally
            {
                freeCString(coordsPtr); // Always free the allocated string
            }
        #endif
    }
}