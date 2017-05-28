rem
set H=R:\KSP_1.3.0_dev
echo %H%

copy /Y "bin\Release\QuantumStrutsContinued.dll" "GameData\QuantumStrutsContinued\Plugins"
copy /Y QuantumStrutsContinued.version GameData\QuantumStrutsContinued

cd GameData
mkdir "%H%\GameData\QuantumStrutsContinued"
xcopy /y /s QuantumStrutsContinued "%H%\GameData\QuantumStrutsContinued"

copy /Y ..\MiniAVC.dll GameData\QuantumStrutsContinued

set RELEASEDIR=d:\Users\jbb\release
set ZIP="c:\Program Files\7-zip\7z.exe"

cd ..


copy QuantumStrutsContinued.version a.version
set VERSIONFILE=a.version
rem The following requires the JQ program, available here: https://stedolan.github.io/jq/download/
c:\local\jq-win64  ".VERSION.MAJOR" %VERSIONFILE% >tmpfile
set /P major=<tmpfile

c:\local\jq-win64  ".VERSION.MINOR"  %VERSIONFILE% >tmpfile
set /P minor=<tmpfile

c:\local\jq-win64  ".VERSION.PATCH"  %VERSIONFILE% >tmpfile
set /P patch=<tmpfile

c:\local\jq-win64  ".VERSION.BUILD"  %VERSIONFILE% >tmpfile
set /P build=<tmpfile
del tmpfile
set VERSION=%major%.%minor%.%patch%
if "%build%" NEQ "0"  set VERSION=%VERSION%.%build%



set FILE="%RELEASEDIR%\QuantumStrutsContinued-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% GameData
