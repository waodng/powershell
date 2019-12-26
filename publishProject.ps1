#获取MSBuild.exe路径
function Get-MsBuildPath
{
    $msBuildRegPath = "HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\12.0"
    $msBuildPathRegItem = Get-ItemProperty $msBuildRegPath -Name "MSBuildToolsPath"
    $msBuildPath = $msBuildPathRegItem.MsBuildToolsPath + "msbuild.exe"
    return $msBuildPath
}

#当前路径
$cur = pwd
#日志文件保存路径
$savepath = $cur.ToString() +'\logs'
#$MsBuild = $env:systemroot + "\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
$MsBuild = Get-MsBuildPath

#部署项目业务逻辑
function LogDeployment
{
  #可设置参数
  #param([string]$filepath,[string]$deployDestination)
  Trap {
    "Trap到了异常: $($_.Exception.Message)";
    logError($_.Exception.Message)
    Break
  }

  if(-not (Test-Path $MsBuild))
  {
    logInfo 'msbuild编译程序不存在..'
    exit
  }

  logInfo '查找csproj项目文件'
  $proName = get-childitem -Filter *.csproj
  if($proName.Length -eq 0)
  {
    logInfo 'csproj项目文件不存在'
	exit
  }
   
  logInfo '查找publicxml配置文件'
  $pubxml = get-childitem -Filter *.pubxml -Recurse
  if($pubxml.Length -eq 0)
  {
    logInfo 'publicxml配置文件不存在'
	exit
  }

  logInfo '开始编译执行...'

  #&$MsBuild $proName.FullName /t:Rebuild /p:DeployOnBuild=true /flp:ErrorsOnly`;verbosity:minimal`;LogFile=$savepath\info.log`;Append`;Encoding=UTF-8 /p:PublishProfile=$pubxml 
  #iex -Command "& '$global_msBuildPath' '$project_path'"

  logInfo '编译执行结束...'  

  $path = get-childitem -Filter *.pubxml -Recurse
  $xml = [xml](get-content $path.FullName)
  
  $filetext =  "Deployed package to " + $xml.Project.PropertyGroup.publishUrl #+ " on " + $datetime
  logInfo($filetext)

  #本地拷贝
  #copyLocal $xml.Project.PropertyGroup.publishUrl,'D:\publish1'
  #远程拷贝
  CopyRemote $xml.Project.PropertyGroup.publishUrl '\\192.168.3.111\setup' $savepath\info.log
}

#复制文件
function copyLocal($src, $dest)
{
    logInfo "从文件 $src 复制到 $dest 开始..."
    xcopy $src $dest /e /h /y /i
    logInfo "从文件 $src 复制到 $dest 完成..."
}

