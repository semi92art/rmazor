%~d0
cd %~dp0
@echo on
dotnet tool update --global dotnet-ef
SETLOCAL
set /p CACHE=<"%~dp0\ClickersAPI\obj\Container\ContainerId.cache"
docker exec -i %CACHE% /bin/sh -c "if PID=$(pidof dotnet); then kill $PID; fi"
devenv.com ClickersAPI.sln /build "Release|Any CPU"
docker exec -i %CACHE% /bin/sh -c "if PID=$(pidof dotnet); then kill $PID; fi"
cd "%~dp0\ClickersAPI"
dotnet ef migrations script --idempotent --output "%~dp0\ClickersAPI\obj\Release\netcoreapp3.1\PubTmp\EFSQLScripts\ClickersAPI.ApplicationDbContext.sql" --context ClickersAPI.ApplicationDbContext 
cd %~dp0
docker build -f "%~dp0\ClickersAPI\Dockerfile" --force-rm -t clickersapi %~dp0
docker tag clickersapi drago1/clickersapi:latest
docker push drago1/clickersapi:latest
