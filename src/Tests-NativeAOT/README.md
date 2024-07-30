
# Tests-NativeAOT


Example unit test for NativeAOT:  
[testfx/samples/public/DemoMSTestSdk/ProjectWithNativeAOT at main · microsoft/testfx](https://github.com/microsoft/testfx/tree/main/samples/public/DemoMSTestSdk/ProjectWithNativeAOT)

**Note** NativeAOT test run currently only from command line

```
cd src/Tests-NativeAOT
```

Debug
```
dotnet publish --runtime win-x64 -c DEBUG
bin\Debug\net8.0\win-x64\publish\Tests-NativeAOT.exe
```

Release
```
dotnet publish --runtime win-x64
bin\Release\net8.0\win-x64\publish\Tests-NativeAOT.exe
```

## Make library trimmable

Added following attribute to methods generating a trim warning like:
```cs
[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "Not called for NativeAOT")]
```

- [How to make libraries compatible with native AOT - .NET Blog](https://devblogs.microsoft.com/dotnet/creating-aot-compatible-libraries/)
