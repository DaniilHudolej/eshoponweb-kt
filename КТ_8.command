#!/bin/zsh
cd "$(dirname "$0")"
export DOTNET_EXE="$(pwd)/../../work/dotnet/dotnet"
/usr/bin/python3 tools/kt8_watch.py --watch
