using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using FuryLion.UI;
using UnityEngine;
using Views;

public class BuildingPanel : Element, IRecyclable
{
    [SerializeField] private List<Sprite> _sprites;

    private readonly List<BuildingButton> _buttons = new List<BuildingButton>();

    private readonly Dictionary<BuildingType, Sprite> _spritesDict = new Dictionary<BuildingType, Sprite>();
    
    private readonly Color _color = new Color(170, 46, 0, 255);

    private float _radius = 0.9f;

    public CellEntity Target { get; private set; }

    private void Awake()
    {
        SetSprites();
        SetData();
        Hide();
        foreach (var b in _buttons)
            b.Click += () => OnBuildingButtonCLick(b);
    }
    
    private void OnBuildingButtonCLick(BuildingButton button) => ShopManager.Buy(Target, button.Type);

    public void Show(CellView target)
    {
        if (target.CellEntity.TeamID != 1 || target.CellEntity.Building != null)
            return;

        Target = target.CellEntity;

        transform.position = new Vector3(target.transform.position.x, target.transform.position.y, 
            transform.position.z);
        gameObject.SetActive(true);
        
        ShowCost();
    }

    private void ShowCost()
    {
        foreach (var button in _buttons.Where(button => button.Type != BuildingType.Upgrade))
            button.Cost = ShopManager.GetCost(button.Type).ToString();
    }

    public void SwitchColor(CustomerController customer)
    {
        foreach (var button in _buttons)
            button.CostColor = customer.Money < int.Parse(button.Cost) ? Color.white : _color;
    }

    private void SetData()
    {
        var center = transform.position;
        var availableBuildingsCount = LevelManager.PlayerData.AvailableBuildings.Count;
        _radius = 0.9f + availableBuildingsCount * 0.01f;
        transform.localScale = new Vector3(transform.localScale.x + availableBuildingsCount * 0.01f,
            transform.localScale.y + availableBuildingsCount * 0.01f,
            transform.localScale.z);

        for (var i = 0; i < availableBuildingsCount; i++)
        {
            var angle =  i * 360 / availableBuildingsCount;
            var pos = PlaceInCircle(center, _radius ,angle);
            var button = Recycler.Get<BuildingButton>();
            button.transform.localScale =
                new Vector3(
                    button.transform.localScale.x - availableBuildingsCount * 0.0035f,
                    button.transform.localScale.y - availableBuildingsCount * 0.0035f, 1f);
            
            button.transform.position = pos;
            button.Type = LevelManager.PlayerData.AvailableBuildings[i];
            button.Spr = _spritesDict[button.Type];
            button.Name = button.Type switch
            {
                BuildingType.Barracks => "Казармы",
                BuildingType.Farm => "Ферма",
                BuildingType.Forge => "Кузница",
                BuildingType.Fort => "Форт",
                BuildingType.Mine => "Шахта",
                BuildingType.Quarry => "Каменоломня",
                BuildingType.Stable => "Конюшня",
                BuildingType.Workshop => "Мастерская",
                BuildingType.WizardTower => "Башня магов",
                _ => button.Name
            };
            button.transform.SetParent(transform);
            switch (button.Type)
            {
                case BuildingType.Farm:
                    ImageUtility.SetImageScale(button.Image, 0.8f, 1.1f);
                    break;
                case BuildingType.Workshop:
                case BuildingType.Quarry:
                    ImageUtility.SetImageScale(button.Image, 1.2f, 1.3f);
                    break;
                case BuildingType.Fort:
                    ImageUtility.SetImageScale(button.Image, 0.9f);
                    break;
            }

            _buttons.Add(button);
        }
    }

    private void SetSprites()
    {
        for (var i = 0 ; i < (int) BuildingType.Settlement; i++)
            _spritesDict.Add((BuildingType) Enum.GetValues(typeof(BuildingType)).GetValue(i), _sprites[i]);

        _spritesDict.Add(BuildingType.Upgrade, _sprites[_sprites.Count - 1]);
    }

    private static Vector3 PlaceInCircle(Vector3 center, float radius, int angle)
    {
        var pos = new Vector3();
        var (x, y, z) = center;
        pos.x = x + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        pos.y = y + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        pos.z = z;
        return pos;
    }

    public void Hide() => gameObject.SetActive(false);
}
