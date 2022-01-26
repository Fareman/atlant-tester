namespace Tester
{
    public interface ITesterService
    {
        /// <summary>
        /// Проверяет тестовое задание по указанной ссылке.
        /// </summary>
        /// <param name="gitZipUri">Ссылка на скачивание zip архива с GitHub.</param>
        /// <returns>Отчет.</returns>
        Task TestAsync(string gitZipUri);
    }
}
