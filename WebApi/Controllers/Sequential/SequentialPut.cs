using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using WebApi.Database;
using WebApi.TorchUtilities.Layers;

namespace WebApi.Controllers.Sequential;

public partial class SequentialController
{
    /// <returns>更新<see cref="SequentialRecord"/>中指定层的<b>全部</b>参数、包括默认参数</returns>
    [HttpPut("Sequential/Update/Name")]
    public async Task<bool> SequentialUpdateName(string oldName, string newName, string remark = "")
    {
        if (await FindSequential(oldName) is not { } sequential)
            return false;
        if (oldName != newName)
        {
            _ = _dbContext.Remove(sequential);
            _ = await _dbContext.SaveChangesAsync();
            sequential.Name = newName;
            _ = _dbContext.Add(sequential);
        }
        if (sequential.Remark != remark)
        {
            sequential.Remark = remark;
            _ = _dbContext.Update(sequential);
            _ = await _dbContext.SaveChangesAsync();
        }
        return true;
    }

    /// <returns><see cref="Sequential"/>中指定层的<b>全部</b>参数、包括默认参数</returns>
    [HttpPut("Layers/Update")]
    public async Task<string?> LayersUpdate(string name, int index, string type, [FromBody] JsonObject layer)
    {
        if (type switch
            {
                nameof(Conv2d) => Conv2d.Deserialize(layer).ToSqlJson(),
                nameof(AvgPool2d) => AvgPool2d.Deserialize(layer).ToSqlJson(),
                nameof(BatchNorm2d) => BatchNorm2d.Deserialize(layer).ToSqlJson(),
                nameof(Linear) => Linear.Deserialize(layer).ToSqlJson(),
                nameof(Flatten) => Flatten.Deserialize(layer).ToSqlJson(),
                nameof(ReLU) => ReLU.Deserialize(layer).ToSqlJson(),
                _ => null
            } is { } ja)
            return null;

        return null;
    }

}
