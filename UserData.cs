public class UserData
{
    public bool isUserRegistered { get; set; }
    public bool isUserCalledCommandStart { get; set; }
    public UserLamaData userLamaData { get; set; }
    private Timer? _lamaAddHungerUpdateTimer;
    private Timer? _lamaSubHealthUpdateTimer;

    public UserData(bool isUserCalledCommandStart)
    {
        this.isUserCalledCommandStart = isUserCalledCommandStart;

        userLamaData = new UserLamaData();
        StartLamaAddHungerTimer();
    }

    private void StartLamaAddHungerTimer()
    {
        _lamaAddHungerUpdateTimer = new Timer(OnLamaHungerAddUpdate, null, 0, Constants.UPDATE_LAMA_ADD_HUNGER_TIME);
    }

    private void StartLamaSubHealthTimer()
    {
        _lamaSubHealthUpdateTimer = new Timer(OnLamaHealthSubUpdate, null, 0, Constants.UPDATE_LAMA_SUB_HEALTH_TIME);
    }

    private void OnLamaHungerAddUpdate(object? state)
    {
        byte lamaHunger = userLamaData.hunger;
        lamaHunger = (byte)Math.Clamp(lamaHunger + 1, Constants.MIN_LAMA_HUNGER, Constants.MAX_LAMA_HUNGER);

        if (lamaHunger == Constants.MAX_LAMA_HUNGER)
        {
            _lamaAddHungerUpdateTimer?.Dispose();
            StartLamaSubHealthTimer();
        }

        userLamaData.hunger = lamaHunger;
    }

    private void OnLamaHealthSubUpdate(object? state)
    {
        byte lamaHealth = userLamaData.health;
        lamaHealth = (byte)Math.Clamp(lamaHealth - 1, Constants.MIN_LAMA_HEALTH, Constants.MAX_LAMA_HEALTH);

        if (lamaHealth == Constants.MIN_LAMA_HEALTH)
        {
            _lamaSubHealthUpdateTimer?.Dispose();
        }

        userLamaData.health = lamaHealth;
    }
}