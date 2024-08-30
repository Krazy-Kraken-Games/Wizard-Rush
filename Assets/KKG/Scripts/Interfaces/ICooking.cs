public interface ICooking
{
    void CheckIngredients();

    void Cook();
}

public enum StationState
{
    READY = 0,
    COOK = 1,
    COLLECT = 2
}
