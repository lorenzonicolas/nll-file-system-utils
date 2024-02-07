# File System Utils
Repositorio que contiene funciones útiles de acceso a archivos y directorios en Windows.


### Como ejecutar
 - Build: `dotnet build`
 - Publish package: ``

### Testing y coverage report:

`dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura`
`reportgenerator -reports:".\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html`

### Mejoras / Pendientes
[ ] E2E tests
[ ] Unit tests