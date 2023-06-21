using Entities;
using FuryLion.UI;
using UnityEngine;

namespace Views
{
    public class BarracksView : BuildingView
    {
        [SerializeField] private Text _unitCountText;

        [SerializeField] private GameObject _outLine;

        public void SetUnitCount(int unitCount)
        {
            _unitCountText.Value = unitCount.ToString();
        }

        public void SetActiveOutLine(bool active) => _outLine.SetActive(active);
    }
}