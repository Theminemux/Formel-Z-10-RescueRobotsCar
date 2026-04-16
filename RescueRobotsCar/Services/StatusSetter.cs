namespace RescueRobotsCar.Services
{
    public class StatusSetter
    {
        public enum EStatus
        {
            Stopped = 0,
            Driving = 1,
            Pause = 2,
            Finished = 3
        }

        private int _status;

        private readonly SemaphoreSlim _sema = new SemaphoreSlim(1, 1);

        public async Task SetStatusAsync(int newStatus)
        {
            await _sema.WaitAsync();
            try
            {
                _status = newStatus;
            }
            finally
            {
                _sema.Release();
            }
        }

        public async Task SetStatusAsync(EStatus newStatus) => await SetStatusAsync((int)newStatus);
        public async Task<int> GetStatusAsync()
        {
            await _sema.WaitAsync();
            try
            {
                return _status;
            }
            finally
            {
                _sema.Release();
            }
        }
    }
}
