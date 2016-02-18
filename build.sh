clear

echo "Starting Dependency Install using Paket."
mono .paket/paket.exe restore

echo "Starting Build using Mono."
xbuild ./src/Togglez/Togglez.sln
