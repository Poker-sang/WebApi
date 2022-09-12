using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using WebApi.Database;

namespace WebApi.Controllers.Sequential;

public partial class SequentialController
{
    /// <summary>
    /// 数据库新建<see cref="SequentialRecord"/>
    /// </summary>
    /// <returns>新建操作是否成功</returns>
    [HttpPut("Sequential/Create")]
    public async Task<bool> SequentialCreate(string name, string remark = "")
    {
        if (await ContainsSequential(name))
            return false;

        _ = await _dbContext.AddAsync(new SequentialRecord
        {
            Name = name,
            Remark = remark,
            CreateTime = DateTime.Now
        });

        return await SaveSequential(1);
    }

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
    public async Task<string?> LayersUpdate(string name, int index, [FromBody] JsonObject jo)
    {

        return null;
    }

}
