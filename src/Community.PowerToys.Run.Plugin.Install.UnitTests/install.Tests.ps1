Describe 'install' {
    BeforeAll {
        $subject = Join-Path $PSScriptRoot "..\Community.PowerToys.Run.Plugin.Install\install.ps1"
        $validAssetUrl = "https://github.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.zip"
        $validPluginDirectory = "C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\GEmojiSharp\"
    }

    Context 'validation' {
        It 'should throw when command is invalid' {
            { & $subject -command "Invalid" } | Should -Throw "Cannot validate argument on parameter 'command'*"
        }

        It 'should throw when assetUrl does not start with https://github.com' {
            $invalid = "https://gitfail.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.zip"
            { & $subject -command "Update" -assetUrl $invalid } | Should -Throw "Cannot validate argument on parameter 'assetUrl'*"
        }
        It 'should throw when assetUrl does not contain owner or repo' {
            $invalid = "https://github.com/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.zip"
            { & $subject -command "Update" -assetUrl $invalid } | Should -Throw "Cannot validate argument on parameter 'assetUrl'*"
        }
        It 'should throw when assetUrl does not contain tag' {
            $invalid = "https://github.com/hlaueriksson/GEmojiSharp/releases/download/GEmojiSharp.PowerToysRun-4.0.0-x64.zip"
            { & $subject -command "Update" -assetUrl $invalid } | Should -Throw "Cannot validate argument on parameter 'assetUrl'*"
        }
        It 'should throw when assetUrl does not end with .zip' {
            $invalid = "https://github.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.0-x64.txt"
            { & $subject -command "Update" -assetUrl $invalid } | Should -Throw "Cannot validate argument on parameter 'assetUrl'*"
        }
        It 'should throw when assetUrl is not found' {
            $invalid = "https://github.com/hlaueriksson/GEmojiSharp/releases/download/v4.0.0/GEmojiSharp.PowerToysRun-4.0.4-x64.zip"
            { & $subject -command "Update" -assetUrl $invalid -pluginDirectory $validPluginDirectory } | Should -Throw "AssetUrl is invalid."
        }
    
        It 'should throw when pluginDirectory does not start with C:\Users' {
            $invalid = "C:\Losers\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\GEmojiSharp\"
            { & $subject -command "Update" -assetUrl $validAssetUrl -pluginDirectory $invalid } | Should -Throw "Cannot validate argument on parameter 'pluginDirectory'*"
        }
        It 'should throw when pluginDirectory does not contain user' {
            $invalid = "C:\Users\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\GEmojiSharp\"
            { & $subject -command "Update" -assetUrl $validAssetUrl -pluginDirectory $invalid } | Should -Throw "Cannot validate argument on parameter 'pluginDirectory'*"
        }
        It 'should throw when pluginDirectory does not contain the correct PowerToys path' {
            $invalid = "C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\"
            { & $subject -command "Update" -assetUrl $validAssetUrl -pluginDirectory $invalid } | Should -Throw "Cannot validate argument on parameter 'pluginDirectory'*"
        }
        It 'should throw when pluginDirectory does not end with plugin folder' {
            $invalid = "C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\"
            { & $subject -command "Update" -assetUrl $validAssetUrl -pluginDirectory $invalid } | Should -Throw "Cannot validate argument on parameter 'pluginDirectory'*"
        }
        It 'should throw when pluginDirectory is not found' {
            $invalid = "C:\Users\Henrik\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins\404\"
            { & $subject -command "Update" -assetUrl $validAssetUrl -pluginDirectory $invalid } | Should -Throw "PluginDirectory is invalid."
        }
    }

    Context 'Install' {
        It 'should work when params are valid' {
            { & $subject -command "Install" -assetUrl $validAssetUrl } | Should -Not -Throw
        }
    }

    Context 'Update' {
        It 'should work when params are valid' {
            { & $subject -command "Update" -assetUrl $validAssetUrl -pluginDirectory $validPluginDirectory } | Should -Not -Throw
        }
    }

    Context 'Uninstall' {
        It 'should work when params are valid' {
            { & $subject -command "Uninstall" -pluginDirectory $validPluginDirectory } | Should -Not -Throw
        }
    }
}
