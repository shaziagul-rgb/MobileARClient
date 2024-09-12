# Client Workflow

## Runtime sequence

1. The Unity client starts.
2. `ApiServices` is configured with the processing-server URL.
3. `WebCam` captures a camera frame.
4. `IngameCanvasUI` receives the captured texture.
5. `ApiServices.UploadImage()` sends the image to the server.
6. The client requests `/runBatchFile`.
7. The server executes the configured deep-learning localization pipeline.
8. The server returns pose information.
9. `DeepLearningRegistration.LoadData()` parses the response.
10. The OpenCV-to-Unity coordinate conversion is applied.
11. The AR target receives the estimated position and orientation.
12. Reprojection evaluation can be performed by the separate evaluation pipeline.

## Design goal

The client should remain independent of the deep-learning model implementation. This makes it possible to change or retrain the server-side localization model without rewriting the Unity camera and AR integration layer.
