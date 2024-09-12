using UnityEngine;

/// <summary>
/// Documentation-only component for the Unity client.
///
/// The original research workflow used C++/OpenCV and Python tooling to
/// calculate and visualize reprojection errors. That evaluation code is kept
/// outside this mobile client so the portfolio project has a clear separation:
/// Unity handles capture/registration/visualisation, while the evaluation
/// pipeline handles quantitative localization validation.
/// </summary>
public class ReprojectionEvaluationNotes : MonoBehaviour
{
    [TextArea(4, 12)]
    public string workflow =
        "1. Obtain 3D scene points and camera pose data.\n" +
        "2. Project 3D points into the image using the camera model.\n" +
        "3. Compare projected points with reference 2D image points.\n" +
        "4. Calculate reprojection error statistics.\n" +
        "5. Visualise/record results using the separate evaluation tools.";
}
