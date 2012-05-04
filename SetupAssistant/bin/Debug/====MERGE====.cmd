@del Setup-Assistant.exe
@ilmerge /out:"Setup-Assistant.exe" "SetupAssistant.exe" "Ionic.Zip.dll" /log
@del SetupAssistant.zip
@zip -9 SetupAssistant.zip Setup-Assistant.exe
@pause