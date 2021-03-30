using System.Collections;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeCommon;
using Shapes;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTurretsGroupProt : IViewMazeTurretsGroup
    {
        #region inject

        private IModelMazeData Data { get; }
        private IViewMazeCommon MazeCommon { get; }
        private ICoordinateConverter Converter { get; }
        private IContainersGetter ContainersGetter { get; }

        public ViewMazeTurretsGroupProt(
            IModelMazeData _Data,
            IViewMazeCommon _MazeCommon, 
            ICoordinateConverter _Converter,
            IContainersGetter _ContainersGetter)
        {
            Data = _Data;
            MazeCommon = _MazeCommon;
            Converter = _Converter;
            ContainersGetter = _ContainersGetter;
        }

        #endregion
        
        #region api
        
        public void Init() { }
        
        public void OnTurretShoot(TurretShotEventArgs _Args)
        {
            HandleTurretShot(_Args.Item, _Args.ProjectileSpeed);
        }

        #endregion
        
        #region nonpublic methods

        private void HandleTurretShot(MazeItem _Item, float _Speed)
        {
            var pos = _Item.Position;
            var go = new GameObject("Projectile");
            go.SetParent(ContainersGetter.MazeItemsContainer);
            go.transform.SetLocalPosXY(Converter.ToLocalMazeItemPosition(_Item.Position));
            var disc = go.AddComponent<Disc>();
            disc.Radius = 1f;
            disc.Color = Color.black;
            disc.SortingOrder = 30;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            
            bool isEnd = false;
            while (!isEnd)
            {
                pos += _Item.Direction;
                var item = Data.Info.MazeItems.FirstOrDefault(_Itm => _Itm.Position == pos);
                if (item == null)
                    continue;
                isEnd = true;
            }
            Coroutines.Run(HandleTurretShotCoroutine(
                rb, _Item.Position, pos, _Item.Direction, _Speed));
        }

        private IEnumerator HandleTurretShotCoroutine(
            Rigidbody2D _Projectile,
            V2Int _From,
            V2Int _To,
            V2Int _Direction,
            float _Speed)
        {
            var pos = _From.ToVector2();
            V2Int point = default;
            bool finish = false;
            yield return Coroutines.DoWhile(
                () => !finish && point != Data.CharacterInfo.Position,
                () =>
                {
                    pos += _Direction.ToVector2() * _Speed;
                    _Projectile.transform.SetLocalPosXY(Converter.ToLocalMazeItemPosition(pos));
                    point = pos.ToV2IntRound();
                    if (point == _To)
                        finish = true;
                },
                () => _Projectile.gameObject.DestroySafe());
        }
        
        #endregion
    }
}