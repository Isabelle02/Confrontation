﻿using FuryLion.UI;
using UnityEngine;
using Views;

public class BuildingButton : BaseButton
{
    [SerializeField] private Text _coinCost;

    [SerializeField] private Image _image;

    [SerializeField] private Text _name;

    public Image Image => _image;
    
    public BuildingType Type { get; set; }

    public Sprite Spr { set => _image.Sprite = value; }
    
    public Color CostColor
    {
        get => _coinCost.Color;
        set => _coinCost.Color = value;
    }

    public string Cost
    {
        get => _coinCost.Value;
        set => _coinCost.Value = value;
    }

    public string Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }
}
