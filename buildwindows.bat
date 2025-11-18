dotnet publish Desktop_Gremlin/Desktop_Gremlin.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o Desktop_Gremlin/bin/Published/Windows
robocopy Desktop_Gremlin\SpriteSheet Desktop_Gremlin\bin\Published\Windows\ /E
copy Desktop_Gremlin\ico.ico Desktop_Gremlin\bin\Published\Windows\ /Y
copy Desktop_Gremlin\config.txt Desktop_Gremlin\bin\Published\Windows\ /Y
