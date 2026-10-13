#!/bin/zsh
cd "$(dirname "$0")"
export DOTNET_EXE="$(pwd)/../../work/dotnet/dotnet"
/usr/bin/python3 tools/run_control_points.py --kt 4
printf "\nНажмите Enter для закрытия\n"
read
