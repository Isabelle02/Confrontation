using System;
using System.Collections;
using System.Collections.Generic;
using Entities;
using FuryLion.UI;
using Interfaces;
using UnityEngine;

namespace Views
{
    public class CellView : BaseView
    {
        [SerializeField] private SpriteRenderer _outlineRenderer;
        [SerializeField] private SpriteRenderer _fog;

        [SerializeField] private GameObject _dollar;

        private Animation _animation;

        private const string HideFogName = "hidefog";
        private const string ShowFogName = "showfog";
        private const string BuyEffectName = "buyeffect";

        public CellEntity CellEntity;

        private void Awake()
        {
            _fog.gameObject.SetActive(true);
            _animation = GetComponent<Animation>();
        }

        public void SetActiveOutline(bool isActive) => _outlineRenderer.enabled = isActive;

        public List<ICell> FindNeighbours()
        {
            var cells = Physics2D.OverlapCircleAll(
                transform.position, 0.7f, LayerMask.GetMask("Cell"));

            var cellEntities = new List<ICell>();
            foreach (var c in cells)
                if (c.gameObject.TryGetComponent(out CellView cell) && cell != this)
                    cellEntities.Add(cell.CellEntity);

            return cellEntities;
        }

        public void SetFog(bool active)
        {
            if (!active)
                _animation.Play(HideFogName);
            else
            {
                _fog.enabled = true;
                _animation.Play(ShowFogName);
            }
        }

        public void HideFog()
        {
            _fog.enabled = false;
        }

        public void ShowDollar()
        {
            _dollar.SetActive(true);
            _animation.Play(BuyEffectName);
        }

        public void HideDollar()
        {
            _dollar.SetActive(false);
        }
    }
}