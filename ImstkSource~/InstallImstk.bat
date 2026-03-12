:: Pass the imstk install path to this batch file to install a new version of 
:: imstk into the appropriate folder
:: e.g InstallImstk.bat C:\Imstk\build\install

echo off

if exist %1 (
  del /Q %~p0..\Plugins\*.dll
  del /Q %~p0..\Plugins\*.meta
  copy %1\bin\*.dll %~p0..\Plugins
) else (echo "Could not find imstk install directory ")