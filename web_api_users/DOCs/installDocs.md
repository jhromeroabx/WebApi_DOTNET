-- 1
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

-- 2
apt-get update; \
apt-get install -y apt-transport-https && \
apt-get update && \
apt-get install -y dotnet-sdk-6.0

-- 3
apt-get update; \
apt-get install -y apt-transport-https && \
apt-get update && \
apt-get install -y aspnetcore-runtime-6.0

-- 4
apt-get install -y dotnet-runtime-6.0