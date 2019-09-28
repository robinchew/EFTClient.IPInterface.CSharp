Get Started on Linux
====================

```
cd DotNetCore.CLIDemo
```
```
dotnet run '{"host": "127.0.0.1", "port": 2011, "amount": 1.0, "ssl": false}'
```
OR
```
dotnet publish -c Release -r linux-x64 /p:PublishSingleFile=true /p:PublishTrimmed=true
./bin/Release/netcoreapp3.0/linux-x64/publish/CLIDemo '{"host": "127.0.0.1", "port": 2011, "amount": 1.0, "ssl": false}'
```
- https://dotnetcoretutorials.com/2019/06/20/publishing-a-single-exe-file-in-net-core-3-0/
- https://radu-matei.com/blog/self-contained-dotnet-cli/
