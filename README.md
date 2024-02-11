# File System Utils
Repositorio que contiene funciones útiles de acceso a archivos y directorios en Windows.

### Generar paquete local:
 - Build and generate package: `dotnet build; dotnet publish; dotnet pack;`
 - Set up local nuget: `dotnet nuget add source D:\Repositorios\packages --name "Local"`
 - Publish package: `dotnet nuget push "**/Release/*.nupkg" --source "Local"`

### Testing y coverage report:

`dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura`
`reportgenerator -reports:".\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html`

### Mejoras / Pendientes
[ ] E2E tests
[ ] Unit tests

[![Publish](https://github.com/lorenzonicolas/nll-file-system-utils/actions/workflows/publish.yml/badge.svg?branch=master)](https://github.com/lorenzonicolas/nll-file-system-utils/actions/workflows/publish.yml)