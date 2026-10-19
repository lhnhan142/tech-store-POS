Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$artifactDir = "C:\Users\Truong\.gemini\antigravity-ide\brain\2f158bda-8c93-4f2a-8391-76ef18fd7764"

# Launch GUI app
$proc = Start-Process -FilePath "C:\Users\Truong\Downloads\BTLLTTQ\PhuKienMayTinh.GUI\bin\Debug\net10.0-windows\PhuKienMayTinh.GUI.exe" -PassThru

# Wait 3 seconds for window to load and populate data
Start-Sleep -Seconds 4

# Capture full screen
$screen = [System.Windows.Forms.Screen]::PrimaryScreen
$bounds = $screen.Bounds
$bmp = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$gfx = [System.Drawing.Graphics]::FromImage($bmp)
$gfx.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)

$outputPath = Join-Path $artifactDir "dashboard_fixed.png"
$bmp.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)

$gfx.Dispose()
$bmp.Dispose()

# Close process
if ($proc -and -not $proc.HasExited) {
    $proc.Kill()
}

Write-Output "Captured screenshot to $outputPath"
