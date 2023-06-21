// Copyright (c) 2012-2022 FuryLion Group. All Rights Reserved.

using FuryLion.UI;
using UnityEngine;

public sealed class InfoMessageBox : MessageBox
{
    [SerializeField] private BaseButton _okButton;
    [SerializeField] private Text _infoText;
    
    private static string _info;
    
    public static bool IsOpen { get; private set; }
    
    public static void Init(string info)
    {
        _info = info;
    }

    protected override void OnCreate()
    {
        _okButton.Click += CloseLast;
    }

    protected override void OnOpenStart(ViewParam viewParam)
    {
        IsOpen = true;
        _infoText.Value = _info;
    }

    protected override void OnCloseComplete()
    {
        IsOpen = false;
    }
}

public static class MessageInfo
{
    public static string LockedLevelInfo = "Данный уровень пока не доступен для вас";
    public static string DevelopingLevelInfo = "Данный уровень находится в разработке";
    public static string AcademyInfo =
        "Добро пожаловать в академию! Здесь вы можете улучшать характеристики юнитов и открывать новые здания.";
}
