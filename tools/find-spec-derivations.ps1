# tools/find-spec-derivations.ps1
# HANDOFF.md v1.0.47. Phase 5B is non-C# and adds PHASE5B=0. Phase 5B marker reconciliation updates PHASE1F to the stitched inventory.
param([Parameter(Mandatory=$true)][string]$Root)
$ErrorActionPreference='Stop'
# Phase 1F is six current source markers after removal of the obsolete
# Phase 1G-replaced reminder placeholder; keep the inventory aligned with
# authoritative source files rather than retaining a dead marker.
$expected=[ordered]@{
 'SPEC-DERIVED-MSG2'=5;'SPEC-DERIVED-MSG3'=4;'SPEC-DERIVED-MSG4'=2;'SPEC-DERIVED-MSG5'=3;'SPEC-DERIVED-MSG6'=3;
 'SPEC-DERIVED-PHASE1A'=2;'SPEC-DERIVED-PHASE1B'=5;'SPEC-DERIVED-PHASE1C'=6;'SPEC-DERIVED-PHASE1D'=7;'SPEC-DERIVED-PHASE1D-MSG2'=4;
 'SPEC-DERIVED-PHASE1E'=5;'SPEC-DERIVED-PHASE1E-MSG2'=2;'SPEC-DERIVED-PHASE1F'=6;'SPEC-DERIVED-PHASE1F-MSG2'=1;
 'SPEC-DERIVED-PHASE1G'=10;'SPEC-DERIVED-PHASE1G-MSG2'=6;'SPEC-DERIVED-PHASE1H'=4;'SPEC-DERIVED-PHASE2A'=5;'SPEC-DERIVED-PHASE2B'=8;'SPEC-DERIVED-PHASE2C'=6;'SPEC-DERIVED-PHASE2D'=4;'SPEC-DERIVED-PHASE2E'=10;'SPEC-DERIVED-PHASE2F'=8;'SPEC-DERIVED-PHASE2G'=11;'SPEC-DERIVED-PHASE3A'=7;'SPEC-DERIVED-PHASE3B'=9;'SPEC-DERIVED-PHASE3C'=7;'SPEC-DERIVED-PHASE3D'=10;'SPEC-DERIVED-PHASE3E'=14;'SPEC-DERIVED-PHASE3F'=2;'SPEC-DERIVED-PHASE4A'=1;'SPEC-DERIVED-PHASE4B'=1;'SPEC-DERIVED-PHASE4C'=0;'SPEC-DERIVED-PHASE4D'=0;'SPEC-DERIVED-PHASE5A'=0;'SPEC-DERIVED-PHASE5B'=0
}
$expectedTotal=($expected.Values|Measure-Object -Sum).Sum
$csFiles=Get-ChildItem -Path $Root -Recurse -Include '*.cs' -File -ErrorAction Stop
$failed=$false;$grandTotal=0
foreach($marker in $expected.Keys){$hits=@();foreach($file in $csFiles){$contents=Get-Content -LiteralPath $file.FullName -Raw;if($contents -match [regex]::Escape($marker)){$hits+=$file.FullName}};$distinct=($hits|Sort-Object -Unique).Count;$grandTotal+=$distinct;if($distinct -eq $expected[$marker]){Write-Host ("OK {0} {1}" -f $marker,$distinct)}else{$failed=$true;Write-Host ("FAIL {0} expected={1} actual={2}" -f $marker,$expected[$marker],$distinct) -ForegroundColor Red}}
Write-Host ("Grand total: {0} expected {1}" -f $grandTotal,$expectedTotal)
if($failed -or $grandTotal -ne $expectedTotal){exit 1}
exit 0
