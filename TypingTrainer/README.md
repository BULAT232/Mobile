# ⌨️ Тренажёр набора текста (Typing Trainer)

Мобильное приложение для Android на .NET MAUI — тренажёр скорости и точности набора текста на клавиатуре, аналог [Клавогонки](https://klavogonki.ru/).

## 📱 Возможности

- **Тренировка набора текста** — набирайте текст из выбранного словаря с отслеживанием в реальном времени
- **Выбор словаря** — 4 встроенных словаря (русский базовый, русский продвинутый, английский базовый, программирование)
- **Редактирование словарей** — создавайте, редактируйте и удаляйте пользовательские словари
- **Статистика** — подробная статистика: WPM, CPM, точность, количество ошибок, история сессий
- **Настройки** — размер шрифта, количество слов, отображение таймера/WPM/точности

## 🎮 Как играть

1. На главной странице выберите словарь
2. Нажмите «Начать тренировку»
3. Начните печатать текст, отображённый на экране
4. Символы подсвечиваются:
   - 🟢 **Зелёный** — правильно набранный символ
   - 🔴 **Красный** — ошибка
   - 🟡 **Жёлтый фон** — текущая позиция
   - ⚪ **Серый** — ещё не набранный символ
5. По завершении увидите результаты: WPM, CPM, точность, время, ошибки

## 🏗️ Архитектура

Проект построен на паттерне **MVVM** с использованием:

- **.NET 8 MAUI** — кроссплатформенный UI фреймворк
- **CommunityToolkit.Mvvm** — MVVM инфраструктура (ObservableObject, RelayCommand)
- **Newtonsoft.Json** — сериализация данных
- **Shell Navigation** — навигация между страницами

### Структура проекта

```
TypingTrainer/
├── Models/                    # Модели данных
│   ├── AppSettings.cs         # Настройки приложения
│   ├── GameSession.cs         # Игровая сессия
│   ├── SessionResult.cs       # Результат сессии
│   └── TypingDictionary.cs    # Словарь слов
├── Services/                  # Сервисы (бизнес-логика)
│   ├── DictionaryService.cs   # Управление словарями
│   ├── SettingsService.cs     # Управление настройками
│   └── StatisticsService.cs   # Управление статистикой
├── ViewModels/                # ViewModel'ы (MVVM)
│   ├── MainViewModel.cs       # Главная страница
│   ├── GameViewModel.cs       # Игровой процесс
│   ├── DictionaryListViewModel.cs   # Список словарей
│   ├── DictionaryEditViewModel.cs   # Редактирование словаря
│   ├── StatisticsViewModel.cs       # Статистика
│   └── SettingsViewModel.cs         # Настройки
├── Views/                     # Страницы (UI)
│   ├── MainPage.xaml/.cs      # Главная
│   ├── GamePage.xaml/.cs      # Игра
│   ├── DictionaryListPage.xaml/.cs  # Список словарей
│   ├── DictionaryEditPage.xaml/.cs  # Редактирование словаря
│   ├── StatisticsPage.xaml/.cs      # Статистика
│   └── SettingsPage.xaml/.cs        # Настройки
├── Converters/                # Конвертеры значений
│   ├── InvertBoolConverter.cs
│   └── IsNotNullConverter.cs
├── Platforms/Android/         # Android-специфичный код
├── Resources/Styles/          # Стили и цвета
├── App.xaml/.cs               # Точка входа приложения
├── AppShell.xaml/.cs          # Навигация (Shell)
└── MauiProgram.cs             # Конфигурация DI и сервисов
```

## 🚀 Сборка и запуск

### Требования

- .NET 8 SDK
- OpenJDK 17
- Android SDK (API 21+, рекомендуется API 34)
- Visual Studio 2022, JetBrains Rider или VS Code с поддержкой MAUI

---

### 🍎 Установка на macOS (через Homebrew)

#### 1. Установка .NET 8 SDK

```bash
brew install dotnet@8
```

#### 2. Установка OpenJDK 17 (необходим для Android SDK)

```bash
brew install openjdk@17
```

#### 3. Установка Android Command Line Tools

```bash
brew install --cask android-commandlinetools
```

#### 4. Установка Android SDK компонентов

```bash
export JAVA_HOME="/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home"
export ANDROID_HOME="/opt/homebrew/share/android-commandlinetools"

yes | sdkmanager --sdk_root="$ANDROID_HOME" \
  "platform-tools" \
  "platforms;android-34" \
  "build-tools;34.0.0"
```

#### 5. Установка .NET MAUI workload

```bash
export PATH="/opt/homebrew/opt/dotnet@8/bin:$PATH"
export DOTNET_ROOT="/opt/homebrew/opt/dotnet@8/libexec"

dotnet workload install maui-android
```

#### 6. Настройка переменных окружения

Добавьте в `~/.zshrc`:

```bash
# .NET 8 SDK
export PATH="/opt/homebrew/opt/dotnet@8/bin:$PATH"
export DOTNET_ROOT="/opt/homebrew/opt/dotnet@8/libexec"

# OpenJDK 17
export PATH="/opt/homebrew/opt/openjdk@17/bin:$PATH"
export JAVA_HOME="/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home"

# Android SDK
export ANDROID_HOME="/opt/homebrew/share/android-commandlinetools"
export ANDROID_SDK_ROOT="/opt/homebrew/share/android-commandlinetools"
export PATH="$ANDROID_HOME/platform-tools:$ANDROID_HOME/emulator:$PATH"
```

Затем перезагрузите терминал: `source ~/.zshrc`

---

### 🔨 Команды сборки и запуска

```bash
# Восстановление пакетов
dotnet restore

# Сборка
dotnet build -f net8.0-android

# Запуск на Android эмуляторе или подключённом устройстве
dotnet build -t:Run -f net8.0-android
```

> **Примечание:** Если Android SDK не обнаруживается автоматически, укажите путь явно:
>
> ```bash
> dotnet build -f net8.0-android -p:AndroidSdkDirectory="$ANDROID_HOME"
> ```

### 📱 Создание Android эмулятора

```bash
# Установка образа системы
sdkmanager --sdk_root="$ANDROID_HOME" "system-images;android-34;google_apis;arm64-v8a"

# Создание эмулятора
avdmanager create avd -n "Pixel_API_34" -k "system-images;android-34;google_apis;arm64-v8a" -d "pixel_6"

# Запуск эмулятора
$ANDROID_HOME/emulator/emulator -avd Pixel_API_34
```

### 📲 Запуск на физическом устройстве

1. Включите **Режим разработчика** на Android-устройстве (Настройки → О телефоне → 7 раз нажмите на «Номер сборки»)
2. Включите **Отладку по USB** (Настройки → Для разработчиков → Отладка по USB)
3. Подключите устройство по USB
4. Проверьте подключение: `adb devices`
5. Запустите: `dotnet build -t:Run -f net8.0-android`

### 🖥️ Через Visual Studio / VS Code

1. Откройте `TypingTrainer.csproj`
2. Выберите Android эмулятор или подключённое устройство
3. Нажмите F5 для запуска

## 📊 Метрики

- **WPM** (Words Per Minute) — слов в минуту (средняя длина слова = 5 символов)
- **CPM** (Characters Per Minute) — символов в минуту
- **Accuracy** — процент правильно набранных символов
