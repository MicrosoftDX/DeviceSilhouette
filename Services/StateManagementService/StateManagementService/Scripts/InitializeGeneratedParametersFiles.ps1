$here = Split-Path -Parent $MyInvocation.MyCommand.Path
Remove-Item "$here\..\ApplicationParameters\*-generated.xml"

Get-ChildItem "$here\..\ApplicationParameters\*.xml" | %{copy-item "$($_.FullName)" "$($_.FullName.Replace(".xml", "-generated.xml"))"}
