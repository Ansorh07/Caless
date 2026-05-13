using UnityEngine;

/// <summary>
/// Система статистики игрока.
/// Отслеживает побед, поражений, ничьих и других показателей.
/// Сохраняется через PlayerPrefs.
/// </summary>
public class StatisticsManager : MonoBehaviour
{
    // Статистика против ИИ
    public int VsAIWins { get; set; } = 0;
    public int VsAILosses { get; set; } = 0;
    public int VsAIDraws { get; set; } = 0;

    // Локальная статистика
    public int LocalMultiplayerGames { get; set; } = 0;

    // Статистика обучения
    public int TrainingPuzzlesSolved { get; set; } = 0;
    public int TrainingPuzzlesTotal { get; set; } = 0;

    // Bluetooth статистика
    public int BluetoothGames { get; set; } = 0;

    // Общая статистика
    public int TotalGamesPlayed { get; set; } = 0;
    public int TotalMovesPlayed { get; set; } = 0;
    public int TotalCapturesMade { get; set; } = 0;

    public void Load()
    {
        VsAIWins = PlayerPrefs.GetInt("Stats_VsAIWins", 0);
        VsAILosses = PlayerPrefs.GetInt("Stats_VsAILosses", 0);
        VsAIDraws = PlayerPrefs.GetInt("Stats_VsAIDraws", 0);
        LocalMultiplayerGames = PlayerPrefs.GetInt("Stats_LocalMultiplayer", 0);
        TrainingPuzzlesSolved = PlayerPrefs.GetInt("Stats_TrainingPuzzlesSolved", 0);
        TrainingPuzzlesTotal = PlayerPrefs.GetInt("Stats_TrainingPuzzlesTotal", 0);
        BluetoothGames = PlayerPrefs.GetInt("Stats_BluetoothGames", 0);
        TotalGamesPlayed = PlayerPrefs.GetInt("Stats_TotalGames", 0);
        TotalMovesPlayed = PlayerPrefs.GetInt("Stats_TotalMoves", 0);
        TotalCapturesMade = PlayerPrefs.GetInt("Stats_TotalCaptures", 0);
    }

    public void Save()
    {
        PlayerPrefs.SetInt("Stats_VsAIWins", VsAIWins);
        PlayerPrefs.SetInt("Stats_VsAILosses", VsAILosses);
        PlayerPrefs.SetInt("Stats_VsAIDraws", VsAIDraws);
        PlayerPrefs.SetInt("Stats_LocalMultiplayer", LocalMultiplayerGames);
        PlayerPrefs.SetInt("Stats_TrainingPuzzlesSolved", TrainingPuzzlesSolved);
        PlayerPrefs.SetInt("Stats_TrainingPuzzlesTotal", TrainingPuzzlesTotal);
        PlayerPrefs.SetInt("Stats_BluetoothGames", BluetoothGames);
        PlayerPrefs.SetInt("Stats_TotalGames", TotalGamesPlayed);
        PlayerPrefs.SetInt("Stats_TotalMoves", TotalMovesPlayed);
        PlayerPrefs.SetInt("Stats_TotalCaptures", TotalCapturesMade);
        PlayerPrefs.Save();
    }

    public void RecordGameResult(GameManager.GameMode mode, int result)
    {
        // result: 1 = победа, -1 = поражение, 0 = ничья
        TotalGamesPlayed++;

        switch (mode)
        {
            case GameManager.GameMode.VsComputer:
                if (result > 0) VsAIWins++;
                else if (result < 0) VsAILosses++;
                else VsAIDraws++;
                break;

            case GameManager.GameMode.LocalMultiplayer:
                LocalMultiplayerGames++;
                break;

            case GameManager.GameMode.Bluetooth:
                BluetoothGames++;
                break;

            case GameManager.GameMode.Training:
                if (result > 0) TrainingPuzzlesSolved++;
                TrainingPuzzlesTotal++;
                break;
        }

        Save();
    }

    public void RecordMove(bool isCapture = false)
    {
        TotalMovesPlayed++;
        if (isCapture) TotalCapturesMade++;
        Save();
    }

    public void ResetAll()
    {
        VsAIWins = 0;
        VsAILosses = 0;
        VsAIDraws = 0;
        LocalMultiplayerGames = 0;
        TrainingPuzzlesSolved = 0;
        TrainingPuzzlesTotal = 0;
        BluetoothGames = 0;
        TotalGamesPlayed = 0;
        TotalMovesPlayed = 0;
        TotalCapturesMade = 0;
        Save();
    }

    public float GetWinRate()
    {
        int total = VsAIWins + VsAILosses + VsAIDraws;
        if (total == 0) return 0;
        return (float)VsAIWins / total * 100f;
    }
}
