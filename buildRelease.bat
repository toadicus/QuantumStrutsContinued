rem


set H=R:\KSP_1.1.4_dev
echo %H%

copy /Y "bin\Release\QuantumStrutsContinued.dll" "GameData\QuantumStrutsContinued\Plugins"
copy /Y QuantumStrutsContinued.version GameData\QuantumStrutsContinued

cd GameData
mkdir "%H%\GameData\QuantumStrutsContinued"
xcopy /y /s QuantumStrutsContinued "%H%\GameData\QuantumStrutsContinued"

copy /Y ..\MiniAVC.dll GameData\QuantumStrutsContinued

set DEFHOMEDRIVE=d:
set DEFHOMEDIR=%DEFHOMEDRIVE%%HOMEPATH%
set HOMEDIR=
set HOMEDRIVE=%CD:~0,2%

set RELEASEDIR=d:\Users\jbb\release
set ZIP="c:\Program Files\7-zip\7z.exe"
echo Default homedir: %DEFHOMEDIR%

rem set /p HOMEDIR= "Enter Home directory, or <CR> for default: "

if "%HOMEDIR%" == "" (
set HOMEDIR=%DEFHOMEDIR%
) 
echo %HOMEDIR%

SET _test=%HOMEDIR:~1,1%
if "%_test%" == ":" (
set HOMEDRIVE=%HOMEDIR:~0,2%
)
cd ..

type QuantumStrutsContinued.version
set /p VERSION= "Enter version: "

set FILE="%RELEASEDIR%\QuantumStrutsContinued-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% GameData
