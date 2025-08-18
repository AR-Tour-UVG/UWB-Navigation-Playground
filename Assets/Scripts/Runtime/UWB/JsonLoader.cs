using System.IO;
using UnityEngine;

public class JsonLoader : MonoBehaviour
{
    [Header("Anchor Map JSON")]
    [Tooltip("Filename of the JSON file containing the anchor map.")]
    public string jsonFile = "testRoom.json";

    void Awake()
    {
        // Load the JSON file from Resources/AnchorMaps
        TextAsset jsonAsset = Resources.Load<TextAsset>($"AnchorMaps/{jsonFile}");
        if (jsonAsset == null)
        {
            Debug.LogError($"JsonLoader: Failed to load JSON file: {jsonFile}");
            return;
        }

        // Initialize the plugin with the JSON content
        UWBLocator.InitializeAnchorMap(jsonAsset.text);

    }
}