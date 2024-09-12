# Source and Portfolio Notes

This repository was reorganised from the supplied AR client code for portfolio presentation.

## Preserved concepts

- Unity camera capture
- image upload to a processing server
- server-side batch/deep-learning processing
- returned camera pose data
- OpenCV-to-Unity pose conversion
- AR scene registration
- client/server separation

## Refactoring

The original application controller contained additional registration modes and testing controls. The portfolio version narrows the main controller to the deep-learning client workflow so the repository has a clear software-engineering boundary.

The original `EsacRegistration` implementation supplied for this project has been renamed conceptually to `DeepLearningRegistration`. The underlying pose parsing and coordinate conversion were retained, with validation and documentation added.

The original `RegistrationManager` was marked as `//Moritz` and coordinated multiple registration modes. It is therefore not included as an individual portfolio contribution.

## Important limitation

The Unity client does not contain the trained deep-learning model. The inference stage is expected to run in the companion processing-server project. Likewise, the quantitative reprojection evaluator remains a separate research/evaluation component rather than being replaced by a new implementation here.
