#数据目录
$path = '.\datas'
#csv格式数据解析
$content = Import-Csv -Path $path'\ster\2.csv' -Delimiter ';' -Encoding UTF7
$content | Select-Object -First 10 | Format-Table  >1.txt #Export-Csv -NoTypeInformation -Encoding UTF8 22.csv 

#txt格式数据解析
$content = Get-Content $path'\ster\4.TXT' -Encoding UTF8
$content = $content | Where-Object { !([string]::IsNullOrWhiteSpace($_)) }
$begLine = $content| Select-String  '- TIME'
$endLine = $content| Select-String  '^LOAD'
$cnt = $endLine.LineNumber - $begLine.LineNumber -1
$content = $content| Select-String  "- TIME" -Context 0,$cnt
$content.Context.PostContext | Select-String -NotMatch '--' |ForEach-Object{
    $_ -replace ' +',' '
} | ConvertFrom-Csv -Delimiter ' ' -Header step,time,tem,val


#get file content
$content = Get-Content $path'\wash\1.TXT' -Encoding UTF7
$header = $content | Select-string machine -Context 2,3 | ConvertFrom-Csv -Delimiter ':' -Header name,value
$header | foreach{
     if($_.name.StartsWith('>'))
     {
        Write-Warning $_.value
     }
}

$content = $content | Where-Object { !([string]::IsNullOrWhiteSpace($_))}
$content| Select-String  "-> " -Context 0,3| ConvertFrom-Csv -Delimiter ' ' | Format-Table *




foreach($line in $content)
{
    $data = $line.Trim(" ").Split(' ',[StringSplitOptions]::RemoveEmptyEntries)
    foreach($cell in $data)
    {
        Write-Host $cell '&' -NoNewline
    }
    Write-Host ' '
}
