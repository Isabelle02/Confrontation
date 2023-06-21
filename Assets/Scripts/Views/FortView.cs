using Interfaces;
using UnityEngine;

namespace Views
{
    public class FortView : SettlementView
    {
        public new void OnTriggerEnter2D(Collider2D other)
        {
            (BuildingEntity as IFort)?.OnCrash(other);
        }
    }
}
