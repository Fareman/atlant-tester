<h1>atlant-bot</h1>

Проект, автоматизирующий проверку тестовых по бэкенду. https://confluence.tomskasu.ru/pages/viewpage.action?pageId=33194204

<h2>Пример команд для для оценки стиля кода или автоматического форматирования</h2>

Инспекция кода без изменений с генерацией отчета

>jb inspectcode ./Api.sln --output=REPORT.xml

Форматирование кода в соответствии с соглашением форматирования кода

>jb cleanupcode ./Api.sln --settings=./Api/codestyle.DotSettings

см. [Инструкцию](https://www.jetbrains.com/help/resharper/ReSharper_Command_Line_Tools.html)

<h2>Установка husky</h2>
Для устаноки необходимо перейти в корневую папку проекта и ввести следующик команды:

>cd <i>Your project root directory</i>

>dotnet new tool-manifest

>dotnet tool install Husky


<h2>Настройка husky</h2>

>dotnet husky install

>dotnet husky attach <file.csproj>

В появившемся файле root\.husky\task-runner.json добавляем команду в формате json:
```
{
   "name": "resharper",
   "pre-push": "pre-push",
   "command": "jb",
   "args": [ "cleanupcode", "./Api.sln", "--settings=./Api/codestyle.DotSettings" ]
}
```

В командной строке проекта пишем:

>dotnet husky add pre-push -c "resharper"

Источник: [Husky.NET](https://alirezanet.github.io/Husky.Net/guide/task-runner.html#why-task-runner)

<h2>Запуск проекта в docker с compose-файлом тестового задания</h2>
>cd <i>Test project root directory</i>

В docker-compose файле тестового задания указать путь к проекту в контексте:
~~~yml
version: '3.4'

services:
  testtap:
    image: ${DOCKER_REGISTRY-}testtap
    build:
      context: . (Вместо точки указать путь к папке с проектом)
      dockerfile: TestTAP/Dockerfile
~~~
По аналогии с примером запустить команду в терминале с путями к docker-compose файлам
>docker-compose -f C:\путь до первого файла\docker-compose.yml -f C:\путь до второго\docker-compose.yml up