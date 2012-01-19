properties {
	$ProductVersion = "3.0"
	$BuildNumber = "0";
	$PatchVersion = "0"
	$PreRelease = "-build"	
	$PackageNameSuffix = ""
	$TargetFramework = "net-4.0"
	$UploadPackage = $false;
	$PackageIds = ""
	$DownloadDependentPackages = $false
	
}

$baseDir  = resolve-path .
$releaseRoot = "$baseDir\Release"
$releaseDir = "$releaseRoot\net40"
$binariesDir = "$baseDir\binaries"
$coreOnlyDir = "$baseDir\core-only"
$srcDir = "$baseDir"
$coreOnlyBinariesDir = "$coreOnlyDir\binaries"
$buildBase = "$baseDir\build"
$outDir =  "$buildBase\output"
$coreOnly =  "$buildBase\coreonly"
$libDir = "$baseDir\lib" 
$artifactsDir = "$baseDir\artifacts"
$toolsDir = "$baseDir\tools"
$nunitexec = "packages\NUnit.2.5.10.11092\tools\nunit-console.exe"
$nugetExec = "$toolsDir\NuGet\NuGet.exe"
$zipExec = "$toolsDir\zip\7za.exe"
$ilMergeKey = "$srcDir\NServiceBus.snk"
$ilMergeExclude = "$toolsDir\IlMerge\ilmerge.exclude"
$script:architecture = "x86"
$script:ilmergeTargetFramework = ""
$script:msBuildTargetFramework = ""	
$script:nunitTargetFramework = "/framework=4.0";
$script:msBuild = ""
$script:isEnvironmentInitialized = $false
$script:packageVersion = "3.0.0-local"
$script:releaseVersion = ""

include $toolsDir\psake\buildutils.ps1

task default -depends ReleaseNServiceBusErrorManagement
 
task Clean{

	if(Test-Path $buildBase){
		Delete-Directory $buildBase
		
	}
	
	if(Test-Path $artifactsDir){
		Delete-Directory $artifactsDir
		
	}
	
	if(Test-Path $binariesDir){
		Delete-Directory $binariesDir
		
	}
	
	if(Test-Path $coreOnlyDir){
		Delete-Directory $coreOnlyDir
		
	}
}

task InitEnvironment{

	if($script:isEnvironmentInitialized -ne $true){
		if ($TargetFramework -eq "net-4.0"){
			$netfxInstallroot ="" 
			$netfxInstallroot =	Get-RegistryValue 'HKLM:\SOFTWARE\Microsoft\.NETFramework\' 'InstallRoot' 
			
			$netfxCurrent = $netfxInstallroot + "v4.0.30319"
			
			$script:msBuild = $netfxCurrent + "\msbuild.exe"
			
			echo ".Net 4.0 build requested - $script:msBuild" 

			$script:ilmergeTargetFramework  = "/targetplatform:v4," + $netfxCurrent
			
			$script:msBuildTargetFramework ="/p:TargetFrameworkVersion=v4.0 /ToolsVersion:4.0"
			
			$script:nunitTargetFramework = "/framework=4.0";
			
			$script:isEnvironmentInitialized = $true
		}
	
	}
}

task Init -depends InitEnvironment, Clean, InstallDependentPackages, DetectOperatingSystemArchitecture {
   	
	echo "Creating build directory at the follwing path $buildBase"
	Delete-Directory $buildBase
	Create-Directory $buildBase
	
	$currentDirectory = Resolve-Path .
	
	echo "Current Directory: $currentDirectory" 
 }
  
