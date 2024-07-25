

# Test code coverage - CLI


## Setup

### Install dotnet-coverage - CLI Tool

See: [dotnet-coverage code coverage utility - Microsoft](https://learn.microsoft.com/en-us/dotnet/core/additional-tools/dotnet-coverage)

```
dotnet tool install --global dotnet-coverage
```


### ReportGenerator - CLI Tool

See: [ReportGenerator - github](https://github.com/danielpalme/ReportGenerator)

```
dotnet tool install -g dotnet-reportgenerator-globaltool
```


## Usage


Create single `coverage.cobertura.xml` file by running unit tests.  
*Executes in ~ 7 second.*
```
dotnet-coverage collect -f cobertura -s docs/scripts/coverage.settings.xml -o "coverage.cobertura.xml" "dotnet test"
```

Create html files from `coverage.cobertura.xml`.  
Html files a generated in `./Report` folder.  
Index page: `./Report/index.html`  
*Executes in ~ 1 second.*
```
reportgenerator "-reports:coverage.cobertura.xml" "-targetdir:Report"
```


## GitHub Actions integration
```yaml
    - name: Install dotnet-coverage
      run: dotnet tool install --global dotnet-coverage

    - name: Test with coverage
      working-directory: ./Engine
      run: dotnet-coverage collect -f cobertura -s coverage.settings.xml -o "coverage.cobertura.xml" "dotnet test"

    - name: Upload coverage to codecov
      uses: codecov/codecov-action@v4
      with:
        token: ${{ secrets.CODECOV_TOKEN }}
        file: Engine/coverage.cobertura.xml
        name: Friflo.Engine.ECS
```