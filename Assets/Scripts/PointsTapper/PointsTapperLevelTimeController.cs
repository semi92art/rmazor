namespace PointsTapper
{
    public class PointsTapperDefaultTimeController : DefaultTimeController
    {
        public override void StartTime(int _Level)
        {
            TimeStopPredicate = () => CurrentTime > 30f;
            base.StartTime(_Level);
        }
    }
}