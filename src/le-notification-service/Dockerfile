#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG NUGET_GITHUB_PASSWORD
WORKDIR /src
COPY ["LE.NotificationService/NuGet.config", "LE.NotificationService/"]
RUN sed -i "s|</configuration>|<packageSourceCredentials><github><add key=\\\"Username\\\" value=\\\"thuongnguyen1508\\\" /><add key=\\\"ClearTextPassword\\\" value=\\\"${NUGET_GITHUB_PASSWORD}\\\" /></github></packageSourceCredentials></configuration>|" "LE.NotificationService/NuGet.config"
COPY ["LE.NotificationService/*.csproj", "LE.NotificationService/"]
COPY ["LE.NotificationService.Infrastructure/*.csproj", "LE.NotificationService.Infrastructure/"]
RUN dotnet restore "LE.NotificationService/LE.NotificationService.csproj" --configfile ./LE.NotificationService/NuGet.config
COPY ./ ./
WORKDIR "/src/LE.NotificationService"
RUN dotnet build "LE.NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LE.NotificationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ARG ASPNETCORE_ENVIRONMENT              
ARG SECRET_KEY
ARG DB_CONNECTION_STRING
ARG CRON_JOB_DB_CONNECTION_STRING
ARG CONSUL_URL
ARG MESSAGE_BUS_HOST
ARG HOST_PREFIX

ENV ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT
ENV SECRET_KEY=$SECRET_KEY
ENV DB_CONNECTION_STRING=$DB_CONNECTION_STRING
ENV CRON_JOB_DB_CONNECTION_STRING=$CRON_JOB_DB_CONNECTION_STRING
ENV CONSUL_URL=$CONSUL_URL
ENV MESSAGE_BUS_HOST=$MESSAGE_BUS_HOST
ENV HOST_PREFIX=$HOST_PREFIX

ENTRYPOINT ["dotnet", "LE.NotificationService.dll"]

# docker build 
#             --build-arg ASPNETCORE_ENVIRONMENT=Develop
#             --build-arg MINIO_HOST
#             --build-arg CONSUL_URL
#             --build-arg S3_BUCKET_NAME
#             --build-arg AWS_ACCESS_KEY_ID
#             --build-arg AWS_SECRET_ACCESS_KEY
#             -t uploader .