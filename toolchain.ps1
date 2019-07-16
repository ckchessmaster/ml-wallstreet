$folder = Resolve-Path "ML-Wallstreet-UI"
Push-Location $folder
dotnet publish -c Release
Pop-Location


$folder = Resolve-Path "DataManager-API"
dotnet publish -c Release
Pop-Location

$folder = Resolve-Path "ML=Service-API"
Push-Location $folder
dotnet publish -c Release
Pop-Location