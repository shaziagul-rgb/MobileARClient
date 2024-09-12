using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Captures a still image from the device camera and sends it to the
/// server-side deep-learning camera-pose estimation pipeline.
///
/// The client is responsible for image capture and communication; the
/// computationally intensive pose estimation is performed by the server.
/// </summary>
public class WebCam : MonoBehaviour
{
    // UI elements used to show the captured image and processing messages.
    public Image display;
    public Text contentDataTxt;
    public Text msgTxt;

    // Reference to the component responsible for HTTP communication
    // with the AR processing server.
    public ApiServices apiServices;

    // Unity's camera texture provides live frames from the device camera.
    private WebCamTexture webCamTexture;

    private void Awake()
    {
        // Clear previous status/results when this scene is loaded.
        if (msgTxt != null)
            msgTxt.text = string.Empty;

        if (contentDataTxt != null)
            contentDataTxt.text = string.Empty;
    }

    private void Start()
    {
        // Keep the UI clean when the camera component starts.
        if (msgTxt != null)
            msgTxt.text = string.Empty;

        if (contentDataTxt != null)
            contentDataTxt.text = string.Empty;
    }

    /// <summary>
    /// Starts the asynchronous camera-capture workflow.
    /// </summary>
    public void OnTakePhoto()
    {
        StartCoroutine(CapturePhoto());
    }

    /// <summary>
    /// Starts the device camera, waits until a frame is available, and copies
    /// the current camera frame into a Texture2D for server processing.
    /// </summary>
    private IEnumerator CapturePhoto()
    {
        if (webCamTexture == null)
        {
            // Create the camera stream only once and wait until Unity has
            // received enough information to provide a valid frame.
            webCamTexture = new WebCamTexture();
            webCamTexture.Play();
            yield return new WaitUntil(() => webCamTexture.width > 16);
        }

        // Wait for the current Unity frame to finish before reading pixels.
        yield return new WaitForEndOfFrame();

        // Copy the live camera frame into a standalone texture that can be
        // encoded and uploaded by ApiServices.
        Texture2D photo = new Texture2D(
            webCamTexture.width,
            webCamTexture.height,
            TextureFormat.RGB24,
            false);

        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();

        LoadImageCompleted(photo);
    }

    /// <summary>
    /// Displays the captured image and starts the client/server processing flow.
    /// </summary>
    public void LoadImageCompleted(Texture2D texture2D)
    {
        if (display != null)
        {
            // Convert the Texture2D into a Unity UI sprite for preview.
            display.sprite = Sprite.Create(
                texture2D,
                new Rect(0, 0, texture2D.width, texture2D.height),
                new Vector2(0.5f, 0.5f));
        }

        if (msgTxt != null)
            msgTxt.text = "Image captured. Sending to server...";

        // Prefer the application-level controller so the same capture can be
        // used by the portfolio UI and the registration component.
        IngameCanvasUI controller = FindObjectOfType<IngameCanvasUI>();
        if (controller != null)
        {
            controller.TakePhoto(texture2D);
            return;
        }

        // Standalone fallback: upload and process directly.
        apiServices.UploadImage(texture2D, OnImageUpdatedEvent);
        StartCoroutine(RequestPoseAfterUpload());
    }

    private IEnumerator RequestPoseAfterUpload()
    {
        yield return new WaitForSeconds(1f);

        // Trigger the server-side processing pipeline. The returned pose/result
        // is delivered to OnSetPossesData through the callback.
        apiServices.RunBatchFileOnServer(OnSetPossesData);
    }

    /// <summary>
    /// Receives the server's upload response and displays it in the UI.
    /// </summary>
    private void OnImageUpdatedEvent(string msg)
    {
        if (msgTxt != null)
            msgTxt.text = msg;
    }

    /// <summary>
    /// Receives the server-side processing/pose response.
    /// </summary>
    private void OnSetPossesData(string data)
    {
        if (contentDataTxt != null)
            contentDataTxt.text = data;
    }

    private void OnDestroy()
    {
        // Stop the camera stream when the Unity object is destroyed.
        if (webCamTexture != null && webCamTexture.isPlaying)
            webCamTexture.Stop();
    }
}
