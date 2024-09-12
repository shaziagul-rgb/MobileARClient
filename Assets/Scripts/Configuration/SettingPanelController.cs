using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides a simple Unity UI for configuring the address of the
/// server-side AR/deep-learning processing service.
/// </summary>
public class SettingPanelController : MonoBehaviour
{
    // Input field containing the server URL entered by the user.
    public InputField inputField;

    // Client-side API component whose endpoint is updated by this panel.
    public ApiServices apiServices;

    /// <summary>
    /// Reads the server URL from the UI and applies it to ApiServices.
    /// </summary>
    public void OnSaveButtonClicked()
    {
        // Do not update the configuration when the required references or
        // the URL field are missing.
        if (inputField == null || apiServices == null || string.IsNullOrWhiteSpace(inputField.text))
            return;

        // ApiServices normalizes the URL so endpoint paths can be appended
        // consistently throughout the application.
        apiServices.HostURL = inputField.text;

        Debug.Log("Server URL updated: " + apiServices.HostURL);
    }
}
