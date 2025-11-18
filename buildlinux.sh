#Thanks to Cautami (https://github.com/Cautami) for this script, taken from https://github.com/Cautami/Desktop_Gremlin_Avalonia
dotnet publish Desktop_Gremlin/Desktop_Gremlin.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true -o Desktop_Gremlin/bin/Published/Linux
cp -r Desktop_Gremlin/Sprites Desktop_Gremlin/bin/Published/Linux/
cp Desktop_Gremlin/ico.ico Desktop_Gremlin/bin/Published/Linux/
cp Desktop_Gremlin/config.txt Desktop_Gremlin/bin/Published/Linux/
