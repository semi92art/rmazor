using System.Collections.Generic;
using System.IO;
using Extensions;
using Games.Maze.Utils;
using Shapes;
using UnityEngine;

namespace Games.Maze
{
    [ExecuteInEditMode]
    public class MazeLevelDesigner : MonoBehaviour
    {
        public bool drawSimple;
        public int nodeIdx;
        private int m_NodeIdxCheck;
        private MazeInfo m_Info;
        private List<GameObject> m_Neibs = new List<GameObject>();
        private void Update()
        {
            if (drawSimple)
            {
                DrawTestMazeSimple();
                drawSimple = false;
            }

            if (nodeIdx != m_NodeIdxCheck)
            {
                ShowNeibs();
            }

            m_NodeIdxCheck = nodeIdx;
        }

        private void ShowNeibs()
        {
            foreach (var neibGo in m_Neibs)
                neibGo.DestroySafe();
            m_Neibs.Clear();

            if (m_Info.PathNodes == null)
                return;
            
            foreach (var neib in m_Info.PathNodes[nodeIdx].Neighbours)
            {
                var go = new GameObject("neib");
                var neibPt = go.AddComponent<Disc>();
                neibPt.Color = Color.green;
                neibPt.Radius = m_Info.WallWidth * 1.3f;
                go.transform.SetPosXY(neib);
                neibPt.UpdateMesh(true);
                m_Neibs.Add(go);
            }
            
            var go1 = new GameObject("pt");
            var neibPt1 = go1.AddComponent<Disc>();
            neibPt1.Color = Color.red;
            neibPt1.Radius = m_Info.WallWidth * 1.3f;
            go1.transform.SetPosXY(m_Info.PathNodes[nodeIdx].Point);
            neibPt1.UpdateMesh(true);
            m_Neibs.Add(go1);
        }
        
        public void DrawTestMazeSimple()
        {
            GetParents(out var wallsParentGo, out var nodesParentGo);
            var mazeInfo = GetMazeInfo();
            m_Info = mazeInfo;
            foreach (var wall in mazeInfo.Walls)
            {
                var wallGo = new GameObject("wall");
                wallGo.SetParent(wallsParentGo);
                var wallLine = wallGo.AddComponent<Line>();
                wallLine.Start = wall.Start;
                wallLine.End = wall.End;
                wallLine.Thickness = mazeInfo.WallWidth;
                wallLine.UpdateMesh(true);
                var wallColl = wallGo.AddComponent<EdgeCollider2D>();
                wallColl.points = new[] {wall.Start, wall.End};
                wallColl.edgeRadius = mazeInfo.WallWidth * 0.5f;
            }

            if (mazeInfo.PathNodes != null)
            {
                foreach (var pathNode in mazeInfo.PathNodes)
                {
                    var pathGo = new GameObject("pathNode");
                    pathGo.SetParent(nodesParentGo);
                    var pathPt = pathGo.AddComponent<Disc>();
                    pathGo.transform.SetPosXY(pathNode.Point);
                    pathPt.Radius = pathNode.Neighbours.Count > 2 ? mazeInfo.WallWidth : mazeInfo.WallWidth * 0.6f;
                    pathPt.UpdateMesh(true);
                }    
            }
            
            nodeIdx = m_NodeIdxCheck = 0;
            ShowNeibs();
        }

        private void GetParents(out GameObject _WallsParent, out GameObject _NodesParent)
        {
            var contentGo = CreateGameObject("Level Content");
            _WallsParent = CreateGameObject("Walls", contentGo);
            _NodesParent = CreateGameObject("Nodes", contentGo);
        }

        private static GameObject CreateGameObject(string _Name, GameObject _Parent = null)
        {
            var go = GameObject.Find(_Name);
            if (go != null)
                go.DestroySafe();
            go = new GameObject(_Name);
            if (_Parent != null)
                go.SetParent(_Parent);
            return go;
        }
        
        private MazeInfo GetMazeInfo()
        {
            string walls = @"C:\temp\triang.svg";
            string wallsText = File.ReadAllText(walls);
            var mazeInfo = MazeSvgParser.ParseSvg(wallsText);
            return mazeInfo;
        }
    }
}