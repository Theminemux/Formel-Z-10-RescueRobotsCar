namespace RescueRobotsCar.Driving
{
    public class CarCoordinator
    {
        public enum CarState
        {
            Idle,
            Stopped,
            FollowingLine,
            TurningOnIntersection,
            Finished
        }

        private readonly CarState _state;
        private readonly Queue<Func<Task, CancellationToken>> _actionQueue;

        public CarCoordinator()
        {
            _state = CarState.Idle;
            _actionQueue = new Queue<Func<Task, CancellationToken>>();
        }

        public void Start() 
        {
        
        }

        public void Pause() 
        { 
        
        }

        public void Continue() 
        {

        }

        public void ImportNewRoute()
        {

        }

        public void Reset()
        {

        }

        public async Task FollowLineUntilCorner(CancellationToken ct)
        {

        }
    }
}
