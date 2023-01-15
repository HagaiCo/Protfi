using System.Runtime.Serialization;

namespace Bank.Service.DataModels;

public class CardModel : BaseModel
{
    [DataMember]
    public string Number { get; set; }
    
    [DataMember]
    public string Type { get; set; }
    
    [DataMember]
    public string Currency { get; set; }
}