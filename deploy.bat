﻿rem


set H=R:\KSP_1.1.4_dev
echo %H%

copy /Y "bin\Debug\QuantumStrutsContinued.dll" "GameData\QuantumStrutsContinued\Plugins"
copy /Y QuantumStrutsContinued.version GameData\QuantumStrutsContinued

cd GameData
mkdir "%H%\GameData\QuantumStrutsContinued"
xcopy /y /s QuantumStrutsContinued "%H%\GameData\QuantumStrutsContinued"

