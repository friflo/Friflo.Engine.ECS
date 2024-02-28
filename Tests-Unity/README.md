

# Unity Editor Tests

Unit tests are used as symlink from `Tests folder`.  
These tests are executed as **Edit Mode** tests in Unity.


Menu > Window > General > Test Runner
Select: **EditMode**

`Tests-Unity/Assets/Scripts/Tests` -> `Tests`

Create symlink on windows
```
cd Tests-Unity/Assets/Scripts
mklink /D Tests ..\..\..\Tests
```