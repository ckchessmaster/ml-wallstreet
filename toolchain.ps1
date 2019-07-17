Stop-WebAppPool -Name "ml-wallstreet-ui"
$folder = Resolve-Path "ML-Wallstreet-UI"
Push-Location $folder
dotnet publish -c Release --self-contained -r win-x64
Move-Item -Path .\ML-Wallstreet-UI\bin\Release\netcoreapp2.2\win-x64\publish -Destination ..\..\hosting\ML-Wallstreet-UI
Pop-Location
Start-WebAppPool -Name "ml-wallstreet-ui"

Stop-WebAppPool -Name "data-manager-api"
$folder = Resolve-Path "DataManager-API"
Push-Location $folder
dotnet publish -c Release --self-contained -r win-x64
Move-Item -Path .\DataManager-API\bin\Release\netcoreapp2.2\win-x64\publish -Destination ..\..\hosting\DataManager-API
Pop-Location
Start-WebAppPool -Name "data-manager-api"

Stop-WebAppPool -Name "ml-service-api"
$folder = Resolve-Path "MLService-API"
Push-Location $folder
dotnet publish -c Release --self-contained -r win-x64
Move-Item -Path .\MLService-API\bin\Release\netcoreapp2.2\win-x64\publish -Destination ..\..\hosting\MLService-API
Pop-Location
Start-WebAppPool -Name "ml-service-api"