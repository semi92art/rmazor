using System.Collections;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Shapes;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTurretsGroupProt : ViewMazeTurretsGroupBase
    {
        #region inject
        
        public ViewMazeTurretsGroupProt(
            IModelMazeData _Data,
            IViewMazeCommon _MazeCommon, 
            ICoordinateConverter _Converter,
            IContainersGetter _ContainersGetter)
            : base(_Data, _MazeCommon, _Converter, _ContainersGetter) { }

        #endregion
        
        #region api
        
        public override void OnTurretShoot(TurretShotEventArgs _Args)
        {
            if (_Args.PreShoot)
                return;
            var mazeItem = _Args.Item;
            var pos = mazeItem.Position;
            var go = new GameObject("Projectile");
            go.SetParent(ContainersGetter.MazeItemsContainer);
            go.transform.SetLocalPosXY(Converter.ToLocalMazeItemPosition(mazeItem.Position));
            var disc = go.AddComponent<Disc>();
            disc.Radius = 1f;
            disc.Color = Color.black;
            disc.SortingOrder = 30;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            
            bool isEnd = false;
            while (!isEnd)
            {
                pos += mazeItem.Direction;
                var item = Data.Info.MazeItems.FirstOrDefault(_Itm => _Itm.Position == pos);
                if (item == null)
                    continue;
                isEnd = true;
            }
            Coroutines.Run(HandleTurretShotCoroutine(
                rb, mazeItem.Position, pos, mazeItem.Direction, _Args.ProjectileSpeed));
        }
        
        #endregion

        #region nonpublic methods

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