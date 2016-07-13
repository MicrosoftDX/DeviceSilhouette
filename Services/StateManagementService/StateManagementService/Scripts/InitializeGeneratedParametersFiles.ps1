$here = Split-Path -Parent $MyInvocation.MyCommand.Path
Write-Host "Removing *-generated.xml from '$here'"
Remove-Item "$here\..\ApplicationParameters\*-generated.xml"

Write-Host "Recreating *-generated.xml in '$here'"
Get-ChildItem "$here\..\ApplicationParameters\*.xml" | %{copy-item "$($_.FullName)" "$($_.FullName.Replace(".xml", "-generated.xml"))"}
