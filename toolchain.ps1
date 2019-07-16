$folder = Resolve-Path "ML-Wallstreet-UI"
Push-Location $folder
dotnet publish -c Release
Pop-Location


$folder = Resolve-Path "DataManager-API"
Push-Location $folder
dotnet publish -c Release
Pop-Location

$folder = Resolve-Path "MLService-API"
Push-Location $folder
dotnet publish -c Release
Pop-Location