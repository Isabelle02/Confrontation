using System;
using System.Collections.Generic;
using Core;
using Data;
using FuryLion.UI;
using Interfaces;
using UnityEngine;
using Views;

namespace Entities
{
    public class CellEntity : BaseEntity<CellData>, ICell
    {
        public CellView CellView { get; }

        public int TeamID
        {
            get => Data.TeamID;
            set
            {
                Data.TeamID = value;
                if (Building != null)
                    Building.TeamID = value;
                
                CellView.SetSprite(LevelManager.LevelsInfo.TeamSprites[value]);
            }
        }

        public IBuilding Building { get; set; }

        public CellEntity(CellData data) : base(data)
        {
            CellView = Recycler.Get<CellView>();
            CellView.transform.position = Data.Position;
            CellView.SetSprite(LevelManager.LevelsInfo.TeamSprites[Data.TeamID]);
            CellView.CellEntity = this;
        }

        public List<ICell> FindNeighbours()
        {
            return CellView.FindNeighbours();
        }
        
        public void CreateBuilding(BuildingType type)
        {
            Data.Building = type switch
            {
                BuildingType.Barracks => new BarracksData(),
                BuildingType.Farm => new FarmData(),
                BuildingType.Forge => new ForgeData(),
                BuildingType.Mine => new MineData(),
                BuildingType.Stable => new StableData(),
                BuildingType.WizardTower => new WizardTowerData(),
                BuildingType.Fort => new FortData(),
                BuildingType.Quarry => new QuarryData(),
                BuildingType.Workshop => new WorkhopData(),
                _ => Data.Building
            };

            if (Data.Building == null)
                return;

            Data.Building.TeamID = TeamID;
            Data.Building.Position = Data.Position - new Vector3(0, 0, 2);
            Building = Gameplay.CreateNewObject(Data.Building) as IBuilding;
            if(TeamID == 1)
                CellView.ShowDollar();
        }

        public void SetFog(bool active) => CellView.SetFog(active);
    }
}
