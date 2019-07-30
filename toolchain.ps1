$basePath = ("C:\Users\ckche\Desktop\ML Stock Project\ml-wallstreet\")

$gitResults = git pull


# Build the UI
if ($gitResults -match "ML-Wallstreet-UI") {
    Stop-WebAppPool -Name "ml-wallstreet-ui" # Stop the app
    $folder = $basePath + "ML-Wallstreet-UI"
    Push-Location $folder # Navigate to the code location
    dotnet publish -c Release --self-contained -r win-x64 # Build the code
    Remove-Item -Force -Path ..\..\hosting\ML-Wallstreet-UI -Recurse #Delete the old code
    Move-Item -Force -Path .\ML-Wallstreet-UI\bin\Release\netcoreapp2.2\win-x64\publish -Destination ..\..\hosting\ML-Wallstreet-UI # Move the new code
    Pop-Location # Go back to where we started
    Start-WebAppPool -Name "ml-wallstreet-ui" # Start the app
}

# Build the DataManager API
if ($gitResults -match "DataManager-API") {
    Stop-WebAppPool -Name "data-manager-api" # Stop the app
    & ($basePath + "db-update.ps1") -server "localhost" -dbName "DataManager" -project "DataManager-API" # Update the Database
    $folder = $basePath + "DataManager-API"
    Push-Location $folder # Navigate to the code location
    dotnet publish -c Release --self-contained -r win-x64 # Build the code
    Remove-Item -Force -Path ..\..\hosting\DataManager-API -Recurse #Delete the old code
    Move-Item -Force -Path .\DataManager-API\bin\Release\netcoreapp2.2\win-x64\publish -Destination ..\..\hosting\DataManager-API # Move the new code
    Pop-Location # Go back to where we started
    Start-WebAppPool -Name "data-manager-api" # Start the app
}

# Build the MLService API
if ($gitResults -match "MLService-API") {
    Stop-WebAppPool -Name "ml-service-api" # Stop the app
    & ($basePath + "db-update.ps1") -server "localhost" -dbName "MLService" -project "MLService-API" # Update the Database
    $folder = $basePath + "MLService-API"
    Push-Location $folder # Navigate to the code location
    dotnet publish -c Release --self-contained -r win-x64 # Build the code
    Remove-Item -Force -Path ..\..\hosting\MLService-API -Recurse #Delete the old code
    Move-Item -Force -Path .\MLService-API\bin\Release\netcoreapp2.2\win-x64\publish -Destination ..\..\hosting\MLService-API # Move the new code
    Pop-Location # Go back to where we started
    Start-WebAppPool -Name "ml-service-api" # Start the app
}
