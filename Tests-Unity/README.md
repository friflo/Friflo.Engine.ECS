

# Unity Editor Tests

Unit tests are used as symlink from `Tests folder`.  
These tests are executed as **EditMode** tests in Unity.


## Create Symlink

`Tests-Unity/Assets/Scripts/Tests` -> `Tests`

Create symlink on windows
```
cd Tests-Unity/Assets/Scripts
mklink /D Tests ..\..\..\Tests
```


## Run Tests

Menu > Window > General > Test Runner  
Select: **EditMode**
