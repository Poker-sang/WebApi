using Microsoft.AspNetCore.Mvc;
using WebApi.Database;

namespace WebApi.Controllers.Sequential;

public partial class SequentialController
{
    /// <summary>
    /// 数据库删除<see cref="SequentialRecord"/>
    /// </summary>
    /// <returns>删除操作是否成功</returns>
    [HttpDelete("Sequential/Delete")]
    public async Task<bool> SequentialDelete(string name)
    {
        if ((await FindSequential(name)) is not { } sequential)
            return false;

        _ = _dbContext.Remove(sequential);

        return await SaveSequential();
    }

    /// <summary>
    /// 数据库中指定<paramref name="name"/>的<see cref="SequentialRecord"/>删除指定层
    /// </summary>
    /// <returns>删除操作是否成功</returns>
    [HttpDelete("Layers/Delete")]
    public async Task<bool> SequentialDelete(string name,int index)
    {
        if ((await FindSequential(name)) is not { } sequential)
            return false;

        //_ = _dbContext.Remove(sequential);

        return await SaveSequential();
    }
}
