using System.Collections.Generic;
using UnityEngine;

namespace Views
{
    public class RegionView : BaseView
    {
        [SerializeField] private LineRenderer _lineRenderer;

        public void Init()
        {
            transform.position = new Vector3(0, 0, -2);
            SetLineRendererSettings(_lineRenderer);
        }
        
        public void DrawBorders(List<Vector3> points)
        {
            _lineRenderer.positionCount = points.Count;
            _lineRenderer.SetPositions(points.ToArray());
        }
        
        private void SetLineRendererSettings(LineRenderer lineRenderer)
        {
            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.08f;
        }
    }
}