# using: .\build.ps1 StatusListener

$file = $args[0]
$infile = "../" + $file + "/*.cs"
$outfile = $file + ".igs"
$minfile = $file + ".min.igs"

Write-Output $file
(get-content $infile -Encoding UTF8) -replace 'public new ([a-z]+) Log', 'public $1 Log' | Where-Object { $_ -notmatch "^using|\/\/@remove$|^[\s]*\/\/" } | out-file -Encoding UTF8 $outfile
get-content ../Libraries/*.cs -Encoding UTF8 | Where-Object { $_ -notmatch "^using|\/\/@remove$|^[\s]*\/\/" } | out-file -Encoding UTF8 -append $outfile
.\min\csminify.exe $outfile > $minfile