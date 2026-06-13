// ============================================================================
// MauiProgram.cs — Конфигурация приложения и Dependency Injection (DI)
// ============================================================================
// Это главный конфигурационный файл приложения .NET MAUI.
// Здесь настраивается DI-контейнер (Dependency Injection), который
// автоматически создаёт и внедряет зависимости во все классы.
//
// Паттерн DI позволяет:
//   - Не создавать объекты вручную через new — контейнер делает это сам
//   - Легко заменять реализации (например, для тестирования)
//   - Управлять временем жизни объектов (Singleton vs Transient)
//
// Singleton — один экземпляр на всё приложение (сервисы с кэшем данных)
// Transient — новый экземпляр при каждом запросе (страницы и ViewModel'ы)
// ============================================================================

using Microsoft.Extensions.Logging;
using TypingTrainer.Services;
using TypingTrainer.ViewModels;
using TypingTrainer.Views;

namespace TypingTrainer;

public static class MauiProgram
{
    /// <summary>
    /// Создаёт и настраивает экземпляр MAUI-приложения.
    /// Вызывается один раз при запуске из MainApplication (Android).
    /// </summary>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()  // Указываем класс App как корневой
            .ConfigureFonts(fonts =>
            {
                // Регистрация шрифтов, доступных во всём приложении
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ---- Регистрация сервисов (бизнес-логика) ----
        // Singleton — один экземпляр на всё время работы приложения,
        // чтобы кэшировать данные и не перечитывать JSON-файлы при каждом переходе
        builder.Services.AddSingleton<DictionaryService>();   // Управление словарями
        builder.Services.AddSingleton<StatisticsService>();   // Управление статистикой
        builder.Services.AddSingleton<SettingsService>();     // Управление настройками

        // ---- Регистрация ViewModel'ов (логика представления) ----
        // Transient — новый экземпляр при каждом переходе на страницу,
        // чтобы данные обновлялись при каждом открытии
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<GameViewModel>();
        builder.Services.AddTransient<DictionaryListViewModel>();
        builder.Services.AddTransient<DictionaryEditViewModel>();
        builder.Services.AddTransient<StatisticsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // ---- Регистрация страниц (UI) ----
        // Transient — каждая страница создаётся заново при навигации
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<GamePage>();
        builder.Services.AddTransient<DictionaryListPage>();
        builder.Services.AddTransient<DictionaryEditPage>();
        builder.Services.AddTransient<StatisticsPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        // В режиме отладки включаем логирование в Debug Output
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
