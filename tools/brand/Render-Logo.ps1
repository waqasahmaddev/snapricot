<#
.SYNOPSIS
  Renders the Snapricot logo PNGs. The geometry mirrors docs/brand/snapricot-logo.svg (512x512 viewBox);
  keep both in sync. Windows only (System.Drawing).
.EXAMPLE
  pwsh tools/brand/Render-Logo.ps1
#>
param([string]$Root = (Resolve-Path "$PSScriptRoot/../.."))

Add-Type -AssemblyName System.Drawing
function Color($hex) { [System.Drawing.ColorTranslator]::FromHtml($hex) }
function Brush($hex) { New-Object System.Drawing.SolidBrush (Color $hex) }
function RoundPen($hex, $width) {
    $pen = New-Object System.Drawing.Pen (Color $hex), $width
    $pen.StartCap = 'Round'; $pen.EndCap = 'Round'
    $pen
}

# Palette (docs/product/brand.md)
$Apricot = '#F4943C'; $Deep = '#D9622B'; $Flesh = '#FFC98F'; $Leaf = '#3E9B6A'; $Stem = '#6B4A2B'

function Draw-Logo([System.Drawing.Graphics]$g, [bool]$small) {
    # Fruit: circle (center 236,290 r 190) minus the top-right quarter
    $g.FillPie((Brush $Apricot), 46, 100, 380, 380, 0, 270)
    # Seam
    $g.DrawBezier((RoundPen $Deep 14), 228, 140, 166, 230, 166, 350, 216, 440)
    # Snapped slice, pulled out up-right
    $g.FillPie((Brush $Flesh), 80, 66, 380, 380, 270, 90)
    if (-not $small) {
        # Dashed selection frame around the slice (dropped at small sizes, where it turns to noise)
        $frame = RoundPen $Deep 12
        $frame.DashPattern = @(1.6, 1.1)
        $g.DrawRectangle($frame, 256, 52, 218, 218)
    }
    # Stem and leaf
    $g.DrawLine((RoundPen $Stem 14), 230, 108, 210, 60)
    $leafPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $leafPath.AddBezier(212, 66, 116, 10, 66, 80, 86, 110)
    $leafPath.AddBezier(86, 110, 136, 120, 186, 100, 212, 66)
    $g.FillPath((Brush $Leaf), $leafPath)
}

function Save-Logo([string]$path, [int]$size, [bool]$small) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.PixelOffsetMode = 'HighQuality'
    $g.Clear([System.Drawing.Color]::Transparent)
    $g.ScaleTransform($size / 512, $size / 512)
    Draw-Logo $g $small
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    Write-Host "wrote $path"
}

$brand = Join-Path $Root 'docs/brand'
$assets = Join-Path $Root 'src/Snapricot.App/Assets'
New-Item -ItemType Directory -Force $brand, $assets | Out-Null

Save-Logo (Join-Path $brand 'snapricot-logo-512.png') 512 $false
Save-Logo (Join-Path $brand 'snapricot-mark-64.png') 64 $true
Save-Logo (Join-Path $assets 'snapricot.png') 256 $false        # window icon, sidebar, About
Save-Logo (Join-Path $assets 'snapricot-tray.png') 64 $true     # tray / menu bar (small mark)
