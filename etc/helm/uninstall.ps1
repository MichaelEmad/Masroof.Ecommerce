param (
	$Namespace="ecommerce-local",
    $ReleaseName="ecommerce-local",
    $User = ""
)

if([string]::IsNullOrEmpty($User) -eq $false)
{
    $Namespace += '-' + $User
    $ReleaseName += '-' + $User
}

helm uninstall ${ReleaseName} --namespace ${Namespace}
exit $LASTEXITCODE