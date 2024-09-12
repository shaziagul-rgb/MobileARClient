# MobileARClient

## Unity AR Client for Server-Assisted Deep-Learning Pose Estimation

**MobileARClient** is a Unity/C# client application that captures camera imagery, communicates with a server-side computer-vision/deep-learning pipeline through REST-style APIs, receives an estimated 6-DoF camera pose, and applies that pose to an AR scene.

The portfolio version is intentionally presented as a **general deep-learning-based AR client** rather than being tied to a specific localization model or algorithm.

### Languages & Technologies

- **C#**
- **Unity 3D**
- HTTP/REST API communication
- Mobile camera capture
- 6-DoF camera pose estimation and registration
- OpenCV-to-Unity coordinate conversion
- Server-assisted deep-learning inference

## What this project demonstrates

- Unity application development in C#
- Camera image capture using Unity's `WebCamTexture`
- Client-server communication through HTTP/REST-style APIs
- Uploading camera imagery for server-side processing
- Triggering a deep-learning localization pipeline remotely
- Receiving and validating camera pose data
- OpenCV-to-Unity coordinate-system conversion
- 6-DoF AR scene registration
- Runtime processing-server configuration
- Server connection testing
- Separation of the mobile/AR client from computationally intensive inference
- Separation of pose estimation from quantitative reprojection-error evaluation

## Architecture

```text
                    ARSpecClient (Unity / C#)
                              |
                    +---------+---------+
                    |                   |
               WebCam.cs        ARClientController
                    |                   |
                    +---------+---------+
                              |
                         ApiServices
                              |
                         HTTP / REST
                              |
                              v
                    AR Processing Server
                              |
                  Deep-Learning Inference
                              |
                           6-DoF Pose
                              |
                              v
                  DeepLearningRegistration
                              |
                              v
                       Unity AR Target
                              |
                              v
                 Separate Evaluation Pipeline
                    (Reprojection Error)
```

The Unity client is responsible for capture, communication, pose handling, and AR registration. The deep-learning model and quantitative reprojection evaluation remain separate components so that they can be developed and evaluated independently.

## Repository structure

```text
ARSpecClient/
├── Assets/
│   ├── Scripts/
│   │   ├── ARClient/
│   │   │   └── ARClientController.cs
│   │   ├── Camera/
│   │   │   ├── WebCam.cs
│   │   │   └── VideoTextureCaptureController.cs
│   │   ├── Configuration/
│   │   │   └── SettingPanelController.cs
│   │   ├── Evaluation/
│   │   │   └── ReprojectionEvaluationNotes.cs
│   │   ├── Registration/
│   │   │   └── DeepLearningRegistration.cs
│   │   └── ServerCommunication/
│   │       ├── ApiServices.cs
│   │       └── connectionTestController.cs
│   └── Scenes/
│       └── PortfolioDemo.unity
├── Documentation/
│   ├── architecture.md
│   ├── workflow.md
│   └── source-notes.md
├── Packages/
├── ProjectSettings/
├── .gitignore
└── README.md
```

## Core components

### `ARClientController.cs`

The main application-level controller for the portfolio client. It coordinates the end-to-end Unity workflow:

1. Start camera capture.
2. Receive a captured `Texture2D`.
3. Upload the image through `ApiServices`.
4. Request server-side deep-learning processing.
5. Receive the estimated camera pose.
6. Pass the pose to `DeepLearningRegistration`.
7. Display pose/status information.
8. Show the registered AR target.
9. Reset the registration when required.

The controller also provides UI hooks for server connection testing, target visibility, reset, and reprojection-testing status. The actual reprojection calculation is deliberately kept outside the Unity client.

### `WebCam.cs`

Provides camera capture using Unity's native `WebCamTexture`. Captured frames can be passed to `ARClientController` for the server-assisted localization workflow.

### `VideoTextureCaptureController.cs`

Provides reusable camera/video texture handling for the Unity client and supports displaying the camera stream within the application.

### `ApiServices.cs`

Provides the HTTP interface to the processing server. The client currently uses endpoints for:

- `GET /checkConnection`
- `POST /upload-image`
- `POST /runBatchFile`
- `GET /readPosesfile`
- `POST /createfolder`

