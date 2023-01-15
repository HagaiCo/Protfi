namespace Protfi.Common.EF
{
    public interface IModifiableEntity
    {
        long ModificationTimeUnixTimeInMs { get; set; }
    }
}