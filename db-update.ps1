param (
    [Parameter(Mandatory = $true)]
    [Alias('d')]
    [string]$dbName,

    [Parameter(Mandatory = $true)]
    [Alias('p')]
    [string]$project
)

Write-Host "Checking for scripts to run..."

$previouslyRunScripts = Invoke-Sqlcmd -ServerInstance "localhost" -Database $dbName -Query "SELECT ScriptName FROM PreviouslyRunScripts"

Push-Location ($PSScriptRoot + "\" + $project + "\db-scripts")

$scriptsToRun = Get-ChildItem -Name

$scriptsThatHaveNotBeenRun = $scriptsToRun |? {$previouslyRunScripts.ScriptName -notcontains $_}

$finalScriptList = $scriptsThatHaveNotBeenRun | Sort-Object -Property @{Expression={[int]$_.substring(0,$_.IndexOf('-'))}}

Write-Host "Running" $finalScriptList.Length "scripts..."
$finalScriptList | ForEach-Object -Process {
    Write-Host "Running script:" $_
    $commandOutput = Invoke-Sqlcmd -ServerInstance "localhost" -Database $dbName -InputFile $_
    $insertOutput = Invoke-Sqlcmd -ServerInstance "localhost" -Database $dbName -Query "INSERT INTO PreviouslyRunScripts (ScriptName, RunDate) VALUES ('$_', GETDATE())"
}

Pop-Location