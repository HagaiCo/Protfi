using System.Text.Json.Nodes;

namespace Protfi.Common;

public class ServiceBase
{
    public void StatusVerification(JsonNode? status)
    {
        if (status?.ToString() != "OK")
            throw new Exception("return status is not ok");
    }
}