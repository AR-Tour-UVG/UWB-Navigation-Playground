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
    // Import the native function to get coordinates from UWB
    // The function returns a pointer to a JSON string containing the coordinates
    [DllImport("UWBplugin")] private static extern IntPtr getCoords(); 

    // Method to retrieve the UWB position as a Vector3
    // Returns null if the position cannot be retrieved or parsed
    public static Vector3? GetUWBPosition()
    {
        // Check if the platform supports the native function (iOS only)
        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            Debug.LogWarning("UWB: getCoords() is only supported on iOS builds.");
            return null;
        }

        // Call the native function to get the coordinates
        IntPtr ptr = getCoords();
        if (ptr == IntPtr.Zero)
        {   
            // Handle null pointer case
            Debug.LogWarning("UWB: getCoords() returned null pointer.");
            return null;
        }

        // Convert the pointer to a string and parse it as JSON
        string json = Marshal.PtrToStringAnsi(ptr);
        if (string.IsNullOrEmpty(json) || json == "{}")
        {   
            // Handle empty or invalid JSON
            Debug.LogWarning("UWB: Invalid or empty JSON from getCoords().");
            return null;
        }

        // Deserialize the JSON to UWBPosition and convert to Vector3
        // Note: Unity's y-axis corresponds to the z-axis in the UWB system
        try
        {   
            // Deserialize the JSON string to UWBPosition struct
            UWBPosition pos = JsonUtility.FromJson<UWBPosition>(json);
            return new Vector3(pos.x, 0f, pos.y);  // y maps to z-axis in Unity
        }
        catch (Exception e)
        {
            // Handle JSON parsing errors
            Debug.LogError("UWB: JSON parse error - " + e.Message);
            return null;
        }
    }
}
