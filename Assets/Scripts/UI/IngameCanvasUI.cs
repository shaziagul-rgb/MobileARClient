using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main application controller for the portfolio version of the Unity AR client.
///
/// The original research UI handled several registration modes and testing
/// controls. This portfolio version keeps the part most relevant to the
/// deep-learning AR contribution: camera/image capture, server communication,
/// pose reception, registration, status reporting, and AR target visibility.
///
/// Workflow:
/// Camera -> ApiServices -> processing server -> pose response ->
/// DeepLearningRegistration -> Unity AR target.
/// </summary>
public class IngameCanvasUI : MonoBehaviour
{
    [Header("Core Components")]
    public ApiServices apiServices;
    public DeepLearningRegistration deepLearningRegistration;

    [Header("UI")]
    public Text statusText;
    public Text poseText;
    public Text processingTimeText;
    public Text serverText;
    public RawImage cameraPreview;

    [Header("AR Target")]
    public GameObject arTarget;
    public Renderer targetRenderer;

    [Header("Controls")]
    public Toggle showTargetToggle;
    public Button resetButton;
    public Button captureButton;
    public Button connectionButton;

    private float requestStartTime;
    private Texture2D lastCapturedImage;

    private void Start()
    {
        if (deepLearningRegistration != null)
            deepLearningRegistration.Reset();

        if (showTargetToggle != null)
        {
            showTargetToggle.isOn = true;
            showTargetToggle.onValueChanged.AddListener(OnEnableMeshValueChange);
        }

        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetClick);

        if (captureButton != null)
            captureButton.onClick.AddListener(OnTakePhoto);

        if (connectionButton != null)
            connectionButton.onClick.AddListener(CheckServerConnection);

        SetStatus("Ready. Capture an image to start deep-learning localization.");
    }

    /// <summary>
    /// Captures an image using the reusable WebCam component.
    /// </summary>
    public void OnTakePhoto()
    {
        WebCam webCam = FindObjectOfType<WebCam>();
        if (webCam == null)
        {
            SetStatus("WebCam component was not found in the scene.");
            return;
        }

        webCam.OnTakePhoto();
        SetStatus("Capturing camera image...");
    }

    /// <summary>
    /// Can be called by another camera component when it already has a texture.
    /// </summary>
    public void TakePhoto(Texture2D texture2D)
    {
        if (texture2D == null || apiServices == null)
        {
            SetStatus("Image capture or API service is unavailable.");
            return;
        }

        lastCapturedImage = texture2D;
        requestStartTime = Time.realtimeSinceStartup;

        if (cameraPreview != null)
            cameraPreview.texture = texture2D;

        if (deepLearningRegistration != null)
            deepLearningRegistration.Reset();

        SetStatus("Uploading image to the AR processing server...");
        apiServices.UploadImage(texture2D, OnImageUpdatedEvent);
    }

    /// <summary>
    /// Starts server-side processing after the image upload response.
    /// </summary>
    private void OnImageUpdatedEvent(string message)
    {
        SetStatus("Image uploaded. Starting deep-learning pose estimation...");
        StartCoroutine(RequestPoseAfterUpload());
    }

    private IEnumerator RequestPoseAfterUpload()
    {
        yield return new WaitForSeconds(0.5f);

        if (apiServices == null)
        {
            SetStatus("API service is unavailable.");
            yield break;
        }

        apiServices.RunBatchFileOnServer(OnSetPossesData);
    }

    /// <summary>
    /// Receives the pose returned by the server and applies it to the Unity AR target.
    /// </summary>
    private void OnSetPossesData(string data)
    {
        float elapsedMs = (Time.realtimeSinceStartup - requestStartTime) * 1000f;

        if (deepLearningRegistration == null)
        {
            SetStatus("DeepLearningRegistration is not assigned.");
            return;
        }

        if (!deepLearningRegistration.LoadData(data))
        {
            SetStatus("The server returned an invalid pose response.");
            if (poseText != null)
                poseText.text = data;
            return;
        }

        if (poseText != null)
        {
            poseText.text =
                "Position: " + deepLearningRegistration.PositionFromServer + "\n" +
                "Rotation: " + deepLearningRegistration.RotationFromServer.eulerAngles;
        }

        if (processingTimeText != null)
            processingTimeText.text = "Client processing time: " + elapsedMs.ToString("F0") + " ms";

        SetStatus("Pose received. AR target registered successfully.");
        SetTargetVisible(true);
    }

    /// <summary>
    /// Checks the health endpoint of the configured processing server.
    /// </summary>
    public void CheckServerConnection()
    {
        if (apiServices == null)
        {
            SetStatus("API service is unavailable.");
            return;
        }

        SetStatus("Checking AR processing server...");
        apiServices.IsConnectedToSever(OnConnectionResult);
    }

    private void OnConnectionResult(bool connected)
    {
        if (serverText != null)
            serverText.text = connected ? "Server: Connected" : "Server: Not reachable";

        SetStatus(connected
            ? "Processing server is reachable."
            : "Processing server could not be reached.");
    }

    /// <summary>
    /// Shows or hides the registered AR model.
    /// </summary>
    public void OnEnableMeshValueChange(bool enabled)
    {
        SetTargetVisible(enabled);
    }

    public void HideShowMesh()
    {
        bool visible = arTarget == null || !arTarget.activeSelf;
        SetTargetVisible(visible);
    }

    /// <summary>
    /// Resets the client-side pose and AR target state.
    /// </summary>
    public void OnResetClick()
    {
        if (deepLearningRegistration != null)
            deepLearningRegistration.Reset();

        SetTargetVisible(false);

        if (poseText != null)
            poseText.text = string.Empty;
        if (processingTimeText != null)
            processingTimeText.text = string.Empty;

        SetStatus("AR registration reset. Ready for a new image.");
    }

    public void ResetPress()
    {
        OnResetClick();
    }

    public void BackPress()
    {
        if (SceneManager.sceneCountInBuildSettings > 1)
            SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Reprojection evaluation is intentionally kept as a separate research
    /// evaluation stage. This method exposes the UI hook without embedding the
    /// original C++/OpenCV evaluator into the Unity client.
    /// </summary>
    public void OnCalculateBtnClick()
    {
        SetStatus("Reprojection evaluation is performed by the evaluation pipeline.");
    }

    public void OnReprojTestBtnClick()
    {
        SetStatus("Reprojection testing mode selected.");
    }

    private void SetTargetVisible(bool visible)
    {
        if (arTarget != null)
            arTarget.SetActive(visible);

        if (targetRenderer != null)
            targetRenderer.enabled = visible;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log("ARSpecClient: " + message);
    }

    private void OnDestroy()
    {
        if (showTargetToggle != null)
            showTargetToggle.onValueChanged.RemoveListener(OnEnableMeshValueChange);
        if (resetButton != null)
            resetButton.onClick.RemoveListener(OnResetClick);
        if (captureButton != null)
            captureButton.onClick.RemoveListener(OnTakePhoto);
        if (connectionButton != null)
            connectionButton.onClick.RemoveListener(CheckServerConnection);
    }
}
