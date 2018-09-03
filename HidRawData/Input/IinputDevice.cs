namespace Djlastnight.Input
{
    public interface IInputDevice
    {
        string DeviceID { get; }

        string Description { get; }

        string Vendor { get; }

        string Product { get; }

        string FriendlyName { get; }

        DeviceType DeviceType { get; }
    }
}
