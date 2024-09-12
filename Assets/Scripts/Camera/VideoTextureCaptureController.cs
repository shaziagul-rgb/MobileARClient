using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Extracts selected frames from a Unity VideoPlayer and sends them to the
/// server-side deep-learning processing pipeline.
/// </summary>
public class VideoTextureCaptureController : MonoBehaviour
{
    // UI and video components configured through the Unity Inspector.
    public RawImage display;
    public AspectRatioFitter imageFitter;
    public Text msgText;
    public Text startStopText;
    public RenderTexture renderTexture;
    public Text debug;
    public VideoPlayer videoPlayer;
    public RawImage image;
    public Text contentDataTxt;

    // Handles image upload and server-side processing requests.
    public ApiServices apiServices;

    // Set to true when the next available video frame should be captured.
    private bool captureImage;

    private void Awake()
    {
        // Clear status/result text when the component is created.
        if (msgText != null)
            msgText.text = string.Empty;

        if (contentDataTxt != null)
            contentDataTxt.text = string.Empty;
    }

    private void Start()
    {
        // Configure the VideoPlayer to render into a RenderTexture.
        videoPlayer.Stop();
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // Frame-ready events allow the application to copy an individual
        // video frame into a Texture2D when requested by the user.
        videoPlayer.sendFrameReadyEvents = true;
        videoPlayer.frameReady += FrameReady;
        videoPlayer.loopPointReached += EndReached;
        videoPlayer.Prepare();
    }

    private void EndReached(VideoPlayer vp)
    {
        // No more frames should be captured after the video has finished.
        captureImage = false;

        // Start the server-side processing once video playback ends.
        StartCoroutine(StartBatch());
    }

    private IEnumerator StartBatch()
    {
        // Preserve a short processing interval before requesting the result.
        yield return new WaitForSeconds(1f);
        apiServices.RunBatchFileOnServer(OnSetPossesData);
    }

    /// <summary>
    /// Called by Unity whenever a new video frame is available.
    /// Only captures a frame when the user has requested one.
    /// </summary>
    private void FrameReady(VideoPlayer vp, long frameIndex)
    {
        if (!captureImage)
            return;

        // Reset immediately so only one frame is captured per request.
        captureImage = false;

        // Convert the VideoPlayer texture into a Texture2D suitable for upload.
        Texture2D frame = TextureToTexture2D(vp.texture);

        if (msgText != null)
            msgText.text = "Frame captured";

        apiServices.UploadImage(frame, OnImageUpdatedEvent);
    }

    /// <summary>
    /// Requests that the next available video frame be captured.
    /// </summary>
    public void OnCaptureImageButtonClicked()
    {
        captureImage = true;
    }

    private void OnImageUpdatedEvent(string msg)
    {
        if (msgText != null)
            msgText.text = msg;
    }

    /// <summary>
    /// Starts video playback from the Unity UI.
    /// </summary>
    public void OnVideoStartBtnClicked()
    {
        videoPlayer.Play();
    }

    /// <summary>
    /// Stops playback and starts the server-side processing request.
    /// </summary>
    public void OnStopButtonClicked()
    {
        videoPlayer.Stop();
        StartCoroutine(StartBatch());
    }

    /// <summary>
    /// Copies a GPU-backed Unity texture into a CPU-readable Texture2D.
    /// This is required before the image can be encoded and uploaded.
    /// </summary>
    private Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(
            texture.width,
            texture.height,
            TextureFormat.RGBA32,
            false);

        // Preserve the caller's active RenderTexture before temporarily
        // switching to an intermediate render target.
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture temporaryRT = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            32);

        // Copy the source texture to a readable temporary render target.
        Graphics.Blit(texture, temporaryRT);
        RenderTexture.active = temporaryRT;

        // Read the pixels from the GPU target into the Texture2D.
        texture2D.ReadPixels(
            new Rect(0, 0, temporaryRT.width, temporaryRT.height),
            0,
            0);
        texture2D.Apply();

        // Restore Unity's previous render target and release the temporary one.
        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(temporaryRT);

        return texture2D;
    }

    /// <summary>
    /// Receives the returned processing/pose data from ApiServices.
    /// </summary>
    private void OnSetPossesData(string data)
    {
        if (contentDataTxt != null)
            contentDataTxt.text = data;
    }

    private void OnDestroy()
    {
        // Unsubscribe from VideoPlayer events to avoid callbacks after
        // this component has been destroyed.
        if (videoPlayer == null)
            return;

        videoPlayer.frameReady -= FrameReady;
        videoPlayer.loopPointReached -= EndReached;
    }
}
