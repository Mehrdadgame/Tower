/* This code snippet defines a C# interface named `IUpgradeable`. This interface declares three methods
that any class implementing it must provide: */
public interface IUpgradeable
{
    void Upgrade();
    bool CanUpgrade();
    int GetUpgradeCost();
}