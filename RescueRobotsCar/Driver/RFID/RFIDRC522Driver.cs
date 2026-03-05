namespace RescueRobotsCar.Driver.RFID;

public class RFIDRC522Driver : BackgroundService
{
    public string? CurrentCardData { get; private set; }
    public string? LastCardData { get; private set; }
    public DateTime LastCardTimestamp { get; private set; }
    public bool IsCardExpired { get; private set; }

    private TimeSpan ExpirationTime { get; set; }

    public RFIDRC522Driver()
    {
        ExpirationTime = TimeSpan.FromSeconds(10);
        CurrentCardData = null;
        LastCardData = null;
        LastCardTimestamp = DateTime.UtcNow;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (LastCardTimestamp + ExpirationTime < DateTime.UtcNow && !IsCardExpired)
            {
                UpdateCardDataProperty(null, true);
                Console.WriteLine("Last rfid card expired!");
            }
        }
        await Task.Delay(1000, ct);
    }

    private void UpdateCardDataProperty(string? newcarddata, bool isExpired = false)
    {
        if (CurrentCardData != newcarddata)
        {
            LastCardData = CurrentCardData;
            CurrentCardData = newcarddata;
            LastCardTimestamp = DateTime.UtcNow;
            IsCardExpired = isExpired;
        }
    }

    public void UpdateCardData(string cardData)
    {
        UpdateCardData(cardData);
        Console.WriteLine($"New card data was saved. Data: {cardData}");
    }
}