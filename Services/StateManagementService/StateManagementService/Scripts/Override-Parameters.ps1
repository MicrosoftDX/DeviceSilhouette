function CreateParametersFileWithEnvironmentOverrides($paramsFilename)
{
    $xml = [xml](Get-Content $paramsFilename)
    $xml.Application.Parameters.Parameter | %{ 
        # [PSCustomObject]@{ "Name"= $_.Name; "EnvVarExists" = (Test-Path "env:$($_.Name)")} 
        if (Test-Path "env:$($_.Name)"){
            $_.Value = (Get-Item "env:$($_.Name)").Value
        }
    }
    $tempfile = [System.IO.Path]::GetTempFileName()
    $xml.Save($tempfile)
    return $tempfile
}