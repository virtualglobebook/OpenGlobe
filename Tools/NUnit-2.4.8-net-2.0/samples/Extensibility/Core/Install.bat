@echo off
Rem This is called by the post-build event. You may modify it to copy the addin
Rem to whatever locations you like. As provided, it copies it to a number of
Rem directories relative to the target, provided they exist.

setlocal
set UP6= ..\..\..\..\..\..

Rem See whether we are doing a production or development install
if exist %UP6%\bin goto prod
if exist %UP6%\src goto dev
echo No install locations found - install %1 manually
goto end

:prod
Rem For production install, ensure we have an addins directory
if not exist %UP6%\bin\addins mkdir %UP6%\bin\addins
copy %1 %UP6%\bin\addins
echo Installed %1 in bin\addins directory
goto end

:dev
Rem For development installs, try each nunit-gui-exe output directory and
Rem use those that have an addins sub-directory
call :try %1 Debug
call :try %1 Debug2005
call :try %1 Release
call :try %1 Release2005

goto end

:try
if not exist %UP6%\src\GuiRunner\nunit-gui-exe\bin\%2 goto end
if exist %UP6%\src\GuiRunner\nunit-gui-exe\bin\%2\addins goto try2
md %UP6%\src\GuiRunner\nunit-gui-exe\bin\%2\addins
:try2
copy %1 %UP6%\src\GuiRunner\nunit-gui-exe\bin\%2\addins
echo Installed %1 in nunit-gui-exe\bin\%2\addins directory

:end