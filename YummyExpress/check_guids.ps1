$sceneGuids = @()
$scene = Select-String -Path 'd:\YummyExpress\YummyExpress\Assets\UI.unity' -Pattern 'm_Script: {fileID: 11500000, guid: ([a-f0-9]+)'
foreach ($hit in $scene) {
    $sceneGuids += $hit.Matches[0].Groups[1].Value
}
$sceneGuids = $sceneGuids | Sort-Object -Unique

$metaGuids = @()
Get-ChildItem -Path 'd:\YummyExpress\YummyExpress\Assets\Scripts' -Recurse -Filter '*.cs.meta' | ForEach-Object {
    $m = Select-String -Path $_.FullName -Pattern 'guid: ([a-f0-9]+)'
    if ($m) { $metaGuids += $m.Matches[0].Groups[1].Value }
}

Write-Output '=== Scene GUIDs ==='
$sceneGuids
Write-Output ''
Write-Output '=== Missing (in scene but NOT in any .cs.meta) ==='
$missing = @($sceneGuids | Where-Object { $_ -notin $metaGuids })
if ($missing.Count -gt 0) { $missing } else { Write-Output '(none)' }
