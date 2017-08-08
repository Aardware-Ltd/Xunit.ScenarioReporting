$v = new-object System.Version($env:APPVEYOR_BUILD_VERSION)
$version = "{0}.{1}.{2}-alpha-{3}" -f ($v.Major, $v.Minor, $v.Build, $v.Revision)

if($env:APPVEYOR_REPO_TAG -eq 'True'){
  $version = $env:APPVEYOR_REPO_BRANCH
}
#Nuget packages created every build for inspection before publishing

nuget pack Xunit.ScenarioReporting.nuspec -version $version