using namespace System.IO
using namespace System.Collections.Generic

$softwareName = Read-Host "Please enter the software name."

# Replace occurrences of "SoftwareName" in all files
Get-ChildItem -File -Recurse | ForEach-Object {
    try {
        (Get-Content $_.FullName) -replace 'SoftwareName', $softwareName | Set-Content $_.FullName
    } 
    catch {}
}

# Rename files and folders
$stack = [Stack[string]]::new()
$allPaths = [List[string]]::new()


# Get all files and directories containing "SoftwareName" recursively
Get-ChildItem -Recurse -Directory | ForEach-Object {
    $dirpath = $_.FullName
    $dirname = Split-Path  $dirpath -Leaf
    if ($dirname.Contains("SoftwareName"))
    {
        Write-Host "dirpath: " $dirpath
        $stack.Push($dirpath)
        $allPaths.Add($dirpath)
    }

    # Write-Host $_.FullName

    foreach ($file in [Directory]::EnumerateFiles($dirpath)) 
    {
        $filename = [Path]::GetFileName($file)
        if ($filename.Contains('SoftwareName') -and -not $allPaths.Contains($file))
        {
            Write-Host "filepath: " $file
            $stack.Push($file)
            $allPaths.Add($file)
        }
    }
}

# Add root files
Get-ChildItem -File | ForEach-Object {
    if ($_.FullName.Contains("SoftwareName")) {
        Write-Host "filepath: " $_.FullName
        $stack.Push($_.FullName)
    }
}

# Rename files and folders
while ($stack.Count) {
    $poppedFullName = $stack.Pop()
    $pathExists = (-not ([string]::IsNullOrEmpty($poppedFullName))) -and (Test-Path -Path $poppedFullName)

    $filename = [Path]::GetFileName($poppedFullName)

    if($filename.Contains('SoftwareName') -and $pathExists)
    {
        $newName = $filename.Replace('SoftwareName', $softwareName)

        Write-Host "Renaming: " $poppedFullName " to: " $newName

        Rename-Item -LiteralPath $poppedFullName -NewName $newName #-WhatIf
    }
}