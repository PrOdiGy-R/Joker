#!/bin/bash
set -e

dotnet publish -c Release -o out

cd out
dotnet Joker.dll
