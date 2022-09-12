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

        return await SaveSequential(1);
    }
}
