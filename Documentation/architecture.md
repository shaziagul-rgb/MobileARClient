# ARSpecClient Architecture

## 1. System overview

ARSpecClient is the Unity/mobile client of a server-assisted deep-learning localization system.

```text
Camera
  |
  v
Unity capture
  |
  v
IngameCanvasUI
  |
  v
ApiServices ----------------------+
  |                              |
  | HTTP                         |
  v                              |
AR Processing Server             |
  |                              |
  v                              |
Deep-learning localization       |
  |                              |
  +---- estimated 6-DoF pose ---+
                 |
                 v
     DeepLearningRegistration
                 |
                 v
            AR target
```

## 2. Client responsibilities

The Unity client is responsible for:

- image acquisition
- image conversion
- API requests
- server configuration
- connection status
- receiving pose data
- coordinate conversion
- applying the pose to an AR object
- visual feedback

## 3. Server responsibilities

The server-side project is responsible for the computationally intensive processing:

- image handling
- model execution
- deep-learning camera localization
- generation of the camera pose response

## 4. Registration

The supplied research implementation parsed a pose containing quaternion and translation values and converted it from the computer-vision/OpenCV convention into Unity's coordinate convention.

The portfolio class is named `DeepLearningRegistration` to describe its architectural responsibility rather than make a particular estimator the headline of the repository.

## 5. Evaluation

Localization evaluation is kept conceptually separate from mobile application logic.

The broader research workflow used 3D points, 2D image points, camera intrinsics, and estimated poses to calculate reprojection error. The saved project material also describes C++/Python/OpenCV evaluation tooling for this purpose.

This separation is useful for a portfolio because it demonstrates both:

- production-style client/server integration, and
- research-oriented quantitative validation.
