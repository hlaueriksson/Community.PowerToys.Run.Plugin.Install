<#PSScriptInfo
.VERSION 0.0.0
.GUID 71a16cf0-1b68-49e7-91d7-c75e6dc9cf91
.AUTHOR Henrik Lau Eriksson
.COMPANYNAME
.COPYRIGHT
.TAGS PowerToys Run Plugins Settings
.LICENSEURI
.PROJECTURI https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install
.ICONURI
.EXTERNALMODULEDEPENDENCIES
.REQUIREDSCRIPTS
.EXTERNALSCRIPTDEPENDENCIES
.RELEASENOTES
#>

<#
    .Synopsis
    Opens the plugin settings.

    .Description
    Opens PowerToys Settings for PowerToys Run.

    .Example
    .\settings.ps1

    .Link
    https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Templates
#>

$machinePath = "C:\Program Files\PowerToys\PowerToys.exe"
$userPath = "$env:LOCALAPPDATA\PowerToys\PowerToys.exe"

if (Test-Path $machinePath) {
    Start-Process -FilePath $machinePath --open-settings=Run
}
elseif (Test-Path $userPath) {
    Start-Process -FilePath $userPath --open-settings=Run
}
else {
    Write-Error "Unable to open PowerToys Settings"
}
