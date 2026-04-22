[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$script:RepoRoot = (Resolve-Path -LiteralPath (Join-Path $script:ScriptDir "..\..")).Path
$script:IgnoredDirectories = @(
    ".git", "bin", "obj",
    "Library", "Temp", "Logs", "UserSettings", "MemoryCaptures",
    "Packages",
    "Build", "Builds"
)
$script:Violations = [System.Collections.Generic.List[string]]::new()

function Get-RepoRelativePath
{
    param([Parameter(Mandatory = $true)][string]$Path)

    $resolved = (Resolve-Path -LiteralPath $Path).Path
    if ($resolved.StartsWith($script:RepoRoot, [System.StringComparison]::OrdinalIgnoreCase))
    {
        return $resolved.Substring($script:RepoRoot.Length).TrimStart([char[]]@("\", "/"))
    }

    return $resolved
}

function Test-IsIgnoredPath
{
    param([Parameter(Mandatory = $true)][string]$Path)

    $relative = $Path.Substring($script:RepoRoot.Length).TrimStart([char[]]@("\", "/"))
    $parts = $relative -split "[\\/]"

    foreach ($part in $parts)
    {
        if ($script:IgnoredDirectories -contains $part)
        {
            return $true
        }
    }

    return $false
}

function Add-Violation
{
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][int]$Line,
        [Parameter(Mandatory = $true)][string]$Message
    )

    $relative = Get-RepoRelativePath -Path $Path
    $script:Violations.Add(("{0}:{1}: {2}" -f $relative, $Line, $Message))
}

function Test-DotnetSdkAvailable
{
    try
    {
        $sdks = @(& dotnet --list-sdks 2>$null)
        return ($LASTEXITCODE -eq 0 -and $null -ne $sdks -and $sdks.Count -gt 0)
    }
    catch
    {
        return $false
    }
}

function Get-FirstFormatWorkspace
{
    $solutions = @(Get-ChildItem -LiteralPath $script:RepoRoot -Filter "*.sln" -File -ErrorAction SilentlyContinue)
    if ($solutions.Count -gt 0)
    {
        return $solutions[0].FullName
    }

    $projects = @(Get-ChildItem -LiteralPath $script:RepoRoot -Filter "*.csproj" -File -Recurse -ErrorAction SilentlyContinue |
        Where-Object { -not (Test-IsIgnoredPath -Path $_.FullName) })

    if ($projects.Count -gt 0)
    {
        return $projects[0].FullName
    }

    return $null
}

function Test-CrlfLineEndings
{
    param([Parameter(Mandatory = $true)][System.IO.FileInfo]$File)

    $text = [System.IO.File]::ReadAllText($File.FullName)
    $match = [regex]::Match($text, "(?<!`r)`n")
    if ($match.Success)
    {
        $line = (($text.Substring(0, $match.Index) -split "`n").Count)
        Add-Violation -Path $File.FullName -Line $line -Message "expected CRLF line endings for C# files"
    }
}

function Test-PascalCase
{
    param([Parameter(Mandatory = $true)][string]$Name)

    return $Name -cmatch "^[A-Z][A-Za-z0-9]*$"
}

function Test-PrivateFieldName
{
    param([Parameter(Mandatory = $true)][string]$Name)

    return $Name -cmatch "^_[a-z][A-Za-z0-9]*$"
}

function Test-UpperSnakeCase
{
    param([Parameter(Mandatory = $true)][string]$Name)

    return $Name -cmatch "^[A-Z][A-Z0-9]*(?:_[A-Z0-9]+)*$"
}

function Test-CSharpNaming
{
    param([Parameter(Mandatory = $true)][System.IO.FileInfo]$File)

    $lines = @(Get-Content -LiteralPath $File.FullName)
    $typeNames = @()
    $fileBaseName = [System.IO.Path]::GetFileNameWithoutExtension($File.Name)

    for ($index = 0; $index -lt $lines.Count; $index++)
    {
        $lineNumber = $index + 1
        $line = $lines[$index]
        $code = ($line -split "//", 2)[0]

        if ($line.Contains("`t"))
        {
            Add-Violation -Path $File.FullName -Line $lineNumber -Message "use spaces for indentation; tabs are not allowed"
        }

        $typeMatch = [regex]::Match(
            $code,
            "^\s*(?:(?:public|internal|private|protected|sealed|abstract|static|partial|readonly|unsafe|new)\s+)*(?:class|struct|interface|enum|record(?:\s+(?:class|struct))?)\s+([A-Za-z_][A-Za-z0-9_]*)\b"
        )
        if ($typeMatch.Success)
        {
            $typeName = $typeMatch.Groups[1].Value
            $typeNames += $typeName
            if (-not (Test-PascalCase -Name $typeName))
            {
                Add-Violation -Path $File.FullName -Line $lineNumber -Message ("type '{0}' must be PascalCase" -f $typeName)
            }
        }

        $delegateMatch = [regex]::Match(
            $code,
            "^\s*(?:(?:public|internal|private|protected|static|unsafe|new)\s+)*delegate\s+\S+\s+([A-Za-z_][A-Za-z0-9_]*)\s*\("
        )
        if ($delegateMatch.Success)
        {
            $delegateName = $delegateMatch.Groups[1].Value
            $typeNames += $delegateName
            if (-not (Test-PascalCase -Name $delegateName))
            {
                Add-Violation -Path $File.FullName -Line $lineNumber -Message ("delegate '{0}' must be PascalCase" -f $delegateName)
            }
        }

        $constMatch = [regex]::Match(
            $code,
            "^\s*(?:(?:public|internal|private|protected)\s+)?const\s+[A-Za-z0-9_<>,\[\]\.?]+\s+([A-Za-z_][A-Za-z0-9_]*)\b"
        )
        if ($constMatch.Success)
        {
            $constName = $constMatch.Groups[1].Value
            if (-not (Test-UpperSnakeCase -Name $constName))
            {
                Add-Violation -Path $File.FullName -Line $lineNumber -Message ("constant '{0}' must be UPPER_SNAKE_CASE" -f $constName)
            }
        }

        $privateFieldMatch = [regex]::Match(
            $code,
            "^\s*(?:\[[^\]]+\]\s*)*private\s+(?!(?:const|class|struct|interface|enum|record|delegate)\b)(?:(?:static|readonly|volatile)\s+)*[A-Za-z0-9_<>,\[\]\.?]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*(?:=|;)"
        )
        if ($privateFieldMatch.Success)
        {
            $privateFieldName = $privateFieldMatch.Groups[1].Value
            if (-not (Test-PrivateFieldName -Name $privateFieldName))
            {
                Add-Violation -Path $File.FullName -Line $lineNumber -Message ("private field '{0}' must be _camelCase" -f $privateFieldName)
            }
        }

        $publicFieldMatch = [regex]::Match(
            $code,
            "^\s*(?:\[[^\]]+\]\s*)*public\s+(?!(?:const|class|struct|interface|enum|record|delegate)\b)(?:(?:static|readonly|volatile)\s+)*[A-Za-z0-9_<>,\[\]\.?]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*(?:=|;)"
        )
        if ($publicFieldMatch.Success)
        {
            $publicFieldName = $publicFieldMatch.Groups[1].Value
            if (-not (Test-PascalCase -Name $publicFieldName))
            {
                Add-Violation -Path $File.FullName -Line $lineNumber -Message ("public field '{0}' must be PascalCase" -f $publicFieldName)
            }
        }

        $methodMatch = [regex]::Match(
            $code,
            "^\s*(?:(?:public|private|protected|internal)\s+)?(?:(?:static|virtual|override|async|sealed|new|partial)\s+)*[A-Za-z0-9_<>,\[\]\.?]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*\("
        )
        if ($methodMatch.Success)
        {
            $methodName = $methodMatch.Groups[1].Value
            if (-not (Test-PascalCase -Name $methodName))
            {
                Add-Violation -Path $File.FullName -Line $lineNumber -Message ("method '{0}' must be PascalCase" -f $methodName)
            }
        }
    }

    if ($typeNames.Count -gt 0 -and $typeNames[0] -ne $fileBaseName)
    {
        Add-Violation -Path $File.FullName -Line 1 -Message ("first type '{0}' must match file name '{1}'" -f $typeNames[0], $File.Name)
    }
}

Push-Location $script:RepoRoot
try
{
    $editorConfigPath = Join-Path $script:RepoRoot ".editorconfig"
    if (-not (Test-Path -LiteralPath $editorConfigPath))
    {
        Write-Error ".editorconfig not found at repository root."
        exit 1
    }

    $csFiles = @(Get-ChildItem -LiteralPath $script:RepoRoot -Filter "*.cs" -File -Recurse -ErrorAction SilentlyContinue |
        Where-Object { -not (Test-IsIgnoredPath -Path $_.FullName) })

    if ($csFiles.Count -eq 0)
    {
        Write-Host "No .cs files found. Nothing to format-check."
        exit 0
    }

    $workspace = Get-FirstFormatWorkspace
    $hasDotnetSdk = Test-DotnetSdkAvailable

    if ($null -ne $workspace -and $hasDotnetSdk)
    {
        Write-Host ("Running dotnet format --verify-no-changes on {0}." -f (Get-RepoRelativePath -Path $workspace))
        & dotnet format $workspace --verify-no-changes --verbosity minimal
        if ($LASTEXITCODE -ne 0)
        {
            exit $LASTEXITCODE
        }
    }
    else
    {
        if ($null -eq $workspace)
        {
            Write-Host "No .sln or .csproj found. Running standalone .editorconfig checks."
        }
        elseif (-not $hasDotnetSdk)
        {
            Write-Host "No .NET SDK found. Running standalone .editorconfig checks."
        }
    }

    foreach ($file in $csFiles)
    {
        Test-CrlfLineEndings -File $file
        Test-CSharpNaming -File $file
    }

    if ($script:Violations.Count -gt 0)
    {
        Write-Host "Format verification failed:"
        foreach ($violation in $script:Violations)
        {
            Write-Host ("  {0}" -f $violation)
        }

        exit 1
    }

    Write-Host ("Format verification passed for {0} C# file(s)." -f $csFiles.Count)
    exit 0
}
finally
{
    Pop-Location
}
