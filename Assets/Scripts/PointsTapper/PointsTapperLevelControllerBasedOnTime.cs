namespace PointsTapper
{
    public class PointsTapperLevelControllerBasedOnTime : LevelControllerBasedOnTime
    {
        public PointsTapperLevelControllerBasedOnTime()
        {
            TimeController = new PointsTapperDefaultTimeController();
        }
    }
}