using System;
using System.Collections.Generic;
using Core;
using Entities;
using FuryLion.UI;
using UnityEngine;
using Views;

namespace Data
{
    [Serializable]
    public class RegionData : ObjectData
    {
        public List<CellData> Cells = new List<CellData>();
        public List<Vector3> Points = new List<Vector3>();

        protected override BaseEntity CreateEntity(IWorld world)
        {
            var region = new RegionEntity(this);
            var regionView = Recycler.Get<RegionView>();
            regionView.Init();
            regionView.DrawBorders(Points);
            foreach (var c in Cells)
            {
                region.AddCell(world.CreateNewObject(c) as CellEntity);
            }
        
            return region;
        }
    }
}