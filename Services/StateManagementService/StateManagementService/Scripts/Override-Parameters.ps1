function CreateParametersFileWithEnvironmentOverrides($paramsFilename)
{
    $xml = [xml](Get-Content $paramsFilename)
    $xml.Application.Parameters.Parameter | %{ 
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

function OverwriteApplicationParameterFilePath{
    param(
        [Parameter(Mandatory=$true)]
        $profileFilePath,

        [Parameter(Mandatory=$true)]
        $parameterFilePath
    )

    $fileContent = Get-Content $profileFilePath -Raw
    $xml = [xml]$fileContent
    $xml.PublishProfile.ApplicationParameterFile.Path = $parameterFilePath 
    $xml.Save($profileFilePath)
}