using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace WebApi.Controllers.Sequential;

public partial class SequentialController
{
    /// <returns><see cref="Sequential"/>中指定层的<b>全部</b>参数、包括默认参数</returns>
    [HttpPut("Layers/Update")]
    public async Task<string?> LayersUpdate(string name, int index, [FromBody] JsonObject jo)
    {

        return null;
    }

}
