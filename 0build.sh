if [ ! -f /usr/bin/xbuild ] 
then
  echo XBuild missed, aborting
  exit 1
fi

xbuild qdvm.csproj
xbuild qdasm.csproj

