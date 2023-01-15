namespace Protfi.Common.EF
{
    public interface ICreatableEntity
    {
        long CreationTimeUnixTimeInMs { get; set; }
    }
}