#将脚本文件所在目录下的文件夹下的文件全部拷贝到站点目录下
# 服务器目录必须是共享的文件夹，可读写权限
function CopyRemote
{
    param($src,$dest,$logfile)
    $drive = "W:"
    $net = New-Object -com WScript.Network
    try
    {
        $Date = (Get-Date).ToString()
        #判断源文件是否存在
        if(!(Test-Path $src -PathType Container))
        {
            $msg = ($Date) + " 源目录不存在:" + ($src) 
            Write-Warning $msg
            if($logfile) {$msg | Out-File -filepath $logfile -Append -Encoding utf8}
            exit
        }
        if(Test-Path $drive)
        {
            $net.RemoveNetworkDrive($drive,$true,$true)
        }
        $net.mapnetworkdrive($drive, $dest, $true, $username, $password)
        #源文件
        $Files_S = Get-ChildItem $src -Recurse
        #目的文件夹
        $Files_D = Get-ChildItem $drive -Recurse
        #统计变量
        $sumCnt = $skipCnt = $copyCnt = 0
        
        $mes = ($Date) + " 开始拷贝远程地址:" + ($dest) 
        Write-Warning $mes
        if($logfile) {$mes | Out-File -filepath $logfile -Append -Encoding utf8}
        

        foreach ($File in $Files_S) {
        
            # 在目标中查找是否已存在源文件，不做覆盖
            $SameFile = $Files_D | Where-Object  -FilterScript { $_.Fullname -eq ($drive + ($File.Fullname).Substring(2)) }
            if ( $SameFile.Exists ) {
               #判断目标是否为目录，如果是目录则跳过，如果不跳过，则会创建一级空目录
               if($SameFile.PSIsContainer)
               {
                   $msg = ($Date) + " 跳过目录:" + ($File.Fullname) 
                   Write-Host $msg
                   if($logfile) {$msg | Out-File -filepath $logfile -Append -Encoding utf8}
                   continue
               }
               #统计数量
               $sumCnt = $sumCnt+1
               #判断目标文件、源文件的新旧情况，如果目标已存在文件的修改时间早于源文件，则重新拷贝覆盖
               If ($SameFile.lastwritetime -lt $File.lastwritetime)
               {
                   $msg = ($Date) + " 拷贝文件:" + ($File.Fullname) + "  " + [Math]::Ceiling($File.Length / 1024) + "KB" 
                   Write-Host $msg
                   if($logfile) {$msg | Out-File -filepath $logfile -Append -Encoding utf8}

                   copy-item $File.Fullname ($drive + ($File.Fullname).Substring(2)) -force
                   $copyCnt = $copyCnt+1
                   continue
               }
               #统计数量
               $skipCnt = $skipCnt+1
               $msg = ($Date) + " 跳过文件:" + ($File.Fullname) 
               Write-Host $msg
               if($logfile) {$msg | Out-File -filepath $logfile -Append -Encoding utf8}
            }   else  {#不存在，则复制
                if($File.PSIsContainer){
                    $msg =  ($Date) + " 创建目录:" + ($File.Fullname) 
                    Write-Host $msg
                    if($logfile) {$msg | Out-File -filepath $logfile -Append -Encoding utf8}
                } else {
                    $msg = ($Date) + " 拷贝文件:" + ($File.Fullname) + "  " + [Math]::Ceiling($File.Length / 1024) + "KB" 
                    Write-Host $msg
                    if($logfile) {$msg | Out-File -filepath $logfile -Append -Encoding utf8}
                }
                Copy-Item $File.Fullname ($drive + ($File.Fullname).Substring(2))
                #统计数量
                $sumCnt = $sumCnt+1
                $copyCnt = $copyCnt+1
            }
        }

        $net.RemoveNetworkDrive($drive,$true,$true)
        #统计信息
        $msg = ($Date) + " 总文件数：" + ($sumCnt) + " 跳过文件数："+ ($skipCnt) + " 拷贝文件数："+ ($copyCnt)
        Write-Host $msg -ForegroundColor Yellow
        if($logfile) {$msg | Out-File -filepath $logfile -Append -Encoding utf8}
    }
    catch{
        $net.RemoveNetworkDrive($drive,$true,$true)
        Write-Error $_.Exception.Message
        if($logfile) {$_.Exception| Out-File -filepath $logfile -Append -Encoding utf8}
    }
}

#开启防火墙的话还需要添加入站规则
function AddFirewallRule($name,$tcpPorts,$appName = $null,$serviceName = $null)
{
    try
    {
        $fw = New-Object -ComObject hnetcfg.fwpolicy2 
        $rule = New-Object -ComObject HNetCfg.FWRule
        $rule.Name = $name
        if ($appName -ne $null) { $rule.ApplicationName = $appName }
        if ($serviceName -ne $null) { $rule.serviceName = $serviceName }
        $rule.Protocol = 6 #NET_FW_IP_PROTOCOL_TCP
        $rule.LocalPorts = $tcpPorts
        $rule.Enabled = $true
        $rule.Grouping = "@firewallapi.dll,-23255"
        $rule.Profiles = 7 # all
        $rule.Action = 1 # NET_FW_ACTION_ALLOW
        $rule.EdgeTraversal = $false
        $fw.Rules.Add($rule)
        Write-Host "防火墙入站规则添加成功"
    }
    catch
    {
        Write-Error $_.Exception.Message
    }
}

