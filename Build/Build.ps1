Function Get-Script-Directory
{
    $Invoction = (Get-Variable MyInvocation -Scope 1).Value
    
    Split-Path $Invoction.MyCommand.Path
}

$Global:Paths = @{ }

$Global:Paths.Root = (Get-Item (Get-Script-Directory)).Parent.FullName + "\"
$Global:Paths.Release = $Global:Paths.Root + "NHibernate.OData.Demo\bin\Release\"
$Global:Paths.Distrib = $Global:Paths.Root + "Build\Distrib\"

[void][System.Reflection.Assembly]::LoadFile($Global:Paths.Root + "Libraries\SharpZipLib\ICSharpCode.SharpZipLib.dll")

Write-Host "Preparing distribution folder"

if (Test-Path -Path $Global:Paths.Distrib)
{
    Remove-Item -Recurse -Force $Global:Paths.Distrib
}

[void](New-Item -Type directory $Global:Paths.Distrib)
[void](New-Item -Type directory ($Global:Paths.Distrib + "Library\"))
[void](New-Item -Type directory ($Global:Paths.Distrib + "Demo\"))

Write-Host "Copying distributables"

Copy-Item ($Global:Paths.Release + "*.dll") ($Global:Paths.Distrib + "Demo\")
Copy-Item ($Global:Paths.Release + "*.config") ($Global:Paths.Distrib + "Demo\")
Copy-Item ($Global:Paths.Release + "*.exe") ($Global:Paths.Distrib + "Demo\") -Exclude "*.vshost.exe"

Copy-Item ($Global:Paths.Release + "NHibernate.OData.dll") ($Global:Paths.Distrib + "Library\")
Copy-Item ($Global:Paths.Release + "NHibernate.OData.xml") ($Global:Paths.Distrib + "Library\")
Copy-Item ($Global:Paths.Release + "License.txt") ($Global:Paths.Distrib)
Copy-Item ($Global:Paths.Release + "Readme.txt") ($Global:Paths.Distrib + "Library\")

Write-Host "Creating ZIP file"

(New-Object ICSharpCode.SharpZipLib.Zip.FastZip).CreateZip(
    ($Global:Paths.Root + "Build\NHibernate.OData.zip"),
    $Global:Paths.Distrib,
    $True,
    $Null
)

Move-Item ($Global:Paths.Root + "Build\NHibernate.OData.zip") ($Global:Paths.Distrib + "NHibernate.OData.zip")