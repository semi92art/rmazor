using UnityEngine;

namespace Games
{
    public interface ILevelDesigner
    {
        GameObject LevelObject { get; set; }
        GameObject StageObject { get; set; }
        int StageIndex { get; set; }
    }
}