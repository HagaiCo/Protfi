using System.Runtime.Serialization;

namespace Bank.Service.DataModels;

public class ValueModel : BaseModel
{
    [DataMember]
    public long Amount { get; set; }
    
    [DataMember]
    public string Currency { get; set; }
}