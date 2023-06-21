using System.Linq;
using Data;
using UnityEngine;

public class RegionLevelMap : MonoBehaviour
{
    [SerializeField] private Tower[] _towers;
    [SerializeField] private RegionLevelMap _nextRegion;
    [SerializeField] private GameObject _backGround;

    private void OnEnable()
    {
        foreach (var t in _towers)
            t.SetStateTowers(PlayerData.LevelCompleted);
        
        if (CheckRegion())
            OpenNextRegion();
    }

    private void Open()
    {
        _backGround.SetActive(false);
    }

    private void OpenNextRegion()
    {
        _nextRegion.Open();
    }

    private bool CheckRegion()
    {
        return _nextRegion != null && _towers.All(tower => tower.State == StateLevel.Ruined);
    }
}
