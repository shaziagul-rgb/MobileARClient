using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Checks whether the Unity client can reach the configured processing server
/// and reports the result to the user.
/// </summary>
public class connectionTestController : MonoBehaviour
{
    // UI label used to show the connection state.
    public Text connectTxt;

    // API client used to send the connection test request.
    public ApiServices apiServices;

    private void Start()
    {
        if (connectTxt != null)
            connectTxt.text = "Connecting to the server...";

        // Send a lightweight request to the server before image processing.
        if (apiServices == null)
        {
            OnConnectTestAction(false);
            return;
        }

        apiServices.IsConnectedToSever(OnConnectTestAction);
    }

    /// <summary>
    /// Updates the UI after the server connection test completes.
    /// </summary>
    public void OnConnectTestAction(bool isConnect)
    {
        if (connectTxt != null)
        {
            connectTxt.text = isConnect
                ? "Successfully connected to the server"
                : "Please connect to the server";
        }
    }
}
