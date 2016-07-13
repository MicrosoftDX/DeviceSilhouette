function OverrideParametersFileFromBaseWithEnvironmentVariables($paramsFilename)
{
    Write-Host "*********************************************"
    Write-Host "*** Applying parameter file overrides"
    if (-not($paramsFilename.Contains("-generated.xml"))){
        Write-Host "*** ERROR: Params file should point to the generated file. Got: '$paramsFilename'"
        return;
    }
    $baseParamsFile = $paramsFilename.Replace("-generated.xml", ".xml")
    Write-Host "*** Replacing '$paramsFilename' using '$baseParamsFile' as input"
    $xml = [xml](Get-Content $baseParamsFile)
    $xml.Application.Parameters.Parameter | %{ 
        $parameterName = $_.Name
        if (Test-Path "env:$parameterName"){
            $newValue = (Get-Item "env:$parameterName").Value
            Write-Host "*** overriding parameter '$parameterName' with value '$newValue' "
            $_.Value = $newValue
        }
    }
    $xml.Save($paramsFilename)
    Write-Host "*********************************************"
}
