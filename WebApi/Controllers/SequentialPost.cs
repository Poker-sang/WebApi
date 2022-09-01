using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace WebApi.Controllers;

public partial class SequentialController : ControllerBase
{
    /// <returns><see cref="Sequential"/>中指定层的<b>全部</b>参数、包括默认参数</returns>
    [HttpPut("Layers/Update")]
    public async Task<IEnumerable<JsonArray>?> LayersUpdate()
    {

    }

}
