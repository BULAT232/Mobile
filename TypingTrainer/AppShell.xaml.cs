// ============================================================================
// AppShell.xaml.cs — Навигационная оболочка приложения (code-behind)
// ============================================================================
// AppShell определяет структуру навигации всего приложения.
// В XAML-части (AppShell.xaml) описано Flyout-меню с 4 разделами:
//   - Главная (MainPage)
//   - Словари (DictionaryListPage)
//   - Статистика (StatisticsPage)
//   - Настройки (SettingsPage)
//
// В code-behind регистрируются дополнительные маршруты для страниц,
// которые НЕ являются частью Flyout-меню, а открываются программно
// через Shell.Current.GoToAsync() — это GamePage и DictionaryEditPage.
// Без регистрации маршрута навигация на эти страницы невозможна.
// ============================================================================

using TypingTrainer.Views;

namespace TypingTrainer;

public partial class AppShell : Shell
{
    public AppShell()
    {
        // Загружаем XAML-разметку с описанием Flyout-меню
        InitializeComponent();

        // Регистрируем маршруты для страниц, доступных только через программную навигацию:
        // GamePage — страница игры (открывается с главной по кнопке "Начать тренировку")
        Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
        // DictionaryEditPage — страница редактирования/создания словаря
        Routing.RegisterRoute(nameof(DictionaryEditPage), typeof(DictionaryEditPage));
    }
}
