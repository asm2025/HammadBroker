@echo off
setlocal EnableDelayedExpansion

set scheme=http
set enable=1

:Parse
if /I "%1"=="ssl" (
	set scheme=https
) else if /I "%1"=="disable" (
	set enable=0
)

shift
if not "%~1"=="" goto Parse

if %enable%==0 goto Disable

set fwName="IISExpressWeb"
netsh advfirewall firewall show rule name=!fwName!>nul

if errorlevel 1 (
	echo Adding firewall rule !fwName!
	netsh advfirewall firewall add rule name=!fwName! dir=in protocol=tcp localport=5000-5010,44300-44310 profile=private,domain remoteip=localsubnet action=allow enable=yes
) else (
	netsh advfirewall firewall set rule name=!fwName! new dir=in protocol=tcp localport=5000-5010,44300-44310 profile=private,domain remoteip=localsubnet action=allow enable=yes
)

for /L %%i in (5000,1,5010) do (
	echo Mapping http://web.local:%%i/
	netsh http delete urlacl url=http://web.local:%%i/>nul
	netsh http add urlacl url=http://web.local:%%i/ user=everyone
)

for /L %%i in (44300,1,44310) do (
	echo Mapping %scheme%://web.local:%%i/
	netsh http delete urlacl url=https://web.local:%%i/>nul
	netsh http delete urlacl url=http://web.local:%%i/>nul
	netsh http add urlacl url=%scheme%://web.local:%%i/ user=everyone
)

goto :EOF

:Disable
for /L %%i in (5000,1,5010) do (
	echo Removing web.local:%%i/
	netsh http delete urlacl url=http://web.local:%%i/>nul
)
for /L %%i in (44300,1,44310) do (
	echo Removing web.local:%%i/
	netsh http delete urlacl url=https://web.local:%%i/>nul
	netsh http delete urlacl url=http://web.local:%%i/>nul
)
