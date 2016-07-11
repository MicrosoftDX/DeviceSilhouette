function CreateParametersFileWithEnvironmentOverrides($paramsFilename)
{
    $xml = [xml](Get-Content $paramsFilename)
    $xml.Application.Parameters.Parameter | %{ 
        # [PSCustomObject]@{ "Name"= $_.Name; "EnvVarExists" = (Test-Path "env:$($_.Name)")} 
        $parameterName = $_.Name
        if (Test-Path "env:$parameterName"){
            $newValue = (Get-Item "env:$parameterName").Value
            Write-Host "** overriding '$parameterName' with '$newValue' "
            $_.Value = $newValue
        }
    }
    $tempfile = [System.IO.Path]::GetTempFileName()
    $xml.Save($tempfile)
    return $tempfile
}