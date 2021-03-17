using Entities;
using Extensions;
using Shapes;
using UnityEngine;


namespace Games.RazorMaze.Views
{
    public class CharacterViewItemProt : MonoBehaviour
    {
        private Disc shape;

        public static CharacterViewItemProt Create(Transform _Parent, MazeInfo _Info)
        {
            var go = new GameObject("Character Item");
            go.SetParent(_Parent);
            var view = go.AddComponent<CharacterViewItemProt>();
            view.Init(_Info);
            return view;
        }
        
        public void SetPosition(Vector2 _Position)
        {
            transform.localPosition = _Position;
        }
        
        private void Init(MazeInfo _Info)
        {
            var scaler = new MazeScreenScaler();
            var pos = scaler.GetWorldPosition(
                _Info.Nodes[0].Position, _Info.Width, out float scale);
            InitShape(0.4f * scale, Color.blue);
            SetPosition(pos);
        }

        private void InitShape(float _Radius, Color _Color)
        {
            shape = gameObject.GetComponent<Disc>() ?? gameObject.AddComponent<Disc>();
            shape.Radius = _Radius;
            shape.Color = _Color;
        }
    }
}