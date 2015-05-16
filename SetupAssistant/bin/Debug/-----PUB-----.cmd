@set LOC="C:\Users\luke\projects\SetupAssistant\Setup-Assistant"

xcopy Setup-Assistant.exe "%LOC%" /Y
xcopy version.txt "%LOC%" /Y
xcopy pad_file.xml "%LOC%" /Y

7za a -tzip "%LOC%\SetupAssistant.zip" "%LOC%\Setup-Assistant.exe"