<h1>Инструкция по внедрению docker в проект</h1>

Требования к проекту:

1. Проект тестового задания создается на основе выданного шаблона. Шаблон  необходимо поместить в директорию **%USERPROFILE%\Documents\Visual Studio <Версия вашей студии>\Templates\ProjectTemplates**

2. В папке, в которую вы поместили шаблон, откройте PowerShell и введите команду. Убедитесь, что в списке присутствует шаблон с названием "TAP back-end test task template"
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

4. В проект добавить пакеты "Npgsql.EntityFrameworkCore.PostgreSQL" и "Npgsql.EntityFrameworkCore.PostgreSQL.Design"

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