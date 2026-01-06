using System.Text.Json;

using Avemepls.Core.Extensions;

namespace Avemepls.Mvc.Json;

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        return name.ToSnakeCase();
    }
}