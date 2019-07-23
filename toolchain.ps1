$basePath = ($PSScriptRoot + "\")

Stop-WebAppPool -Name "ml-wallstreet-ui" # Stop the app
$folder = $basePath + "ML-Wallstreet-UI"
& ($basePath + "db-update.ps1") -server "localhost" -dbName "DataManager-ckingdon" -project "DataManager-API"
Push-Location $folder # Navigate to the code location
dotnet publish -c Release --self-contained -r win-x64 # Build the code
Remove-Item -Force -Path ..\..\hosting\ML-Wallstreet-UI -Recurse #Delete the old code
Move-Item -Force -Path .\ML-Wallstreet-UI\bin\Release\netcoreapp2.2\win-x64\publish -Destination ..\..\hosting\ML-Wallstreet-UI # Move the new code
Pop-Location # Go back to where we started
Start-WebAppPool -Name "ml-wallstreet-ui" # Start the app
