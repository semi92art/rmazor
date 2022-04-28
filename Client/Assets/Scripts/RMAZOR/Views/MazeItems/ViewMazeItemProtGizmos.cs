using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Exceptions;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public partial class ViewMazeItemProt
    {
        private const float AdditionalScale = 0.25f;

        private float GetScale()
        {
            return AdditionalScale * scale;
        }
        
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;
            if (props.Blank)
            {
                Gizmos.color = Color.magenta;
                DrawGizmosBlankNode();
            }
            if (props.IsNode)
            {
                if (!props.IsStartNode)
                    return;
                Gizmos.color = Color.yellow;
                DrawGizmosStartNode();
                return;
            }
            Gizmos.color = GetShapeColor(props.Type, false, props.IsNode);
            switch (props.Type)
            {
                case EMazeItemType.Block:
                case EMazeItemType.ShredingerBlock:
                case EMazeItemType.GravityBlockFree:
                case EMazeItemType.GravityTrap:
                case EMazeItemType.Bazooka:
                    break;
                case EMazeItemType.TrapIncreasing:
                    DrawGizmosTrapIncreasing();
                    break;
                case EMazeItemType.TrapMoving:
                case EMazeItemType.GravityBlock:
                    DrawGizmosPath();
                    break;
                case EMazeItemType.TrapReact:
                case EMazeItemType.Turret:
                    DrawGizmosDirections();
                    break;
                case EMazeItemType.Portal:
                    DrawGizmosPortal();
                    break;
                case EMazeItemType.Springboard:
                    DrawGizmosSpringboard();
                    break;
                case EMazeItemType.Hammer:
                    DrawGizmosHammer();
                    break;
                default: throw new SwitchCaseNotImplementedException(props.Type);
            }
        }

        private void DrawGizmosStartNode()
        {
            var p = (Vector3) ToWorldPosition(props.Position);
            Gizmos.DrawSphere(p, GetScale());
        }

        private void DrawGizmosBlankNode()
        {
            var p = (Vector3) ToWorldPosition(props.Position);
            Gizmos.DrawCube(p, Vector3.one * GetScale());
        }

        private void DrawGizmosTrapIncreasing()
        {
            var p1 = new V2Int(props.Position.X - 1, props.Position.Y - 1);
            var p2 = new V2Int(props.Position.X - 1, props.Position.Y + 1);
            var p3 = new V2Int(props.Position.X + 1, props.Position.Y + 1);
            var p4 = new V2Int(props.Position.X + 1, props.Position.Y - 1);

            var p1V2 = (Vector3) ToWorldPosition(p1);
            var p2V2 = (Vector3) ToWorldPosition(p2);
            var p3V2 = (Vector3) ToWorldPosition(p3);
            var p4V2 = (Vector3) ToWorldPosition(p4);

            Gizmos.DrawLine(p1V2, p2V2);
            Gizmos.DrawLine(p2V2, p3V2);
            Gizmos.DrawLine(p3V2, p4V2);
            Gizmos.DrawLine(p4V2, p1V2);
            Gizmos.DrawLine(p1V2, p3V2);
            Gizmos.DrawLine(p2V2, p4V2);

            Gizmos.DrawSphere(p1V2, 0.5f * GetScale());
            Gizmos.DrawSphere(p2V2, 0.5f * GetScale());
            Gizmos.DrawSphere(p3V2, 0.5f * GetScale());
            Gizmos.DrawSphere(p4V2, 0.5f * GetScale());
        }

        private void DrawGizmosPath()
        {
            if (!props.Path.Any())
                return;
            for (int i = 0; i < props.Path.Count; i++)
            {
                var pos = props.Path[i];
                Gizmos.DrawSphere(ToWorldPosition(pos), 0.5f * GetScale());
                if (i == props.Path.Count - 1)
                    return;
                Gizmos.DrawLine(ToWorldPosition(pos), ToWorldPosition(props.Path[i + 1]));
            }
        }

        private void DrawGizmosDirections()
        {
            if (!props.Directions.Any())
                return;
            var pos = ToWorldPosition(props.Position);
            foreach (var dir in props.Directions)
            {
                Gizmos.DrawSphere(ToWorldPosition(props.Position + dir), 0.5f * GetScale());
                Gizmos.DrawLine(pos, ToWorldPosition(props.Position + dir));
            }
        }

        private void DrawGizmosPortal()
        {
#if UNITY_EDITOR
            if (props.Pair == default)
                return;
            var pairItem = LevelDesigner.Instance.maze
                .FirstOrDefault(_Item => _Item.props.Position == props.Pair);
            if (pairItem != null && (pairItem.props.Type != EMazeItemType.Portal || pairItem.props.IsNode))
            {
                props.Pair = default;
                return;
            }
            var pos = ToWorldPosition(props.Position);
            var pairPos = ToWorldPosition(props.Pair);
            Gizmos.DrawSphere(pos, 0.5f * GetScale());
            Gizmos.DrawSphere(pairPos, 0.5f * GetScale());
            Gizmos.DrawLine(pos, pairPos);
#endif
        }

        private void DrawGizmosSpringboard()
        {
            if (!props.Directions.Any())
                return;
            var pos = ToWorldPosition(props.Position);
            var dir = props.Directions.First();
            var dirs = new List<V2Int>();
            if (dir == V2Int.Up + V2Int.Left)
                dirs = new List<V2Int> {V2Int.Up, V2Int.Left};
            else if (dir == V2Int.Up + V2Int.Right)
                dirs = new List<V2Int> {V2Int.Up, V2Int.Right};
            else if (dir == V2Int.Down + V2Int.Left)
                dirs = new List<V2Int> {V2Int.Down, V2Int.Left};
            else if (dir == V2Int.Down + V2Int.Right)
                dirs = new List<V2Int> {V2Int.Down, V2Int.Right};
            foreach (var direct in dirs)
            {
                Gizmos.DrawSphere(ToWorldPosition(props.Position + direct), 0.3f * GetScale());
                Gizmos.DrawLine(pos, ToWorldPosition(props.Position + direct));
            }
        }

        private void DrawGizmosHammer()
        {
            if (!props.Directions.Any())
                return;
            var pos = ToWorldPosition(props.Position);
            var dir = props.Directions.First();
            Gizmos.DrawSphere(ToWorldPosition(props.Position + dir), 0.35f * GetScale());
            Gizmos.DrawLine(pos, ToWorldPosition(props.Position + dir));
            if (!props.Args.Any())
                return;
            string angleArg = props.Args[0].Split(':')[1];
            string clockwiseArg = props.Args[1].Split(':')[1];

            static V2Int RotateDirPointAroundCenterBy90DegOneTime(V2Int _Direction, bool _Clockwise)
            {
                var res = _Direction;
                if (_Direction == V2Int.Up)
                    res = _Clockwise ? V2Int.Right : V2Int.Left;
                else if (res == V2Int.Down)
                    res = !_Clockwise ? V2Int.Right : V2Int.Left;
                else if (res == V2Int.Left)
                    res = _Clockwise ? V2Int.Up : V2Int.Down;
                else if (res == V2Int.Right)
                    res = !_Clockwise ? V2Int.Up : V2Int.Down;
                return res;
            }
            V2Int RotateDirPointAroundCenterBy90Deg(int _Count, bool _Clockwise)
            {
                var newDir = dir;
                for (int i = 0; i < _Count; i++)
                    newDir = RotateDirPointAroundCenterBy90DegOneTime(newDir, _Clockwise);
                return props.Position + newDir;
            }
            if (!int.TryParse(angleArg, out int angle))
                return;
            bool closkwise = clockwiseArg == "true";
            if (angle >= 90)
            {
                var dir90 = ToWorldPosition(RotateDirPointAroundCenterBy90Deg(1, closkwise));
                Gizmos.DrawLine(pos, dir90);
                Gizmos.DrawSphere(dir90, 0.25f * GetScale());
            }
            if (angle >= 180)
            {
                var dir180 = ToWorldPosition(RotateDirPointAroundCenterBy90Deg(2, closkwise));
                Gizmos.DrawLine(pos, dir180);
                Gizmos.DrawSphere(dir180, 0.2f * GetScale());
            }
            if (angle >= 270)
            {
                var dir270 = ToWorldPosition(RotateDirPointAroundCenterBy90Deg(3, closkwise));
                Gizmos.DrawLine(pos, dir270);
                Gizmos.DrawSphere(dir270, 0.15f * GetScale());
            }
        }

        private Vector2 ToWorldPosition(Vector2 _Point)
        {
            return Converter.ToLocalMazeItemPosition(_Point);
        }
    }
}