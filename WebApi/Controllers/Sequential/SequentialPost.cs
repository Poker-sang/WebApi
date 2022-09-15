using Microsoft.AspNetCore.Mvc;
using WebApi.Database;
using WebApi.TorchUtilities.Services;

namespace WebApi.Controllers.Sequential;

public partial class SequentialController
{
    /// <summary>
    /// 数据库新建<see cref="SequentialRecord"/>
    /// </summary>
    /// <returns>新建操作是否成功</returns>
    [HttpPost("Sequential/Create")]
    public async Task<bool> SequentialCreate(string name, string remark = "")
    {
        if (name.GetLayerType() is ModuleUtility.PresetType.Sequential && await ContainsSequential(name))
            return false;

        _ = await _dbContext.AddAsync(new SequentialRecord
        {
            Name = name,
            Remark = remark,
            CreateTime = DateTime.Now
        });

        return await SaveSequential();
    }

    /// <summary>
    /// 数据库中指定<paramref name="name"/>的<see cref="SequentialRecord"/>新建一个层
    /// </summary>
    /// <returns>新建操作是否成功</returns>
    [HttpPost("Layers/Create")]
    public async Task<bool> LayersCreate(string name, string type)
    {

        return false;
    }
}