task CompileMain -depends InitEnvironment -description "A build script CompileMain " { 

	$solutions = dir ".\*.sln"
	$solutions | % {
		$solutionFile = $_.FullName
		exec { &$script:msBuild $solutionFile /p:OutDir="$buildBase\nservicebusErrorMangement\" }
	}
	if(Test-Path $outDir){
		Delete-Directory $outDir
	}
	Create-Directory $outDir
	
	Copy-Item "$buildBase\nservicebusErrorMangement\NServiceBus.Management*.dll*" $outDir -Force;
	Copy-Item "$buildBase\nservicebusErrorMangement\GenerateError.dll.*" $outDir -Force;
}



task PrepareBinaries -depends Init, CompileMain {
	Prepare-Binaries
}

task JustPrepareBinaries -depends Init, CompileMain {
	Prepare-Binaries
}

function Prepare-Binaries{
	if(Test-Path $binariesDir){
		Delete-Directory "binaries"
	}
	Create-Directory $binariesDir;
	
	Copy-Item $outDir\*.* $binariesDir -Force;
	
}


task PrepareBinariesWithGeneratedAssemblyIno -depends GenerateAssemblyInfo, PrepareBinaries {}

task PrepareRelease -depends GenerateAssemblyInfo, PrepareBinaries {
	
	if(Test-Path $releaseRoot){
		Delete-Directory $releaseRoot	
	}
	
	Create-Directory $releaseRoot
	if ($TargetFramework -eq "net-4.0"){
		$releaseDir = "$releaseRoot\net40"
	}
	Create-Directory $releaseDir
 
	Copy-Item -Force "$baseDir\*.txt" $releaseRoot  -ErrorAction SilentlyContinue
	
	Copy-Item -Force -Recurse "$baseDir\binaries" $releaseDir\binaries -ErrorAction SilentlyContinue  
}

task CreatePackages -depends PrepareRelease  {
	
	
 }


task DetectOperatingSystemArchitecture {
	if (IsWow64 -eq $true)
	{
		$script:architecture = "x64"
	}
    echo "Machine Architecture is $script:architecture"
}
  
task GenerateAssemblyInfo {
	if($env:BUILD_NUMBER -ne $null) {
    	$BuildNumber = $env:BUILD_NUMBER
	}
	Write-Output "Build Number: $BuildNumber"
	
	$fileVersion = $ProductVersion + "." + $PatchVersion + "." + $BuildNumber 
	$asmVersion =  $ProductVersion + ".0.0"
	$infoVersion = $ProductVersion+ ".0" + $PreRelease + $BuildNumber 
	$script:releaseVersion = $infoVersion
	
	#Temporarily removed the PreRelease prefix ('-build') from the package version for CI packages to maintain compatibility with the existing versioning scheme
	#We will remove this as soon as we until we consolidate the CI and regular packages
	if($PackageNameSuffix -eq "-CI") {
		$script:packageVersion = $ProductVersion + "." + $BuildNumber
	}
	else {
		$script:packageVersion = $infoVersion
	}
		
	Write-Output "##teamcity[buildNumber '$script:releaseVersion']"
	
	$projectFiles = ls -path $srcDir -include *.csproj -recurse  
#	$projectFiles += ls -path $baseDir\tests -include *.csproj -recurse  

	foreach($projectFile in $projectFiles) {

		$projectDir = [System.IO.Path]::GetDirectoryName($projectFile)
		$projectName = [System.IO.Path]::GetFileName($projectDir)
		$asmInfo = [System.IO.Path]::Combine($projectDir, [System.IO.Path]::Combine("Properties", "AssemblyInfo.cs"))
		
		$assemblyTitle = gc $asmInfo | select-string -pattern "AssemblyTitle"
		
		if($assemblyTitle -ne $null){
			$assemblyTitle = $assemblyTitle.ToString()
			if($assemblyTitle -ne ""){
				$assemblyTitle = $assemblyTitle.Replace('[assembly: AssemblyTitle("', '') 
				$assemblyTitle = $assemblyTitle.Replace('")]', '') 
				$assemblyTitle = $assemblyTitle.Trim()
				
			}
		}
		else{
			$assemblyTitle = ""	
		}
		
		$assemblyDescription = gc $asmInfo | select-string -pattern "AssemblyDescription" 
		if($assemblyDescription -ne $null){
			$assemblyDescription = $assemblyDescription.ToString()
			if($assemblyDescription -ne ""){
				$assemblyDescription = $assemblyDescription.Replace('[assembly: AssemblyDescription("', '') 
				$assemblyDescription = $assemblyDescription.Replace('")]', '') 
				$assemblyDescription = $assemblyDescription.Trim()
			}
		}
		else{
			$assemblyDescription = ""
		}
		
		
		$assemblyProduct =  gc $asmInfo | select-string -pattern "AssemblyProduct" 
		
		if($assemblyProduct -ne $null){
			$assemblyProduct = $assemblyProduct.ToString()
			if($assemblyProduct -ne ""){
				$assemblyProduct = $assemblyProduct.Replace('[assembly: AssemblyProduct("', '') 
				$assemblyProduct = $assemblyProduct.Replace('")]', '') 
				$assemblyProduct = $assemblyProduct.Trim()
			}
		}
		else{
			$assemblyProduct = "NServiceBus"
		}
		
		$internalsVisibleTo = gc $asmInfo | select-string -pattern "InternalsVisibleTo" 
		
		if($internalsVisibleTo -ne $null){
			$internalsVisibleTo = $internalsVisibleTo.ToString()
			if($internalsVisibleTo -ne ""){
				$internalsVisibleTo = $internalsVisibleTo.Replace('[assembly: InternalsVisibleTo("', '') 
				$internalsVisibleTo = $internalsVisibleTo.Replace('")]', '') 
				$internalsVisibleTo = $internalsVisibleTo.Trim()
			}
		}
		else{
			$assemblyProduct = "NServiceBus"
		}
		
		$notclsCompliant = @("")

		$clsCompliant = $false
		
		Generate-Assembly-Info $assemblyTitle `
		$assemblyDescription  `
		$clsCompliant `
		$internalsVisibleTo `
		"release" `
		"NServiceBus" `
		$assemblyProduct `
		"Copyright � NServiceBus 2007-2011" `
		$asmVersion `
		$fileVersion `
		$infoVersion `
		$asmInfo 
 	}
}

task InstallDependentPackages {
	cd "$baseDir\packages"
	$files =  dir -Exclude *.config
	cd $baseDir
	$installDependentPackages = $true;
	if($installDependentPackages -eq $false){
		$installDependentPackages = ((($files -ne $null) -and ($files.count -gt 0)) -eq $false)
	}
	if($installDependentPackages){
	 	dir -recurse -include ('packages.config') |ForEach-Object {
		$packageconfig = [io.path]::Combine($_.directory,$_.name)

		write-host $packageconfig 

		 exec{ &$nugetExec install $packageconfig -o packages } 
		}
	}
 }

task ReleaseNServiceBusErrorManagement -depends PrepareRelease, CreatePackages, ZipOutput{
    if(Test-Path -Path $releaseDir)
	{
        del -Path $releaseDir -Force -recurse
	}	
	echo "Release completed for NServiceBus." + $script:releaseVersion 
	
	Stop-Process -Name "nunit-agent.exe" -ErrorAction SilentlyContinue -Force
	Stop-Process -Name "nunit-console.exe" -ErrorAction SilentlyContinue -Force
}

<#Ziping artifacts directory for releasing#>
task ZipOutput {
	
	echo "Cleaning the Release Artifacts before ziping"
	$packagingArtifacts = "$releaseRoot\PackagingArtifacts"
	$packageOutPutDir = "$releaseRoot\packages"
	
	if(Test-Path -Path $packagingArtifacts ){
		Delete-Directory $packagingArtifacts
	}
	Copy-Item -Force -Recurse $releaseDir\binaries "$releaseRoot\binaries"  -ErrorAction SilentlyContinue  
	Copy-Item -Force -Recurse $releaseDir\packages "$releaseRoot\packages"  -ErrorAction SilentlyContinue  
	
	Delete-Directory $releaseDir
			
	if((Test-Path -Path $packageOutPutDir) -and ($UploadPackage) ){
        Delete-Directory $packageOutPutDir
	}

	if((Test-Path -Path $artifactsDir) -eq $true)
	{
		Delete-Directory $artifactsDir
	}
	
    Create-Directory $artifactsDir
	
	$archive = "$artifactsDir\NServiceBusErrorManagement.$script:releaseVersion.zip"
	echo "Ziping artifacts directory for releasing"
	exec { &$zipExec a -tzip $archive $releaseRoot\** }
	
}

task UpdatePackages{
	dir -recurse -include ('packages.config') |ForEach-Object {
		$packageconfig = [io.path]::Combine($_.directory,$_.name)

		write-host $packageconfig

		if($PackageIds -ne "")
		{
			write-host "Doing an unsafe update of" $PackageIds 
			&$nugetExec update $packageconfig -RepositoryPath packages -Id $PackageIds
		}
		else
		{	
			write-host "Doing a safe update of all packages" $PackageIds 
			&$nugetExec update -Safe $packageconfig -RepositoryPath packages
		}
	}
}