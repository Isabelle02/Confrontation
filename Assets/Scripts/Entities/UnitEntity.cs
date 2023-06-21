using System;
using Core;
using Data;
using DG.Tweening;
using FuryLion.UI;
using Interfaces;
using UnityEngine;
using Views;
using Random = UnityEngine.Random;

namespace Entities
{
    public class UnitEntity : BaseEntity<UnitData>, IBoostable, IBonusDependent
    {
        private UnitView _unitView;
        private Tween _tween;
        private float _boost = 1;
        private Vector3 _targetPos;
        private float _speed;

        private float Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                if (_tween == null)
                    return;
                
                var duration = (_targetPos - _unitView.Position).magnitude / _speed;
                _tween.Kill();
                _tween = _unitView.transform.DOMove(_targetPos, duration);
            }
        }
        
        public float Force { get; private set; }

        public float DebuffProtection { get; private set; }

        public int TeamID
        {
            get => Data.TeamID;
            set => Data.TeamID = value;
        }

        public Vector3 TargetPosition => Data.TargetPosition;

        public float Boost
        {
            get => _boost;
            set
            {
                Speed = Speed / _boost * value;
                _boost = value;
            }
        }

        public event Action<UnitEntity> Crashed;

        public UnitEntity(UnitData data, IWorld world) : base(data)
        {
            _speed = data.Speed;
            Force = data.Force;
            DebuffProtection = data.DebuffProtection;
            
            _unitView = Recycler.Get<UnitView>();
            _unitView.gameObject.SetActive(false);
            _unitView.UnitEntity = this;
            UnitUtility.AddUnit(this);
            Crashed += world.RemoveEntity;
        }

        public void Move()
        {
            _unitView.gameObject.SetActive(true);
            _unitView.Position = (Vector3) Random.insideUnitCircle * 0.1f + Data.Position + Vector3.back;
            _targetPos = (Vector3) Random.insideUnitCircle * 0.1f + Data.TargetPosition + Vector3.back;
            var duration = (_targetPos - _unitView.Position).magnitude / Speed;
            _unitView.LookAt(_targetPos);
            _unitView.SetAnimations(TeamID - 1);
            _unitView.PlayMoveAnim();
            _tween = _unitView.transform.DOMove(_targetPos, duration);
        }
        
        public void AddSpeedBonus(float bonus)
        {
            Speed += bonus;
        }

        public void AddForceBonus(float bonus)
        {
            Force += bonus;
        }
        
        public void AddDebuffProtectionBonus(float bonus)
        {
            DebuffProtection += bonus;
        }

        public void AddReproductionBonus(float bonus) { }

        public void AddProtectionBonus(float bonus) { }

        public void SetPause(bool isPaused)
        {
            if (isPaused)
            {
                _tween?.Pause();
                Data.Position = _unitView.Position;
            }
            else
                _tween?.Play();
        }

        public void Crash()
        {
            if (Crashed == null)
                return;

            _tween?.Kill();
            Recycler.Release(_unitView);
            UnitUtility.RemoveUnit(this);
            Crashed.Invoke(this);
            Crashed = null;
        }
    }
}
