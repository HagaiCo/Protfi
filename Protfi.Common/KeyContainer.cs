using System.Runtime.Serialization;

namespace Protfi.Common;

public class KeyContainer
{
    public KeyContainer(byte[] key, byte[] iv, Guid userIdentifier)
    {
        Key = key;
        Iv = iv;
        UserIdentifier = userIdentifier;
    }
    
    [DataMember]
    public byte[] Key { get; set; }
    
    [DataMember]
    public byte[] Iv { get; set; }
    
    [DataMember]
    public Guid UserIdentifier { get; set; }
}