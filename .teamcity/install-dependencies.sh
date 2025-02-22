#!/bin/bash
set -euo pipefail

export DEBIAN_FRONTEND=noninteractive

# Install zip
if which zip &>/dev/null; then
  echo "zip is already installed";
else
  apt-get update
  apt-get -y install zip
fi

# Install ASP.NET Core runtime
dotnet_runtime_package="aspnetcore-runtime-9.0"
if dpkg -s $dotnet_runtime_package &>/dev/null; then
  echo "$dotnet_runtime_package is already installed."
else
  apt-get update
  apt-get install -y software-properties-common
  add-apt-repository -y ppa:dotnet/backports
  apt-get install -y $dotnet_runtime_package
fi