#Thanks to Cautami (https://github.com/Cautami) for this script, taken from https://github.com/Cautami/Desktop_Gremlin_Avalonia
dotnet publish Desktop_Gremlin/Desktop_Gremlin.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true -o Desktop_Gremlin/bin/Published/Linux

#Use this to release the Windows version on a Linux machine
#dotnet publish Desktop_Gremlin/Desktop_Gremlin.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o Desktop_Gremlin/bin/Published/Windows

#Use this to release the Mac version on a Linux machine
#dotnet publish Desktop_Gremlin/Desktop_Gremlin.csproj -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true -o Desktop_Gremlin/bin/Published/Mac
