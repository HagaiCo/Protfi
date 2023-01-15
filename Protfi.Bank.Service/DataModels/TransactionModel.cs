using System.Runtime.Serialization;

namespace Bank.Service.DataModels;

public class TransactionModel : BaseModel
{
    [DataMember]
    public string From { get; set; }
    
    [DataMember]
    public string To { get; set; }
    
    [DataMember]
    public ValueModel Amount { get; set; }
    
    [DataMember]
    public long DateAsUnix { get; set; }
}