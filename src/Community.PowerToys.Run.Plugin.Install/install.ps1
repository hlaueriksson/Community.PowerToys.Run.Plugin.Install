<#PSScriptInfo
.VERSION 0.2.0
.GUID fb1d81ea-1c02-4585-a631-6e603bd04d4e
.AUTHOR Henrik Lau Eriksson
.COMPANYNAME
.COPYRIGHT
.TAGS PowerToys Run Plugins Install
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
    Installs/Updates/Uninstalls PowerToys Run community plugins.

    .Description
    Installs a plugin in these steps:
    1. Kills PowerToys
    2. Downloads the latest release from GitHub
    3. Verifies release hash
    4. Extracts the release zip file
    5. Starts PowerToys

    Updates a plugin in these steps:
    1. Kills PowerToys
    2. Downloads the latest release from GitHub
    3. Verifies release hash
    4. Deletes the old plugin files
    5. Extracts the release zip file
    6. Starts PowerToys

    Uninstalls a plugin in these steps:
    1. Kills PowerToys
    2. Deletes the plugin folder
    3. Starts PowerToys

    .Parameter Command
    Install | Update | Uninstall

    .Parameter AssetUrl
    The URL to the release zip file on GitHub

    .Parameter PluginDirectory
    The path to the plugin directory, i.e. a subdirectory under %LocalAppData%\Microsoft\PowerToys\PowerToys Run\Plugins\

    .Example
    .\install.ps1 "Install" "https://github.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.zip" ""

    .Example
    .\install.ps1 "Update" "https://github.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.zip" "%LocalAppData%\Microsoft\PowerToys\PowerToys Run\Plugins\GEmojiSharp"

    .Example
    .\install.ps1 "Uninstall" "" "%LocalAppData%\Microsoft\PowerToys\PowerToys Run\Plugins\GEmojiSharp"

    .Link
    https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Install
#>
param (
    [Parameter(Position = 0, Mandatory = $true)]
    [ValidateSet('Install', 'Update', 'Uninstall')]
    [string]$command,

    [Parameter(Position = 1, Mandatory = $false)]
    [ValidatePattern('^https://github\.com/[^/]+/[^/]+/releases/download/[^/]+/[^/]+\.zip$')]
    [string]$assetUrl,

    [Parameter(Position = 2, Mandatory = $false)]
    [ValidatePattern('(?i)^C:\\Users\\[^\\]+\\AppData\\Local\\Microsoft\\PowerToys\\PowerToys Run\\Plugins\\[^\\]+\\?$')]
    [string]$pluginDirectory
)

function ValidateAssetUrl {
    try { Invoke-WebRequest -Uri $assetUrl -Method Head } catch {
        throw "AssetUrl is invalid."
    }
}

function ValidatePluginDirectory {
    if (-Not (Test-Path -Path $pluginDirectory)) {
        throw "PluginDirectory is invalid."
    }
}

function Write-Log {
    param (
        [string]$message
    )
    $result = "$(Get-Date -Format "yyyy-MM-dd HH:mm:ss") $message"
    Write-Output $result
}

#Requires -RunAsAdministrator

switch ($command) {
    "Install" {
        ValidateAssetUrl
    }
    "Update" {
        ValidateAssetUrl
        ValidatePluginDirectory
    }
    "Uninstall" {
        ValidatePluginDirectory
    }
}

$script:currentDirectory = $PSScriptRoot

$log = Join-Path $script:currentDirectory "install.log"
Start-Transcript -Path $log

Write-Log "Command: $command"
Write-Log "AssetUrl: $assetUrl"
Write-Log "PluginDirectory: $pluginDirectory"
Write-Log "Log: $log"

if ($command -ne "Uninstall") {
    $assetName = Split-Path $assetUrl -Leaf
    $script:release = Join-Path $script:currentDirectory $assetName

    Write-Log "Release: $script:release"
}

function DownloadRelease {
    Write-Log "Download release"
    Invoke-WebRequest -Uri $assetUrl -OutFile $script:release

    $hash = Get-FileHash $script:release -Algorithm SHA256 | Select-Object -ExpandProperty Hash
    Write-Log "Hash: $hash"
    if ($assetUrl -match 'github\.com/([^/]+)/([^/]+)/') {
        $owner = $matches[1]
        $repo = $matches[2]
        $latest = "https://github.com/$owner/$repo/releases/latest"
        Write-Log "Latest: $latest"
        $response = Invoke-WebRequest -Uri $latest
        if ($response.Content -match $hash) {
            Write-Log "Hash is verified"
        }
        else {
            Write-Warning "Hash could not be verified"
        }
    }
}

function InstallPlugin {
    DownloadRelease

    Write-Log "Extract release"
    $parent = Split-Path -Path $script:currentDirectory -Parent
    Expand-Archive -Path $script:release -DestinationPath $parent -Force
}

function UpdatePlugin {
    DownloadRelease

    Write-Log "Delete plugin files"
    Get-ChildItem -Path $pluginDirectory -Exclude @("install.*", "*.zip") | Remove-Item -Recurse -Force -Confirm:$false

    Write-Log "Extract release"
    $parent = Split-Path -Path $pluginDirectory -Parent
    Expand-Archive -Path $script:release -DestinationPath $parent -Force
}

function UninstallPlugin {
    Write-Log "Delete plugin folder"
    Remove-Item -Path $pluginDirectory -Recurse -Force -Confirm:$false
}

try {
    Write-Log "Kill PowerToys"
    $name = "PowerToys"
    $process = Get-Process -Name $name -ErrorAction SilentlyContinue
    if ($process) {
        Stop-Process -Name $name -Force
        while ($null -ne (Get-Process -Name $name -ErrorAction SilentlyContinue)) {
            Start-Sleep -Seconds 1
        }
    }

    switch ($command) {
        "Install" {
            InstallPlugin
        }
        "Update" {
            UpdatePlugin
        }
        "Uninstall" {
            UninstallPlugin
        }
    }

    Write-Log "Start PowerToys"
    $machinePath = "C:\Program Files\PowerToys\PowerToys.exe"
    $userPath = "$env:LOCALAPPDATA\PowerToys\PowerToys.exe"
    if (Test-Path $machinePath) {
        Start-Process -FilePath $machinePath
    }
    elseif (Test-Path $userPath) {
        Start-Process -FilePath $userPath
    }
    else {
        Write-Error "Start PowerToys failed"
    }

    Write-Log "$command complete!"
}
catch {
    Write-Error $_
    Write-Log "$command failed!"
}
finally {
    Stop-Transcript
}
