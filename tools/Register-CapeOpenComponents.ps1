<#
.SYNOPSIS
    Registers CAPE-OPEN .NET 8 ComHost-hosted assemblies fully (CLSID + CAPE-OPEN
    CATIDs + CapeDescription keys), as a replacement for what RegAsm + the
    [ComRegisterFunction] callback used to do on .NET Framework.

.DESCRIPTION
    .NET 8 EnableComHosting only writes the bare CLSID/InprocServer32 key. The
    [ComRegisterFunction] callback in CapeUnitBase is NOT invoked. This script
    reflectively scans the assembly, finds every [ComVisible(true)] type with a
    [Guid], and writes the CAPE-OPEN-specific registry entries that simulators
    such as Aspen Plus and COFE require to discover the unit operation.

.PARAMETER AssemblyPaths
    Paths to the managed .dll files to scan (NOT the *.comhost.dll). The
    *.comhost.dll must already be regsvr32-registered before running this.

.PARAMETER Unregister
    If set, removes only the CAPE-OPEN extra keys (CapeDescription +
    Implemented Categories). Use regsvr32 /u to remove the base CLSID.

.EXAMPLE
    pwsh .\tools\Register-CapeOpenComponents.ps1 `
        -AssemblyPaths @('CapeOpen\bin\Debug\net8.0-windows\CapeOpen.dll',
                         'Test\bin\Debug\net8.0-windows\Test.dll')
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string[]]$AssemblyPaths,
    [switch]$Unregister
)

# --- CAPE-OPEN CATID constants (from CapeOpen\Interfaces\COGuids1.cs) ---
$CATID = @{
    CapeOpenComponent           = '{678c09a1-7d66-11d2-a67d-00105a42887f}'
    CapeUnitOperation           = '{678c09a5-7d66-11d2-a67d-00105a42887f}'
    CapeThermoSystem            = '{678c09a3-7d66-11d2-a67d-00105a42887f}'
    CapeThermoPropertyPackage   = '{678c09a4-7d66-11d2-a67d-00105a42887f}'
    CapeThermoEquilibriumServer = '{678c09a6-7d66-11d2-a67d-00105a42887f}'
    CATID_MONITORING_OBJECT     = '{7BA1AF89-B2E4-493d-BD80-2970BF4CBE99}'
    Consumes_Thermo             = '{4150C28A-EE06-403f-A871-87AFEC38A249}'
    SupportsThermodynamics10    = '{0D562DC8-EA8E-4210-AB39-B66513C0CD09}'
    SupportsThermodynamics11    = '{4667023A-5A8E-4cca-AB6D-9D78C5112FED}'
}

# Check admin
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) { throw "Must run elevated (admin)." }

function Get-AttrNamedArg($attrData, $attrShortName) {
    # Read first ctor argument string from a CustomAttributeData whose type name ends with $attrShortName
    foreach ($a in $attrData) {
        if ($a.AttributeType.Name -eq $attrShortName -or $a.AttributeType.Name -eq "${attrShortName}Attribute") {
            if ($a.ConstructorArguments.Count -gt 0) {
                return [string]$a.ConstructorArguments[0].Value
            }
        }
    }
    return $null
}

function Has-Attr($attrData, $attrShortName) {
    foreach ($a in $attrData) {
        if ($a.AttributeType.Name -eq $attrShortName -or $a.AttributeType.Name -eq "${attrShortName}Attribute") {
            # Attribute may have a single bool ctor arg; treat presence as true unless arg is explicit false
            if ($a.ConstructorArguments.Count -gt 0 -and $a.ConstructorArguments[0].Value -is [bool]) {
                return [bool]$a.ConstructorArguments[0].Value
            }
            return $true
        }
    }
    return $false
}

function Process-Assembly($asmPath) {
    $asmPath = (Resolve-Path $asmPath).Path
    Write-Host "`n=== $($Unregister.IsPresent ? 'Unregistering' : 'Registering') CAPE-OPEN entries from $asmPath ===" -ForegroundColor Cyan

    # MetadataLoadContext: load for reflection without locking / without resolving dependencies.
    Add-Type -AssemblyName System.Reflection.MetadataLoadContext
    $coreAsmDir = [System.IO.Path]::GetDirectoryName([object].Assembly.Location)
    $runtimeAsms = [System.IO.Directory]::GetFiles($coreAsmDir, '*.dll')
    $asmDir = [System.IO.Path]::GetDirectoryName($asmPath)
    $localAsms = [System.IO.Directory]::GetFiles($asmDir, '*.dll')
    $allAsms = New-Object 'System.Collections.Generic.List[string]'
    $allAsms.AddRange([string[]]$runtimeAsms)
    $allAsms.AddRange([string[]]$localAsms)
    $resolver = [System.Reflection.PathAssemblyResolver]::new($allAsms)
    $mlc = [System.Reflection.MetadataLoadContext]::new($resolver)
    try {
        $asm = $mlc.LoadFromAssemblyPath($asmPath)
        $asmVersion = $asm.GetName().Version.ToString()
        $codeBase = "file:///$($asmPath -replace '\\','/')"

        foreach ($t in $asm.GetExportedTypes()) {
            if ($t.IsAbstract -or $t.IsInterface) { continue }
            $attrs = $t.GetCustomAttributesData()

            # Effective ComVisible: explicit on type wins; otherwise inherit assembly default (true here).
            $explicitComVisible = $null
            foreach ($a in $attrs) {
                if ($a.AttributeType.Name -in @('ComVisibleAttribute','ComVisible') -and $a.ConstructorArguments.Count -gt 0) {
                    $explicitComVisible = [bool]$a.ConstructorArguments[0].Value; break
                }
            }
            $asmComVisible = $false
            foreach ($a in $asm.GetCustomAttributesData()) {
                if ($a.AttributeType.Name -in @('ComVisibleAttribute','ComVisible') -and $a.ConstructorArguments.Count -gt 0) {
                    $asmComVisible = [bool]$a.ConstructorArguments[0].Value; break
                }
            }
            $isComVisible = if ($null -ne $explicitComVisible) { $explicitComVisible } else { $asmComVisible }
            if (-not $isComVisible) { continue }

            # Need an explicit Guid (otherwise we'd be guessing CLSID)
            $guidStr = $null
            foreach ($a in $attrs) {
                if ($a.AttributeType.Name -in @('GuidAttribute','Guid') -and $a.ConstructorArguments.Count -gt 0) {
                    $guidStr = [string]$a.ConstructorArguments[0].Value; break
                }
            }
            if (-not $guidStr) { continue }
            $clsid = "{$guidStr}"
            $isUnitOp = Has-Attr $attrs 'CapeUnitOperation'
            $isMonitor = Has-Attr $attrs 'CapeFlowsheetMonitoring'

            # Only operate on Cape components: must look like a Cape* unit (has CapeUnitOperation OR derives from CapeUnitBase OR has CapeName attribute)
            $hasCapeName = Has-Attr $attrs 'CapeName'
            $derivesCapeUnitBase = $false
            $bt = $t.BaseType
            while ($null -ne $bt) {
                if ($bt.FullName -eq 'CapeOpen.CapeUnitBase' -or $bt.FullName -eq 'CapeOpen.CapeObjectBase') { $derivesCapeUnitBase = $true; break }
                $bt = $bt.BaseType
            }
            if (-not ($isUnitOp -or $hasCapeName -or $derivesCapeUnitBase)) { continue }

            Write-Host "  - $($t.FullName)  CLSID=$clsid"

            $clsidKeyPath = "Registry::HKEY_CLASSES_ROOT\CLSID\$clsid"
            $impCatPath   = "$clsidKeyPath\Implemented Categories"
            $capeDescPath = "$clsidKeyPath\CapeDescription"
            $inprocPath   = "$clsidKeyPath\InprocServer32"

            if ($Unregister) {
                if (Test-Path $impCatPath)   { Remove-Item -Path $impCatPath   -Recurse -Force -ErrorAction SilentlyContinue }
                if (Test-Path $capeDescPath) { Remove-Item -Path $capeDescPath -Recurse -Force -ErrorAction SilentlyContinue }
                Write-Host "      [unregistered CAPE keys]" -ForegroundColor Yellow
                continue
            }

            if (-not (Test-Path $clsidKeyPath)) {
                Write-Warning "      CLSID key missing — did you run regsvr32 on the .comhost.dll?"
                continue
            }

            # 1) Implemented Categories
            New-Item -Path $impCatPath -Force | Out-Null
            New-Item -Path "$impCatPath\$($CATID.CapeOpenComponent)" -Force | Out-Null
            if ($isUnitOp -or $derivesCapeUnitBase) {
                New-Item -Path "$impCatPath\$($CATID.CapeUnitOperation)" -Force | Out-Null
            }
            if ($isMonitor)                              { New-Item -Path "$impCatPath\$($CATID.CATID_MONITORING_OBJECT)" -Force | Out-Null }
            if (Has-Attr $attrs 'CapeConsumesThermo')    { New-Item -Path "$impCatPath\$($CATID.Consumes_Thermo)" -Force | Out-Null }
            if (Has-Attr $attrs 'CapeSupportsThermodynamics10') { New-Item -Path "$impCatPath\$($CATID.SupportsThermodynamics10)" -Force | Out-Null }
            if (Has-Attr $attrs 'CapeSupportsThermodynamics11') { New-Item -Path "$impCatPath\$($CATID.SupportsThermodynamics11)" -Force | Out-Null }

            # 2) Ensure InprocServer32 has CodeBase (some PMEs read it)
            if (Test-Path $inprocPath) {
                Set-ItemProperty -Path $inprocPath -Name 'CodeBase' -Value $codeBase -Force
            }

            # 3) CapeDescription metadata
            $name        = (Get-AttrNamedArg $attrs 'CapeName')        ?? $t.FullName
            $description = (Get-AttrNamedArg $attrs 'CapeDescription') ?? ''
            $version     = (Get-AttrNamedArg $attrs 'CapeVersion')     ?? ''
            $vendorUrl   = (Get-AttrNamedArg $attrs 'CapeVendorURL')   ?? ''
            $helpUrl     = (Get-AttrNamedArg $attrs 'CapeHelpURL')     ?? ''
            $about       = (Get-AttrNamedArg $attrs 'CapeAbout')       ?? ''

            New-Item -Path $capeDescPath -Force | Out-Null
            Set-ItemProperty -Path $capeDescPath -Name 'Name'             -Value $name        -Force
            Set-ItemProperty -Path $capeDescPath -Name 'Description'      -Value $description -Force
            Set-ItemProperty -Path $capeDescPath -Name 'CapeVersion'      -Value $version     -Force
            Set-ItemProperty -Path $capeDescPath -Name 'ComponentVersion' -Value $asmVersion  -Force
            Set-ItemProperty -Path $capeDescPath -Name 'VendorURL'        -Value $vendorUrl   -Force
            Set-ItemProperty -Path $capeDescPath -Name 'HelpURL'          -Value $helpUrl     -Force
            Set-ItemProperty -Path $capeDescPath -Name 'About'            -Value $about       -Force

            Write-Host "      Name=$name  CapeUnitOp=$($isUnitOp -or $derivesCapeUnitBase)" -ForegroundColor Green
        }
    }
    finally {
        $mlc.Dispose()
    }
}

foreach ($p in $AssemblyPaths) { Process-Assembly $p }
Write-Host "`nDone." -ForegroundColor Cyan
