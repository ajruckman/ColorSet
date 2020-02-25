function Clean-DotNETProject
{
    Get-ChildItem .\ -include bin, obj -Recurse | foreach ($_) {
        Write-Output "Remove: $($_.FullName)"
        Remove-Item $_.FullName -Force -Recurse
    }
}

Clean-DotNETProject
dotnet restore

cd .\ColorSet\

cd .\ColorGenerator\
npm install
cd ..

dotnet run
cd ..

dotnet pack -c Debug

if (!(Test-Path .\_published\))
{
    md .\_published\
}
rm -Force .\_published\*

Get-ChildItem -Directory | foreach {
	if (Test-Path "$($_.FullName)\bin") {
		Get-ChildItem "$($_.FullName)\bin\" -Depth 1 -Filter *.nupkg | foreach {
			Write-Output "Copying package: $($_.Name)"
			Copy-Item $_.FullName .\_published\
		}
	}
}

Remove-Item -ErrorAction Ignore -Force -Recurse $HOME\.nuget\packages\colorset\
