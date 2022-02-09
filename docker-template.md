<h1>Инструкция по настройке docker в проекте</h2>
<h2>Шаг 1 Создание dockerfile</h2>
Среда visual studio имеет встроенные инструменты для автоматического создания dockerfile'ов и композиции. Если вы начинающий пользователь Docker вот пара подсказок:

[Краткое руководство. Использование Docker в Visual Studio](https://docs.microsoft.com/ru-ru/visualstudio/containers/container-tools?view=vs-2022#prerequisites)

[Использование средств Visual Studio для работы с контейнерами для Docker](https://docs.microsoft.com/ru-ru/visualstudio/containers/overview?view=vs-2022#prerequisites)

В случае если вы фанат Jetbrains Rider:

[Generate Dockerfile for .NET Applications with Rider](https://blog.jetbrains.com/dotnet/2021/03/15/generate-dockerfile-for-net-applications-with-rider/)

Если возникли проблемы и не получается найти решение в интернете. В корневой папке проекта необходимо создать файл "dockerfile" по следующему шаблону:

**Пример dockerfile проекта WebApplication-Docker**
~~~dockerfile
#Указываем на чем основан проект(ASP.NET6.0)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
#Указываем расположение файлов внутри docker image
WORKDIR /app
#Инструкция EXPOSE задает сетевые порты, которые прослушиваются во время выполнения
EXPOSE 80
EXPOSE 443

#Теперь указываем инструкции по созданию сборки проекта
#Копируем файл(-ы) сборки в контейнер (Зависит от количества файлов в вашем проекте)
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebApplication-Docker/WebApplication-Docker.csproj", "WebApplication-Docker/"]
#Запускаем команду dotnet restore для восстановления зависимостей проекта и переноса в контейнер
RUN dotnet restore "WebApplication-Docker/WebApplication-Docker.csproj"
COPY . .
WORKDIR "/src/WebApplication-Docker"
#Запускаем команду dotnet build для создания сборки внутри контейнера
RUN dotnet build "WebApplication-Docker.csproj" -c Release -o /app/build

#Компилируем приложение на основе созданной сборки
FROM build AS publish
#Запускаем команду dotnet publish для компиляции проекта внутри контейнера
RUN dotnet publish "WebApplication-Docker.csproj" -c Release -o /app/publish

#Указываем точку входа в контейнер
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication-Docker.dll"]
~~~
<h2>Шаг 2 Создание docker-compose</h2>

Теперь необходимо создать файл "docker-compose.yml". Его необходимо поместить в отдельную папку в корне, если не получилось сделать автоматически.
~~~yml
#Указываем версию
version: '3.4'

#Перечисление сервисов внутри композиции
services:
  #Название сервиса
  testtap:
    #Image, на основе которого создается контейнер
    image: ${DOCKER_REGISTRY-}testtap
    build:
      #Указание маршрута к докерфайлу внутри вашего проекта
      context: .
      dockerfile: TestTAP/Dockerfile
~~~