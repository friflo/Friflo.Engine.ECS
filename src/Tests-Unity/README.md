
# Install

Friflo.Engine.ECS is added as a nuget package using [NuGetForUnity - GitHub](https://github.com/GlitchEnzo/NuGetForUnity).  
NuGetForUnity **must be installed** in Unity to add Friflo.Engine.ECS as nuget package.

### Install NuGetForUnity

1. Open Package Manager at  
  Menu > Window > Package Manager

2. Click + button on the upper-left of a window, and select "Add package from git URL..."

3. Enter the following URL and click Add button
    ```
    https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity
    ```
### Add nuget package **Friflo.Engine.ECS**

1. Open Nuget Manager at  
   Menu > NuGet > Manage Nuget Packages

2. Search the package below and click Install.
    ```
    Friflo.Engine.ECS
    ```



# Unity Editor Tests

Running unit tests is only relevant for development of the library.

Unit tests are used as symlink from `Tests folder`.  
These tests are executed as **EditMode** tests in Unity.


### Create Symlink

The symlink is already committed to git repository.  
To create a symlink manually execute the steps below.

`Unity/Assets/Scripts/Tests` -> `Tests`

Create symlink on windows
```
cd Unity/Assets/Scripts
mklink /D Tests ..\..\..\Tests
```

### Run Tests

Menu > Window > General > Test Runner  
Select: **EditMode**


### Unity Project Setup
- Selected Unity Install: 2022.3.20f1
- Selected template: 3D (URP) Core  
  URP is required to enable using an Matrix4x4[] array with a length  
  of multiple of 100.000 by `Graphics.RenderMeshInstanced()`  
  

### Build
- Windows:  
  logs of a standalone build are written to:
  ```
  %appdata%\..\LocalLow\DefaultCompany\Unity
  ```
  
