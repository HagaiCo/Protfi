using System.Text.Json.Nodes;

namespace Protfi.Common;

public interface IServiceBase
{
    public void StatusVerification(JsonNode? status);
}