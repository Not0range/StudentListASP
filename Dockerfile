FROM mcr.microsoft.com/dotnet/sdk:3.1

WORKDIR /usr/src/app
COPY . .
RUN [ "dotnet", "add", "package", "System.IdentityModel.Tokens.Jwt" ]
RUN [ "dotnet", "build" ]

EXPOSE 3000
ENTRYPOINT [ "dotnet", "run" ]