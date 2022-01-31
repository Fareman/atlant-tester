# atlant-bot

Проект, автоматизирующий проверку тестовых по бэкенду. https://confluence.tomskasu.ru/pages/viewpage.action?pageId=33194204

Пример команд для для оценки стиля кода или автоматического форматирования:
Проверка кода с генерацией отчета
jb inspectcode ./Api.sln --output=REPORT.xml

Форматирование кода в соответствии с соглашением форматирования кода
jb cleanupcode ./Api.sln --settings=./Api/codestyle.DotSettings
