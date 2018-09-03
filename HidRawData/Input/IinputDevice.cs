namespace Djlastnight.Input
{
    public interface IinputDevice
    {
        string DeviceID { get; }

        string Description { get; }

        string Vendor { get; }

        string Product { get; }

        string FriendlyName { get; }

        DeviceType DeviceType { get; }
    }
}
