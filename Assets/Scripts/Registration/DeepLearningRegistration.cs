using System;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Applies a camera pose returned by the server-side deep-learning
/// localization pipeline to a Unity AR scene.
///
/// The original research implementation was named EsacRegistration. For the
/// portfolio project it is presented as DeepLearningRegistration because the
/// client-side responsibility is registration from a deep-learning pose result.
/// The pose conversion logic is retained from the supplied implementation.
/// </summary>
public class DeepLearningRegistration : MonoBehaviour
{
    [Header("AR Scene")]
    [Tooltip("Object that should receive the estimated world pose.")]
    public GameObject targetObject;

    [Tooltip("Optional parent/alignment transform used by the original AR scene.")]
    public GameObject geoAlignment;

    [Header("Camera Reset")]
    [Tooltip("AR camera whose local transform is temporarily compensated during reset.")]
    public GameObject cam;

    [Tooltip("Optional transform used to undo the AR camera transform.")]
    public GameObject undoARCameraTransform;

    [Header("Pose State")]
    public bool isDone;

    /// <summary>Last position received from the processing server.</summary>
    public Vector3 PositionFromServer { get; private set; }

    /// <summary>Last rotation received from the processing server.</summary>
    public Quaternion RotationFromServer { get; private set; } = Quaternion.identity;

    /// <summary>The last raw pose response received from the server.</summary>
    public string LastPoseData { get; private set; }

    /// <summary>
    /// Returns the currently registered transform when a new pose has been
    /// marked as ready.
    /// </summary>
    public bool GetTransform(out Vector3 position, out Vector3 rotation)
    {
        position = targetObject != null ? targetObject.transform.position : Vector3.zero;
        rotation = targetObject != null
            ? targetObject.transform.rotation.eulerAngles
            : Vector3.zero;

        if (!isDone)
            return false;

        isDone = false;
        return true;
    }

    /// <summary>
    /// Restores the inverse camera transform used by the original workflow.
    /// </summary>
    public void Reset()
    {
        isDone = false;
        LastPoseData = null;
        PositionFromServer = Vector3.zero;
        RotationFromServer = Quaternion.identity;

        if (targetObject != null)
            targetObject.SetActive(false);

        if (undoARCameraTransform == null || cam == null)
            return;

        // Preserve the original research implementation's camera compensation.
        undoARCameraTransform.transform.rotation = Quaternion.Inverse(cam.transform.localRotation);

        Vector3 currentPosition = cam.transform.localPosition;
        undoARCameraTransform.transform.position = new Vector3(
            -currentPosition.x,
            -currentPosition.y,
            -currentPosition.z);
    }

    /// <summary>
    /// Parses the server response and converts the OpenCV pose convention into
    /// the Unity coordinate convention used by the original implementation.
    ///
    /// Expected format:
    /// imageName qw qx qy qz tx ty tz
    /// </summary>
    public bool LoadData(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            Debug.LogWarning("DeepLearningRegistration: empty pose response.");
            return false;
        }

        string[] poses = data.Trim().Split(new[] { ' ', '\t', '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries);

        if (poses.Length < 8)
        {
            Debug.LogError(
                "DeepLearningRegistration: expected at least 8 pose values " +
                "(imageName qw qx qy qz tx ty tz), but received " + poses.Length + ".");
            return false;
        }

        if (!TryParsePoseValue(poses[1], out float qw) ||
            !TryParsePoseValue(poses[2], out float qx) ||
            !TryParsePoseValue(poses[3], out float qy) ||
            !TryParsePoseValue(poses[4], out float qz) ||
            !TryParsePoseValue(poses[5], out float tx) ||
            !TryParsePoseValue(poses[6], out float ty) ||
            !TryParsePoseValue(poses[7], out float tz))
        {
            Debug.LogError("DeepLearningRegistration: pose contains invalid numeric values.");
            return false;
        }

        LastPoseData = data;

        // Convert the server/OpenCV coordinate convention to Unity.
        Vector3 position = new Vector3(tx, -ty, tz);
        Quaternion rotation = new Quaternion(-qy, qx, -qz, qw);

        PositionFromServer = position;
        RotationFromServer = rotation;

        Matrix4x4 poseMatrix = Matrix4x4.TRS(
            position,
            rotation,
            Vector3.one);

        ApplyPoseMatrix(poseMatrix);

        // A new pose is ready for any higher-level controller that uses the
        // original GetTransform/Place style workflow.
        isDone = true;

        Debug.Log("Deep-learning pose received: " + position + " / " + rotation);
        return true;
    }

    /// <summary>
    /// Marks the currently calculated pose as ready for scene placement.
    /// </summary>
    public void Place()
    {
        isDone = true;
        if (targetObject != null)
            targetObject.SetActive(true);
    }

    /// <summary>
    /// Applies a homogeneous transformation matrix to the AR target object.
    /// </summary>
    private void ApplyPoseMatrix(Matrix4x4 matrix)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("DeepLearningRegistration: targetObject is not assigned.");
            return;
        }

        targetObject.transform.position = matrix.GetColumn(3);
        targetObject.transform.rotation = Quaternion.LookRotation(
            matrix.GetColumn(2),
            matrix.GetColumn(1));
        targetObject.SetActive(true);

        if (geoAlignment != null)
            geoAlignment.SetActive(true);
    }

    private static bool TryParsePoseValue(string value, out float result)
    {
        return float.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out result);
    }
}
