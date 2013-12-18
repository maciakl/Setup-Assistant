@del Setup-Assistant.exe
@"C:\Program Files (x86)\Microsoft\ILMerge\ilmerge" /targetplatform:v4,"C:\Windows\Microsoft.NET\Framework\v4.0.30319" /out:"Setup-Assistant.exe" "SetupAssistant.exe" "Ionic.Zip.dll" /log
@pause