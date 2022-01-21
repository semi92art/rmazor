namespace Common.Constants
{
    public static class SceneNames
    {
        public const string Preload = "_preload";
        public const string Level = "Level";
        public const string Prototyping = "Prot";

        public static string FullName(string _SceneName)
        {
            return GetScenesPath() + _SceneName + ".unity";
        }

        public static string GetScenesPath()
        {
            return "Assets\\Scenes\\";
        }
    }
}