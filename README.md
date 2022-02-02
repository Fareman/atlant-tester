# atlant-bot

Проект, автоматизирующий проверку тестовых по бэкенду. https://confluence.tomskasu.ru/pages/viewpage.action?pageId=33194204

Пример команд для для оценки стиля кода или автоматического форматирования:

Проверка кода с генерацией отчета
jb inspectcode ./Api.sln --output=REPORT.xml

Форматирование кода в соответствии с соглашением форматирования кода
jb cleanupcode ./Api.sln --settings=./Api/codestyle.DotSettings

При сложностях см. https://www.jetbrains.com/help/resharper/ReSharper_Command_Line_Tools.html

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
Для устаноки необходимо перейти в корневую папку проекта и ввести следующик команды:
cd <Your project root directory>
dotnet new tool-manifest
dotnet tool install Husky

Настройка husky:
dotnet husky install
dotnet husky attach <file.csproj>

В появившемся файле root\.husky\task-runner.json добавляем команду в формате json:
#{
#   "name": "resharper",
#   "pre-push": "pre-push",
#   "command": "jb",
#   "args": [ "cleanupcode", "./Api.sln", "--settings=./Api/codestyle.DotSettings" ]
#}

В командной строке проекта пишем:
dotnet husky add pre-push -c "resharper"
Радуемся!

Husky.NET см. https://alirezanet.github.io/Husky.Net/guide/task-runner.html#why-task-runner

