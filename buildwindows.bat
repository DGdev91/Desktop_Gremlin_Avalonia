dotnet publish Desktop_Gremlin/Desktop_Gremlin.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o Desktop_Gremlin/bin/Published/Windows

rem Use this to release the Linux version on a Windows machine
rem dotnet publish Desktop_Gremlin/Desktop_Gremlin.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true -o Desktop_Gremlin/bin/Published/Linux

rem Use this to release the Windows version on a Windows machine
rem dotnet publish Desktop_Gremlin/Desktop_Gremlin.csproj -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true -o Desktop_Gremlin/bin/Published/Mac
