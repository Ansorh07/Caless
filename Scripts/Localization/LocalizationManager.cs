using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Система локализации для Caless.
/// Поддерживает русский (RU) и английский (EN) языки.
/// </summary>
public static class LocalizationManager
{
    public enum Language { RU, EN }
    private static Language currentLanguage = Language.RU;

    private static readonly Dictionary<string, Dictionary<Language, string>> translations = new Dictionary<string, Dictionary<Language, string>>()
    {
        // Settings UI
        { "SETTINGS", new Dictionary<Language, string> { { Language.RU, "НАСТРОЙКИ" }, { Language.EN, "SETTINGS" } } },
        { "APPEARANCE", new Dictionary<Language, string> { { Language.RU, "ВНЕШНИЙ ВИД" }, { Language.EN, "APPEARANCE" } } },
        { "SOUND", new Dictionary<Language, string> { { Language.RU, "ЗВУК" }, { Language.EN, "SOUND" } } },
        { "GAME", new Dictionary<Language, string> { { Language.RU, "ИГРА" }, { Language.EN, "GAME" } } },
        { "OTHER", new Dictionary<Language, string> { { Language.RU, "ДРУГОЕ" }, { Language.EN, "OTHER" } } },

        // Appearance settings
        { "BOARD_THEME", new Dictionary<Language, string> { { Language.RU, "🎨  Тема интерфейса" }, { Language.EN, "🎨  Board Theme" } } },
        { "BOARD_STYLE", new Dictionary<Language, string> { { Language.RU, "⬜  Стиль доски" }, { Language.EN, "⬜  Board Style" } } },
        { "HIGHLIGHT_COLOR", new Dictionary<Language, string> { { Language.RU, "☀  Цвет подсветки" }, { Language.EN, "☀  Highlight Color" } } },

        // Sound settings
        { "MUSIC", new Dictionary<Language, string> { { Language.RU, "🎵  Музыка" }, { Language.EN, "🎵  Music" } } },
        { "SOUND_EFFECTS", new Dictionary<Language, string> { { Language.RU, "🔊  Звуковые эффекты" }, { Language.EN, "🔊  Sound Effects" } } },
        { "MOVE_SOUND", new Dictionary<Language, string> { { Language.RU, "♞  Звук перемещения" }, { Language.EN, "♞  Move Sound" } } },
        { "CAPTURE_SOUND", new Dictionary<Language, string> { { Language.RU, "⚔  Звук захвата" }, { Language.EN, "⚔  Capture Sound" } } },

        // Game settings
        { "HINTS", new Dictionary<Language, string> { { Language.RU, "💡  Подсказки" }, { Language.EN, "💡  Hints" } } },
        { "SHOW_MOVES", new Dictionary<Language, string> { { Language.RU, "⚙  Показывать возможные ходы" }, { Language.EN, "⚙  Show Possible Moves" } } },
        { "AUTOSAVE", new Dictionary<Language, string> { { Language.RU, "🛡  Автосохранение" }, { Language.EN, "🛡  Autosave" } } },
        { "MOVE_CONFIRMATION", new Dictionary<Language, string> { { Language.RU, "✔  Подтверждение хода" }, { Language.EN, "✔  Move Confirmation" } } },
        { "DEFAULT_DIFFICULTY", new Dictionary<Language, string> { { Language.RU, "📊  Сложность по умолчанию" }, { Language.EN, "📊  Default Difficulty" } } },
        { "DEFAULT_TIME", new Dictionary<Language, string> { { Language.RU, "⏰  Время на ход по умолчанию" }, { Language.EN, "⏰  Default Move Time" } } },

        // Other settings
        { "LANGUAGE", new Dictionary<Language, string> { { Language.RU, "🌐  Язык" }, { Language.EN, "🌐  Language" } } },
        { "STATISTICS", new Dictionary<Language, string> { { Language.RU, "📊  Статистика" }, { Language.EN, "📊  Statistics" } } },
        { "RESET_SETTINGS", new Dictionary<Language, string> { { Language.RU, "↻  Сбросить настройки" }, { Language.EN, "↻  Reset Settings" } } },
        { "SUPPORT", new Dictionary<Language, string> { { Language.RU, "🎧  Поддержка" }, { Language.EN, "🎧  Support" } } },
        { "ABOUT", new Dictionary<Language, string> { { Language.RU, "ⓘ  О приложении" }, { Language.EN, "ⓘ  About" } } },

        // Difficulty
        { "EASY", new Dictionary<Language, string> { { Language.RU, "Лёгкая" }, { Language.EN, "Easy" } } },
        { "MEDIUM", new Dictionary<Language, string> { { Language.RU, "Средняя" }, { Language.EN, "Medium" } } },

        // Time
        { "5MIN", new Dictionary<Language, string> { { Language.RU, "5 мин" }, { Language.EN, "5 min" } } },
        { "10MIN", new Dictionary<Language, string> { { Language.RU, "10 мин" }, { Language.EN, "10 min" } } },
        { "15MIN", new Dictionary<Language, string> { { Language.RU, "15 мин" }, { Language.EN, "15 min" } } },
        { "30MIN", new Dictionary<Language, string> { { Language.RU, "30 мин" }, { Language.EN, "30 min" } } },

        // Statistics
        { "STATS_TITLE", new Dictionary<Language, string> { { Language.RU, "Статистика" }, { Language.EN, "Statistics" } } },
        { "STATS_VS_AI", new Dictionary<Language, string> { { Language.RU, "Против ИИ" }, { Language.EN, "vs AI" } } },
        { "STATS_WINS", new Dictionary<Language, string> { { Language.RU, "Побед" }, { Language.EN, "Wins" } } },
        { "STATS_LOSSES", new Dictionary<Language, string> { { Language.RU, "Поражений" }, { Language.EN, "Losses" } } },
        { "STATS_DRAWS", new Dictionary<Language, string> { { Language.RU, "Ничьих" }, { Language.EN, "Draws" } } },
        { "STATS_TOTAL_GAMES", new Dictionary<Language, string> { { Language.RU, "Всего игр" }, { Language.EN, "Total Games" } } },
        { "STATS_TOTAL_MOVES", new Dictionary<Language, string> { { Language.RU, "Всего ходов" }, { Language.EN, "Total Moves" } } },
        { "STATS_CAPTURES", new Dictionary<Language, string> { { Language.RU, "Захватов" }, { Language.EN, "Captures" } } },
        { "STATS_WIN_RATE", new Dictionary<Language, string> { { Language.RU, "Процент побед" }, { Language.EN, "Win Rate" } } },

        // Support
        { "SUPPORT_TITLE", new Dictionary<Language, string> { { Language.RU, "Поддержка" }, { Language.EN, "Support" } } },
        { "SUPPORT_EMAIL", new Dictionary<Language, string> { { Language.RU, "Email: support@caless.game" }, { Language.EN, "Email: support@caless.game" } } },
        { "SUPPORT_WEBSITE", new Dictionary<Language, string> { { Language.RU, "Сайт: www.caless.game" }, { Language.EN, "Website: www.caless.game" } } },
        { "SUPPORT_DISCORD", new Dictionary<Language, string> { { Language.RU, "Discord: discord.gg/caless" }, { Language.EN, "Discord: discord.gg/caless" } } },
        { "SUPPORT_MESSAGE", new Dictionary<Language, string> { { Language.RU, "Свяжитесь с нами если у вас есть вопросы или предложения" }, { Language.EN, "Contact us if you have any questions or suggestions" } } },

        // Dialogs
        { "CLOSE", new Dictionary<Language, string> { { Language.RU, "Закрыть" }, { Language.EN, "Close" } } },
        { "RESET_CONFIRM", new Dictionary<Language, string> { { Language.RU, "Вы уверены? Все настройки будут сброшены." }, { Language.EN, "Are you sure? All settings will be reset." } } },
        { "ABOUT_TEXT", new Dictionary<Language, string> { 
            { Language.RU, "Caless v1.0\n\nСтратегическая настольная игра\nс 12 уникальными фигурами\nна доске 10×10.\n\n© 2025" }, 
            { Language.EN, "Caless v1.0\n\nStrategic board game\nwith 12 unique pieces\non a 10×10 board.\n\n© 2025" } 
        } },
    };

    public static void SetLanguage(Language lang)
    {
        currentLanguage = lang;
    }

    public static void SetLanguageByIndex(int index)
    {
        if (index == 0) currentLanguage = Language.RU;
        else if (index == 1) currentLanguage = Language.EN;
    }

    public static Language GetCurrentLanguage()
    {
        return currentLanguage;
    }

    public static string Get(string key)
    {
        if (translations.ContainsKey(key))
        {
            var langDict = translations[key];
            if (langDict.ContainsKey(currentLanguage))
                return langDict[currentLanguage];
        }
        return key; // Fallback to key if translation not found
    }

    public static string Get(string key, Language lang)
    {
        if (translations.ContainsKey(key))
        {
            var langDict = translations[key];
            if (langDict.ContainsKey(lang))
                return langDict[lang];
        }
        return key;
    }
}
