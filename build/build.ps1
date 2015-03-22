Framework('4.0')
Include .\version.ps1

$prod = 'togglez'
$tests = "$prod.tests"
$solution = "$prod.sln"
$nugetName = 'Togglez'
$nugetProj = 'Togglez.csproj'

properties {
    $config= if($config -eq $null) {'Debug' } else {$config}
    $base_dir = resolve-path .\..
    $source_dir = "$base_dir\src"
    $tools_dir = "$base_dir\tools"
    $env = "local"
    $out_dir = "$base_dir\out\$config"
    $prod_dir = "$source_dir\$prod"
    $prod_artefacts_dir="$prod_dir\$prod\bin\$config"
    $prod_test_dir = "$prod_dir\$tests\bin\$config"
    $test_results_dir="$base_dir\test-results"
    $package_dir = "$base_dir\deploy"
    $test_dir = "$out_dir\tests"
}

task local -depends prepare, tokenize-tests, test
task default -depends local


task clean {
    #code
    rd $prod_artefacts_dir -recurse -force  -ErrorAction SilentlyContinue | out-null
    mkdir $prod_artefacts_dir  -ErrorAction SilentlyContinue  | out-null
    
    #out dirs
    rd $out_dir -recurse -force  -ErrorAction SilentlyContinue | out-null
    mkdir "$out_dir\$prod" -ErrorAction SilentlyContinue  | out-null
    mkdir "$test_dir\$tests" -ErrorAction SilentlyContinue  | out-null
}

task version -depends clean {
	 $commitHashAndTimestamp = Get-GitCommitHashAndTimestamp
     $commitHash = Get-GitCommitHash
     $timestamp = Get-GitTimestamp
     $branchName = Get-GitBranchOrTag
	 
	 $assemblyInfos = Get-ChildItem -Path $base_dir -Recurse -Filter AssemblyInfo.cs

	 $assemblyInfo = gc "$base_dir\AssemblyInfo.pson" | Out-String | iex
	 $version = $assemblyInfo.Version
	 #$productName = $assemblyInfo.ProductName
	 $companyName = $assemblyInfo.CompanyName
	 $copyright = $assemblyInfo.Copyright

	 try {
        foreach ($assemblyInfo in $assemblyInfos) {
            $path = Resolve-Path $assemblyInfo.FullName -Relative
            #Write-Host "Patching $path with product information."
            Patch-AssemblyInfo $path $Version $Version $branchName $commitHashAndTimestamp $companyName $copyright
        }         
    } catch {
        foreach ($assemblyInfo in $assemblyInfos) {
            $path = Resolve-Path $assemblyInfo.FullName -Relative
            Write-Host "Reverting $path to original state."
            & { git checkout --quiet $path }
        }
    }	
}

task compile -depends version {
	try{
		exec { msbuild $prod_dir\$solution /t:Clean /t:Build /p:Configuration=$config /v:q /nologo }
	} finally{
		$assemblyInfos = Get-ChildItem -Path $base_dir -Recurse -Filter AssemblyInfo.cs
		foreach ($assemblyInfo in $assemblyInfos) {
            $path = Resolve-Path $assemblyInfo.FullName -Relative
            Write-Verbose "Reverting $path to original state."
            & { git checkout --quiet $path }
        }
	}
}

task prepare -depends compile {       
    exec {
        copy-item $prod_artefacts_dir\* $out_dir\$prod\ 
    }
            
    exec {
        copy-item $prod_test_dir\* $test_dir\$tests\
    }
}

task tokenize-tests {
    $env_dir = "$base_dir\env\$env"
    
    exec {
        & "$tools_dir\config-transform\config-transform.exe" "$test_dir\$tests\$tests.dll.config" "$env_dir\$tests\App.$config.config"
    }
}

task mergedlls -depends prepare {
    mkdir "$out_dir\$prod\ilmerged"
    exec {
        & $tools_dir\ilmerge\tools\ilmerge.exe /internalize /out:$out_dir\$prod\ilmerged\Togglez.dll $out_dir\$prod\Togglez.dll $out_dir\$prod\ZooKeeperNet.dll $out_dir\$prod\log4net.dll $out_dir\$prod\Newtonsoft.Json.dll
    }
}

task test {    
    $testassemblies = get-childitem "$test_dir\$tests" -recurse -include *tests*.dll
    mkdir $test_results_dir  -ErrorAction SilentlyContinue  | out-null
    exec { 
        & $tools_dir\NUnit2.6.3\nunit-console-x86.exe $testassemblies /nologo /nodots /xml="$test_results_dir\test_results.xml"; 
    }
}

task nuget -depends build-nuget, publish-nuget

task build-nuget -depends mergedlls {
	try{
		Push-Location "$prod_dir\$nugetName"
		#exec { & "$prod_dir\.nuget\NuGet.exe" "spec"}
		exec { & "$prod_dir\.nuget\nuget.exe" pack $base_dir\nuget\Togglez.nuspec }
	} finally{
		Pop-Location
		$assemblyInfos = Get-ChildItem -Path $base_dir -Recurse -Filter AssemblyInfo.cs
		foreach ($assemblyInfo in $assemblyInfos) {
            $path = Resolve-Path $assemblyInfo.FullName -Relative
            #Write-Verbose "Reverting $path to original state."
            & { git checkout --quiet $path }
        }
	}	
}

task publish-nuget -depends build-nuget {
	$pkgPath = Get-ChildItem -Path "$prod_dir\$nugetName" -Filter "*.nupkg" | select-object -first 1
	exec { & "$prod_dir\.nuget\nuget.exe" push "$prod_dir\$nugetName\$pkgPath" }
	ri "$prod_dir\$nugetName\$pkgPath"
}