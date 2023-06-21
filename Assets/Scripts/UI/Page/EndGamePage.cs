// Copyright (c) 2012-2022 FuryLion Group. All Rights Reserved.

using FuryLion.UI;
using UnityEngine;
using Data;
using FuryLion;

public sealed class EndGamePage : Page
{
    [SerializeField] private Text _result;
    [SerializeField] private Text _reward;
    [SerializeField] private BaseButton _restart;
    [SerializeField] private BaseButton _next;
    [SerializeField] private BaseButton _menu;

    private static EndGamePage _instance;
    public static bool IsWin = false;
    
    private void OnRestartButton()
    {
        Gameplay.Init(LevelManager.ResetLevel());
        PageManager.Open<GamePage>();
    }
    
    private void OnMenuButton()
    {
        SoundManager.StopAllSounds();
        SoundManager.PlaySound(Sounds.Music.Main, true);
        PageManager.Open<MainPage>();
    }

    private void OnNextLvlButton()
    {
        Gameplay.Init(LevelManager.CurrentLevel + 1);
        PageManager.Open<GamePage>();
    }

    private void SetResult()
    {
        var reward = 1;
        if (IsWin)
            reward = LevelManager.CurrentLevel <= PlayerData.LevelCompleted
                ? LevelManager.RewardCurrentLevel / 3 : LevelManager.RewardCurrentLevel;

        if (IsWin && PlayerData.LevelCompleted < LevelManager.CurrentLevel)
            PlayerData.LevelCompleted = LevelManager.CurrentLevel;

        _reward.Value = "+ " + reward;
        PlayerData.GameCurrency += reward;
        _result.Value = IsWin ? "You win!" : "You lost!";
        _next.gameObject.SetActive(IsWin && LevelManager.LevelsInfo.Levels.Count - 1 > LevelManager.CurrentLevel);
    }

    protected override void OnCreate()
    {
        _restart.Click += OnRestartButton;
        _menu.Click += OnMenuButton;
        _next.Click += OnNextLvlButton;
    }

    protected override void OnOpenComplete()
    {
        LevelManager.SaveWorlds();
        SetResult();
    }
}
