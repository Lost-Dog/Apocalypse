/// <summary>
/// Optional extension interface for player providers that expose survival stats
/// (temperature, stamina, infection, hunger, thirst) as framework-backed values.
/// </summary>
public interface ISurvivalStatsProvider
{
    float Temperature { get; }
    float MaxTemperature { get; }
    void SetTemperature(float value);

    float Stamina { get; }
    float MaxStamina { get; }
    void SetStamina(float value);

    float Infection { get; }
    float MaxInfection { get; }
    void SetInfection(float value);

    float Hunger { get; }
    float MaxHunger { get; }
    void SetHunger(float value);

    float Thirst { get; }
    float MaxThirst { get; }
    void SetThirst(float value);
}
