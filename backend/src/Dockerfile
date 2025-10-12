
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env


WORKDIR /app

COPY ./OnlineEducation/OnlineEducation.csproj ./OnlineEducation/
RUN dotnet restore ./OnlineEducation/OnlineEducation.csproj


COPY ./OnlineEducation/ ./OnlineEducation/


RUN dotnet publish ./OnlineEducation/OnlineEducation.csproj -c Release -o /app/publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:8.0


WORKDIR /app

COPY --from=build-env /app/publish .


EXPOSE 8080


ENTRYPOINT ["dotnet", "OnlineEducation.dll"]

