<h1>Инструкция по внедрению docker в проект</h2>

Требования к проекту:
<ol>
  <li>Проект тестового задания создается на основе выданного шаблона. Его необходимо поместить в директорию %USERPROFILE%\Documents\Visual Studio <Версия вашей студии>\Templates\ProjectTemplates</li>
  <li>При создании проекта из шаблона задате имя TestTAP и не ставьте галочку на опции "Поместить проект и решение в одной директории"</li>
  <li>Убедитесь, что файл решения (.sln) лежит в корне проекта, файл сборки (.csproj) лежит ./TestTAP/TestTAP.csproj</li>
  <li>Убедитесь, что файлы .dockerignore, docker-compose.dcproj, docker-compose.override.yml, dockerfile и docker-compose.yml лежат в корневой папке вместе с файлом TestTAP.sln</li>
  <li>В проект добавить пакеты "Npgsql.EntityFrameworkCore.PostgreSQL" и "Npgsql.EntityFrameworkCore.PostgreSQL.Design"</li>
</ol>

6. Внедрение пакетов и изменение строки подключения в appsettings.json
~~~C#
builder.Services.AddDbContext<Ваш контекст>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("PostgreDatabase"));
});
~~~
~~~json
"ConnectionStrings": {
    "PostgreDatabase": "Server=postgres;Database=personApp;User Id=postgres;Password=changeme"
  }
~~~