public class UserLamaData
{
    public string? name { get; set; }
    public byte health { get; set; }
    public byte hunger { get; set; }

    public UserLamaData(string? name = null, byte health = Constants.MAX_LAMA_HEALTH, byte hunger = Constants.MIN_LAMA_HUNGER)
    {
        this.name = name;
        this.health = health;
        this.hunger = hunger;
    }
}