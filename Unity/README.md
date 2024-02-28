

# Unity Editor Tests

Unit tests are used as symlink from `Tests folder`.  
These tests are executed as **EditMode** tests in Unity.

Friflo.Engine.ECS is added as a nuget package using [NuGetForUnity - GitHub](https://github.com/GlitchEnzo/NuGetForUnity).

## Install NuGetForUnity

1. Open Package Manager at  
  Menu > Window > Package Manager

2. Click + button on the upper-left of a window, and select "Add package from git URL..."

3. Enter the following URL and click Add button
    ```
    https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity
    ```


## Create Symlink

The symlink is already committed to git repository.  
To create a symlink manually execute the steps below.

`Tests-Unity/Assets/Scripts/Tests` -> `Tests`

Create symlink on windows
```
cd Tests-Unity/Assets/Scripts
mklink /D Tests ..\..\..\Tests
```


## Run Tests

Menu > Window > General > Test Runner  
Select: **EditMode**
