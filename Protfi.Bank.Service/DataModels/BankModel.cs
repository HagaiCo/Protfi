using System.Runtime.Serialization;

namespace Bank.Service.DataModels;

public class BankModel : BaseModel
{
    [DataMember]
    public string BankExternalId { get; set; }
    
    [DataMember]
    public string Name { get; set; }
    
    [DataMember]
    public string Region { get; set; }
}