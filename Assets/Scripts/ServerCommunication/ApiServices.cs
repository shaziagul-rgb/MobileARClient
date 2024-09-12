using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Provides the Unity client with a small HTTP API layer for communicating
/// with the server-assisted deep-learning camera pose estimation pipeline.
///
/// Typical workflow:
/// 1. Upload a camera image.
/// 2. Ask the server to start its processing pipeline.
/// 3. Receive the returned pose/result data.
/// </summary>
public class ApiServices : MonoBehaviour
{
    // The URL is intentionally configurable rather than containing the
    // original private research-network address.
    [SerializeField]
    private string hostURL = "http://localhost:3001/";

    // Callbacks allow UI/camera components to receive asynchronous results
    // without needing to know how HTTP requests are implemented.
    private Action<string> onPoseDataReadAction;
    private Action<bool> isConnectedAction;
    private Action<string> onImageUploaded;

    /// <summary>
    /// Public access to the configured processing-server URL.
    /// </summary>
    public string HostURL
    {
        get => hostURL;
        set => hostURL = NormalizeUrl(value);
    }

    private void Awake()
    {
        // Normalize the Inspector value once when the component starts.
        hostURL = NormalizeUrl(hostURL);
    }

    /// <summary>
    /// Requests execution of the server-side processing pipeline.
    /// </summary>
    public void RunBatchFileOnServer(Action<string> onPoseDataReadAction)
    {
        this.onPoseDataReadAction = onPoseDataReadAction;
        StartCoroutine(RunBatchFile());
    }

    /// <summary>
    /// Encodes a captured image and uploads it to the processing server.
    /// </summary>
    public void UploadImage(Texture2D texture, Action<string> onImageUploaded)
    {
        this.onImageUploaded = onImageUploaded;
        StartCoroutine(SendImage(texture));
    }

    /// <summary>
    /// Retrieves pose data from the server's pose-data endpoint.
    /// </summary>
    public void GetPossesFile(Action<string> onPoseDataReadAction)
    {
        this.onPoseDataReadAction = onPoseDataReadAction;
        StartCoroutine(ReadPosesFile());
    }

    /// <summary>
    /// Requests creation of any server-side folders required by the pipeline.
    /// </summary>
    public void CreateFolderOnServer()
    {
        StartCoroutine(CreateFolders());
    }

    /// <summary>
    /// Tests whether the configured server is reachable.
    /// </summary>
    public void IsConnectedToSever(Action<bool> isConnected)
    {
        isConnectedAction = isConnected;
        StartCoroutine(CheckConnection());
    }

    private IEnumerator CheckConnection()
    {
        // A GET request is used as a lightweight server health/connection test.
        using (UnityWebRequest request = UnityWebRequest.Get(BuildUrl("checkConnection")))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                isConnectedAction?.Invoke(false);
            }
            else
            {
                isConnectedAction?.Invoke(true);
                Debug.Log(request.downloadHandler.text);
            }
        }
    }

    private IEnumerator RunBatchFile()
    {
        Debug.Log("Deep-learning processing requested.");

        // POST tells the server to start its processing workflow using the
        // image/data that has already been uploaded.
        using (UnityWebRequest request = new UnityWebRequest(BuildUrl("runBatchFile"), "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "text/plain");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                onPoseDataReadAction?.Invoke(request.error);
            }
            else
            {
                // Pass the server response back to the Unity component that
                // initiated the request.
                onPoseDataReadAction?.Invoke(request.downloadHandler.text);
                Debug.Log(request.downloadHandler.text);
            }
        }
    }

    private IEnumerator SendImage(Texture2D texture)
    {
        if (texture == null)
        {
            onImageUploaded?.Invoke("No image was provided.");
            yield break;
        }

        // Convert the Unity texture into JPEG bytes for network transfer.
        byte[] imageData = texture.EncodeToJPG();

        using (UnityWebRequest request = new UnityWebRequest(BuildUrl("upload-image"), "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(imageData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "image/jpeg");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onImageUploaded?.Invoke(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                onImageUploaded?.Invoke(request.downloadHandler.text);
            }
        }
    }

    private IEnumerator ReadPosesFile()
    {
        // Keep the original server-assisted workflow's short wait before
        // requesting the generated pose data.
        yield return new WaitForSeconds(1f);

        Debug.Log("Pose data requested.");

        using (UnityWebRequest request = UnityWebRequest.Get(BuildUrl("readPosesfile")))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
            }
            else
            {
                onPoseDataReadAction?.Invoke(request.downloadHandler.text);
                Debug.Log(request.downloadHandler.text);
            }
        }
    }

    private IEnumerator CreateFolders()
    {
        // Ask the processing server to create its required working folders.
        using (UnityWebRequest request = new UnityWebRequest(BuildUrl("createfolder"), "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "text/plain");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
            }
        }
    }

    /// <summary>
    /// Convenience method for retrieving the current pose response.
    /// </summary>
    public void GetResponse(Action<string> onPoseDataReadAction)
    {
        this.onPoseDataReadAction = onPoseDataReadAction;
        StartCoroutine(ReadPosesFile());
    }

    /// <summary>
    /// Builds an endpoint URL from the configured server address.
    /// </summary>
    private string BuildUrl(string endpoint)
    {
        return NormalizeUrl(hostURL) + endpoint.TrimStart('/');
    }

    /// <summary>
    /// Ensures the server URL has exactly one trailing slash.
    /// </summary>
    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return "http://localhost:3001/";

        return url.Trim().TrimEnd('/') + "/";
    }
}