#检查net版本
function CheckNetVersion {
    # To detect the .NET Framework whether exists in the registry
    $isExists = Test-Path "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\"
    if(!$isExists) {
        return $false
    } else {
        # Returns the current .NET Framework version
        $version = gci "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP" | sort pschildname -desc | select -fi 1 -exp pschildname
        return $version
    }
}
# iis 注册
function Registry {
    $is64bit = [IntPtr]::Size -eq 8  # To determine whether a system is 64-bit
    $isapiPath_32 = "$env:windir\Microsoft.NET\Framework\v4.0.30319\aspnet_isapi.dll"
    Set-Location "$env:windir\Microsoft.NET\Framework\v4.0.30319\"; .\aspnet_regiis.exe -i
    if($is64bit) {
        $isapiPath_64 = "$env:windir\Microsoft.NET\Framework64\v4.0.30319\aspnet_isapi.dll"
        Set-Location "$env:windir\Microsoft.NET\Framework64\v4.0.30319\"; .\aspnet_regiis.exe -i
    }
}

#设置权限
function SetSecurity($name,$path)
{
	$acl= get-acl $path
	$ar = new-object System.Security.AccessControl.FileSystemAccessRule("$name","ReadAndExecute","ContainerInherit,ObjectInherit","None","Allow")
	$acl.SetAccessRule($ar)
	set-acl $acl -path $path
}

#检查目录是否存在，不存在新建,并设置权限
function CheckDirectory($path)
{
　　$existsPath=test-path $path
　　if($existsPath -eq $false)
　　{
　　　　write-host "【$path】目录不存在，新建该目录"
　　　　new-item -path $path -type directory
　　}
　　#设置network service用户的权限
　　Write-Progress "正在设置目录权限，请稍候……"
　　SetSecurity "network service" $path
　　SetSecurity "everyone" $path
　　Write-Progress "completed" -Completed
}

#添加扩展名 $mime为哈希表类型 如$mimes = @{".a"="application/stream";".b"="application/stream";".c"="application/stream";}
function AddMime($mime)
{
    try
    {
        if($mimes -eq $null -or $mimes.count -le 0)
        {
            return
        }
        foreach($item in $mimes.Keys)
        {
            Write-Host "添加MIME类型：$item"
            $extension = get-webconfigurationproperty //staticcontent -name collection | ?{$_.fileExtension -eq $item}
	         if($extension -ne $null)
	         {
		        write-host "该扩展名已经存在"
	         }
	         else
	         {
		        add-webconfigurationproperty //staticcontent -name collection -value @{fileExtension=$item;mimeType=$mimes[$item]}
	         }
        }
        Write-Host "MIME类型添加完成"
    }
    catch 
    {
        Write-Error $_.Exception.Message
    } 
}


#写日志信息
function logInfo
{
    param($msg)

    if(-not(Test-Path $savepath -PathType Container))
    {
        New-Item $savepath -ItemType directory
    }
    $datetime = Get-Date
    $content = $datetime.ToString() +":" + $msg
    Write-Host $content
    $content | Out-File -filepath $savepath'\info.log' -Append -Encoding utf8
}
#写日志错误信息
function logError
{
    param($msg)
    $datetime = Get-Date
    if(-not(Test-Path $savepath -PathType Container))
    {
        New-Item $savepath -ItemType directory
    }
    $datetime.ToString() +":" + $msg | Out-File -filepath $savepath'\error.log' -Append -Encoding utf8
}


#有参数调用
#LogDeployment $args[0] $args[1]
#无参数调用
LogDeployment

#visual studio 2013 项目中生成事件调用
#powershell.exe  –ExecutionPolicy Unrestricted "& { $(ProjectDir)LogDeploy.ps1 'C:\Users\Administrator\Desktop\log.txt' 'TESTWEB1' }"


