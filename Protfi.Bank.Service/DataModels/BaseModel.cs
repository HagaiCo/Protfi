using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Bank.Service.DataModels;

public class BaseModel
{
    [DataMember]
    [Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [DataMember]
    [Column(Order = 1)]
    public Guid UserId { get; set; }
    
    [DataMember]
    public long CreationTimeUtcAsUnix { get; set; }
}