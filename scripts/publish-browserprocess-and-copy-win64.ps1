& "$PSScriptRoot/publish-browserprocess-win64.ps1"
Copy-Item -Path "../src/CefBrowserProcess/bin/Release/win-x64/publish/*" -Destination "../src/CefBrowser/Plugins/CefBrowser/" -Recurse -Force -PassThru