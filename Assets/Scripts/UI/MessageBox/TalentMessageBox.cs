// Copyright (c) 2012-2022 FuryLion Group. All Rights Reserved.

using System;
using FuryLion.UI;
using Unity.VisualScripting;
using UnityEngine;

public sealed class TalentMessageBox : MessageBox
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Text _infoText;
    [SerializeField] private Text _valueText;
    [SerializeField] private Text _newValueText;
    [SerializeField] private BaseButton _buyButton;
    [SerializeField] private BaseButton _backButton;

    private Vector3 _baseInfoTextPos;
    private Vector3 _baseBuyButtonPos;
    
    private static Vector3 _position;
    private static string _info;
    private static string _value;
    private static string _newValue;
    private static Action _buyButtonClick;
    private static bool _isAvailable;

    public static void Init(Vector3 position, string info, string value, string newValue, Action action, bool isAvailable)
    {
        _position = position;
        _info = info;
        _value = value;
        _newValue = newValue;
        _buyButtonClick = action;
        _isAvailable = isAvailable;
    }

    private void Awake()
    {
        _baseInfoTextPos = _infoText.transform.localPosition;
        _baseBuyButtonPos = _buyButton.transform.localPosition;
    }
    
    protected override void OnCreate()
    {
        _backButton.Click += CloseLast;
        _buyButton.Click += CloseLast;
    }

    protected override void OnOpenStart(ViewParam viewParam)
    {
        _panel.transform.localPosition = _position;
        _infoText.Value = _info;
        _valueText.Value = _value;
        _newValueText.Value = _newValue;
        _buyButton.Click += _buyButtonClick;
        _buyButton.gameObject.SetActive(_isAvailable);
        _valueText.gameObject.SetActive(_isAvailable);
        _newValueText.gameObject.SetActive(_isAvailable);
        if (!_isAvailable)
            _infoText.transform.localPosition = _baseInfoTextPos + Vector3.down * _baseInfoTextPos.y;
        else if (_valueText.Value == "" && _newValueText.Value == "")
        {
            _infoText.transform.localPosition = _baseInfoTextPos + Vector3.down * 0.1f;
            _buyButton.transform.localPosition = _baseBuyButtonPos + Vector3.up * 0.1f;
        }
        else
        {
            _infoText.transform.localPosition = _baseInfoTextPos;
            _buyButton.transform.localPosition = _baseBuyButtonPos;
        }
    }

    protected override void OnCloseStart()
    {
        _buyButton.Click -= _buyButtonClick;
    }
}
