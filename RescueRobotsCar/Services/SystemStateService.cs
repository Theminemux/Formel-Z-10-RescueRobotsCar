namespace RescueRobotsCar.Services
{
    public class SystemStateService
    {
        public bool IsLoggedIn { get; private set; } = false;
        public string? OrangePiIp { get; private set; } = null;

        public bool IsEsp32Connected { get; private set; } = false;
        public bool IsMotorsInitialized { get; private set; } = false;
        public bool IsMapLoaded { get; private set; } = false;
        public bool IsCompassCalibrated { get; private set; } = false;

        public bool IsOperational =>
            IsLoggedIn &&
            IsEsp32Connected &&
            IsMotorsInitialized &&
            IsMapLoaded &&
            IsCompassCalibrated;

        private SemaphoreSlim _stateLock = new SemaphoreSlim(1, 1);

        public async Task SetLoggedIn(bool value)
        {
            await _stateLock.WaitAsync();
            try
            {
                IsLoggedIn = value;
            }
            finally
            {
                _stateLock.Release();
            }
        }

        public async Task SetOrangePiIp(string? ip)
        {
            await _stateLock.WaitAsync();
            try
            {
                OrangePiIp = ip;
            }
            finally
            {
                _stateLock.Release();
            }
        }

        public async Task SetEsp32Connected(bool value)
        {
            await _stateLock.WaitAsync();
            try
            {
                IsEsp32Connected = value;
            }
            finally
            {
                _stateLock.Release();
            }
        }

        public async Task SetMotorsInitialized(bool value)
        {
            await _stateLock.WaitAsync();
            try
            {
                IsMotorsInitialized = value;
            }
            finally
            {
                _stateLock.Release();
            }
        }

        public async Task SetMapLoaded(bool value)
        {
            await _stateLock.WaitAsync();
            try
            {
                IsMapLoaded = value;
            }
            finally
            {
                _stateLock.Release();
            }
        }

        public async Task SetCompassCalibrated(bool value)
        {
            await _stateLock.WaitAsync();
            try
            {
                IsCompassCalibrated = value;
            }
            finally
            {
                _stateLock.Release();
            }
        }
    }
}
