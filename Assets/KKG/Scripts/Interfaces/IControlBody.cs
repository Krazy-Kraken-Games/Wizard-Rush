/// <summary>
/// This interface corresponds to bodies in game which you can control
/// For example, the player will connect with cannon to use it
/// </summary>
public interface IControlBody
{
    void TakeControl(ulong _objectID);
}
