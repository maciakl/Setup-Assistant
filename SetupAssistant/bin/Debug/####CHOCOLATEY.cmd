@del /s /q tools
@mkdir tools
@copy Setup-Assistant.exe tools
@copy Setup-Assistant.exe.gui tools
cpack
@pause