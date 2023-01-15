using System.Runtime.Serialization;

namespace Bank.Service.DataModels;

public class CardTransactions : BaseModel
{
    [DataMember]
    public CardModel Card { get; set; }
    
    [DataMember]
    public List<TransactionModel> Transactions { get; set; }
}