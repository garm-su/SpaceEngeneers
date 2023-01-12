# using: .\build.ps1 StatusListener

$file = $args[0]
$infile = "../" + $file + "/*.cs"
$outfile = $file + ".igs"

Write-Output $file
(get-content $infile) -replace 'public new ([a-z]+) Log', 'public $1 Log' | Where-Object { $_ -notmatch "^using|\/\/@remove$|^[\s]*\/\/" } | out-file $outfile
get-content ../Libraries/*.cs | Where-Object { $_ -notmatch "^using|\/\/@remove$|^[\s]*\/\/" } | out-file -append $outfile