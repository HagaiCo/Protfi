using System.Runtime.Serialization;

namespace Bank.Service.DataModels;

public class AccountInformationModel : BaseModel
{
    [DataMember]
    public string BankId { get; set; }
    
    [DataMember]
    public string Number { get; set; }
}