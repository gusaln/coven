.PHONY: all single-linux-x64 single-win-x64

all:
	dotnet publish -c Release -r linux-x64 --no-self-contained -o build/notsc src/Coven

single-linux-x64:
	dotnet publish -v d -c Release -r linux-x64 --self-contained -p:PublishTrimmed=true -p:PublishSingleFile=true -o build/single-linux-x64 src/Coven

single-win-x64:
	dotnet publish -v d -c Release -r win-x64 --self-contained -p:PublishTrimmed=true -p:PublishSingleFile=true -o build/single-win-x64 src/Coven

clean:
	dotnet clean src
	rm build/**/*
	rmdir build/notsc
	rmdir build/single-linux-x64
	rmdir build/single-win-x64