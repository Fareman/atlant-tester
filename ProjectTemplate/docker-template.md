<h1>Инструкция по использованию шаблона</h1>

Шаги по созданию проекта:

1. Откройте PowerShell и перейдите в корневую папку шаблона, где находится папка .template.config

2. Введите следующие команды и убедитесь, что в списке присутствует шаблон с названием "TAP back-end test task template"
~~~
dotnet new -i ./ 
dotnet new --list
~~~

3. Введите следующее в PowerShell заранее выбрав директорию, в которой хотите создать проект
~~~
mkdir TestTAP
cd TestTAP
dotnet new testtap
~~~

4. В созданный проект добавить пакеты "Npgsql.EntityFrameworkCore.PostgreSQL" и "Npgsql.EntityFrameworkCore.PostgreSQL.Design"

5. Внедрение пакетов и изменение строки подключения в appsettings.json
~~~C#
к//AppContext - ваш класс контекста бд
builder.Services.AddDbContext<AppContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("PostgreDatabase"));
});
~~~
~~~json
"ConnectionStrings": {
    "PostgreDatabase": "Server=postgres;Database=personApp;User Id=postgres;Password=changeme"
  }
~~~