The server URL is configurable. The public portfolio project uses `http://localhost:3001/` as the default rather than exposing the original private research-network address.

### `DeepLearningRegistration.cs`

The registration component for applying the server's deep-learning pose result to the Unity AR scene.

- Parses the returned pose values.
- Validates numeric pose data.
- Converts the server/OpenCV coordinate convention into Unity coordinates.
- Constructs a Unity transformation matrix.
- Applies position and orientation to the AR target.
- Exposes the received position and rotation for the client UI.

Expected pose format:

```text
imageName qw qx qy qz tx ty tz
```

Example:

```text
37.jpg 0.966442 0.029568 -0.255020 0.008909 2.230467 0.271206 2.978533
```

The coordinate conversion used by the client is based on the supplied implementation:

```text
Unity position  = (tx, -ty, tz)
Unity quaternion = (-qy, qx, -qz, qw)
```

### `SettingPanelController.cs`

Handles client configuration UI used to expose runtime settings without hard-coding environment-specific values into the application.

### `connectionTestController.cs`

Provides a small connection-testing component for checking whether the configured processing server is reachable.

### `ReprojectionEvaluationNotes.cs`

Documents the boundary between the Unity client and the separate reprojection-error evaluation module. It does **not** implement or duplicate the C++/Python evaluator.

## Server communication workflow

```text
Camera capture
     |
     v
ARClientController
     |
     v
POST /upload-image
     |
     v
POST /runBatchFile
     |
     v
Server-side deep-learning localization
     |
     v
Pose response
     |
     v
DeepLearningRegistration
     |
     v
Unity AR target
```

The Unity project does not contain the trained deep-learning model or the server-side inference implementation. Those components can be maintained as separate repositories.

## Running the portfolio demo

The project was prepared for **Unity 2020.3.14f1**, matching the supplied project's original Unity version.

1. Open the project in Unity 2020.3.14f1.
2. Open `Assets/Scenes/PortfolioDemo.unity`.
3. Select the `ARClientServices` GameObject.
4. Configure `ApiServices` → `Host URL` for the compatible processing server.
5. Make sure the server-side processing pipeline is running.
6. Run the scene.
7. Use the capture control to capture an image.
8. The client uploads the image, requests processing, receives the pose, and applies it to `ARTarget`.

The included scene is intentionally lightweight and demonstrates the client architecture without requiring the original research project's large AR assets or proprietary/third-party mobile plugins.

## Configuration

The processing server URL is exposed through `ApiServices` rather than embedded in the workflow. For local development, the default is:

```text
http://localhost:3001/
```

Replace this with the address of the compatible server when running the complete client-server system.

No private research-network addresses, credentials, or authentication secrets are included in this repository.

## Reprojection evaluation

Reprojection-error calculation is treated as a **separate evaluation project** rather than part of the Unity client.

The evaluation pipeline can use camera pose, 2D image points, 3D reconstruction points, and COLMAP-related data to assess localization accuracy. Keeping it separate makes the AR client easier to understand and allows the evaluation tools to be reused independently.

## Portfolio scope

This repository represents the **Unity client/integration layer** of a broader computer-vision and deep-learning research system. The wider workflow includes:

- Unity/C# AR client
- REST API communication
- Server-side processing
- Deep-learning camera localization
- 6-DoF pose registration
- 3D reconstruction / COLMAP data
- Reprojection-error evaluation

The repositories can therefore be viewed as complementary components rather than one monolithic application.

## Attribution and code organisation

The supplied original research application contained multiple registration modes and collaborator-authored components. In particular, the supplied `RegistrationManager.cs` was explicitly marked `//Moritz`; it is therefore not included or presented as an individual portfolio contribution.

This version focuses on the supplied deep-learning pose-registration path and the Unity client/server integration. The original `ManualRegistration` implementation is represented by the more general `DeepLearningRegistration` component with additional validation and documentation.

## Related projects

This client can be presented alongside separate repositories for:

- The Node.js/REST processing server
- The server-side deep-learning localization pipeline
- The C++/Python reprojection and pose-evaluation tools

Together they demonstrate a complete research software pipeline while keeping each repository focused and maintainable.
