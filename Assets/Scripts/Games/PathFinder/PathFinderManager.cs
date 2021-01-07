using Extensions;
using Managers;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Games.PathFinder
{
    public class PathFinderManager : GameManagerBase
    {
        #region singleton
        
        private static PathFinderManager _instance;

        public static PathFinderManager Instance
        {
            get
            {
                if (_instance is PathFinderManager ptm && !ptm.IsNull()) 
                    return _instance;
                var go = new GameObject("Game Manager");
                _instance = go.AddComponent<PathFinderManager>();
                return _instance;
            }
        }
        
        #endregion

        #region public members

        public Tile selectedTile = null;
        public TileBehaviour originTileTb = null;
        public TileBehaviour destinationTileTb = null;
        
        #endregion
        
        #region nonpublic members
        
        private IGridItemsGenerator m_GridItemsGenerator;

        #endregion
        
        #region api

        public bool DoInstantiate { set => m_GridItemsGenerator.DoInstantiate = value; }

        public override void Init(int _Level)
        {
            m_GridItemsGenerator = new GridItemsGenerator();
           // m_GridItemsGenerator.GenerateItems();
            // m_PointItemsGenerator = new PointItemsGenerator(
            //     new Dictionary<PointType, UnityAction>
            // {
            //     {PointType.Default, () => MainScoreController.Score++},
            //     {PointType.Bad, () => LifesController.MinusOneLife()},
            //     {PointType.BonusGold, () => RevenueController.AddRevenue(MoneyType.Gold, 300)}
            // });
            // SpriteRenderer background = new GameObject().AddComponent<SpriteRenderer>();
            // background.sprite = PrefabInitializer.GetObject<Sprite>("points_tapper", "background");
            // GameUtils.FillByCameraRect(background);
            // background.color = ColorUtils.GetColorFromPalette("Points Tapper", "Background");
            // background.sortingOrder = SortingOrders.Background;
            GameMenuUi = new PathFinderGameMenuUi();
            base.Init(_Level);
        }
       
        #endregion

        #region public methods

        public void GenerateOriginTileTb()
        {
            //генерация начала пути: в первой строке сетки
        }
        
        public void GenerateDestinationTileTb()
        {
            //генерация окончания пути: в последней строке сетки
        }
        
        public void GenerateAndShowPath()
        {
            //генерация случайного пути
            //Don't do anything if origin or destination is not defined yet
            if (originTileTb == null || this.destinationTileTb == null)
            {
                DrawPath(new List<Tile>());
                return;
            }
            //We assume that the distance between any two adjacent tiles is 1
            //If you want to have some mountains, rivers, dirt roads or something else which might slow down the player you should replace the function with something that suits better your needs
            var path = PathFinderInGame.FindPath(originTileTb.tile, this.destinationTileTb.tile);
            DrawPath(path);
            // MovementController mc = CombatController.instanceCombatController.selectedUnit.GetComponent<MovementController>();
            // mc.StartMoving(path.ToList());
        }
        
        public void DrawPath(IEnumerable<Tile> _Path)
        {
            //Отрисовка пути
            // if (this.m_path == null)
            //     this.m_path = new List<GameObject>();
            // //Destroy game objects which used to indicate the path
            // this.m_path.ForEach(Destroy);
            // this.m_path.Clear();
            //
            // //Lines game object is used to hold all the "Line" game objects indicating the path
            // GameObject lines = GameObject.Find("Lines");
            // if (lines == null)
            //     lines = new GameObject("Lines");
            // foreach (Tile tile in _Path)
            // {
            //     var line = (GameObject)Instantiate(this.line);
            //     //calcWorldCoord method uses squiggly axis coordinates so we add y / 2 to convert x coordinate from straight axis coordinate system
            //     Vector2 gridPos = new Vector2(tile.X, tile.Y);
            //     line.transform.position = CalcWorldCoord(gridPos);
            //     this.m_path.Add(line);
            //     line.transform.parent = lines.transform;
            // }
        }

        public void CheckPath()
        {
            //проверка правильности отрисованного пути 
        }

        #endregion

        #region nonpublic methods

        protected override void InitTouchSystem()
        {
            // var go = new GameObject("Lean Select");
            // LeanSelect ls = go.AddComponent<LeanSelect>();
            // ls.SelectUsing = LeanSelectBase.SelectType.Overlap2D;
            // ls.Search = LeanSelectBase.SearchType.GetComponent;
            // ls.Camera = Camera.main;
            // ls.LayerMask = LayerMask.GetMask(LayerNames.Touchable);
            // ls.MaxSelectables = 1;
            // ls.Reselect = LeanSelect.ReselectType.SelectAgain;
            // ls.AutoDeselect = false;
            // ls.SuppressMultipleSelectWarning = false;
            //
            // LeanFingerDown lfd = go.AddComponent<LeanFingerDown>();
            // lfd.IgnoreStartedOverGui = true;
            // lfd.OnFinger.AddListener(ls.SelectScreenPosition);
            // base.InitTouchSystem();
        }
        
        protected override void OnBeforeLevelStarted(LevelStateChangedArgs _Args)
        {
            DoInstantiate = false;
            base.OnBeforeLevelStarted(_Args);
        }
        
        protected override void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            DoInstantiate = true;
            ((IOnLevelStartedFinished)m_GridItemsGenerator).OnLevelStarted(_Args);
            base.OnLevelStarted(_Args);
        }
        
        protected override void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            DoInstantiate = false;
            ((IOnLevelStartedFinished)m_GridItemsGenerator).OnLevelFinished(_Args);
            base.OnLevelFinished(_Args);
        }
        
        protected override void OnTimeEnded()
        {
            DoInstantiate = false;
            ((IOnLevelStartedFinished)m_GridItemsGenerator).OnLevelFinished(null);
            base.OnTimeEnded();
        }

        protected override void OnLifesEnded()
        {
            DoInstantiate = false;
            ((IOnLevelStartedFinished)m_GridItemsGenerator).OnLevelFinished(null);
            base.OnLifesEnded();
        }
        
        protected override float LevelDuration(int _Level)
        {
            if (_Level < 5)
                return 200f;
            if (_Level < 10)
                return 25f;
            if (_Level < 20)
                return 30f;
            if (_Level < 50)
                return 40f;
            return 60f;
        }
        
        protected override int NecessaryScore(int _Level)
        {
            if (_Level < 5)
                return 10;
            if (_Level < 10)
                return 15;
            if (_Level < 20)
                return 20;
            if (_Level < 50)
                return 30;
            return 40;
        }

        #endregion

        #region engine methods

        protected override void OnDestroy()
        {
            m_GridItemsGenerator.ClearGrid();
            base.OnDestroy();
        }
        
        #endregion
    }
   
}
