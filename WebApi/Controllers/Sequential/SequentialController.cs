using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using WebApi.Database;

namespace WebApi.Controllers.Sequential;

[ApiController]
[Route("api/[controller]")]
public partial class SequentialController : ControllerBase
{
    private readonly CnnDatabaseContext _dbContext;

    public SequentialController(CnnDatabaseContext context) => _dbContext = context;

    /// <summary>
    /// 按照名称寻找某项
    /// </summary>
    /// <param name="sequentialName"></param>
    /// <returns><see langword="null"/>未找到</returns>
    private ValueTask<SequentialRecord?> FindSequential(string sequentialName) =>
        _dbContext.SequentialRecord.FindAsync(sequentialName);

    /// <returns>是否包含某项</returns>
    private async ValueTask<bool> ContainsSequential(string sequentialName) =>
        (await FindSequential(sequentialName)) is { };

    /// <returns>是否包含某项</returns>
    private async ValueTask<JsonArray?> ParseContentJson(string sequentialName) =>
        JsonNode.Parse((await FindSequential(sequentialName))?.ContentJson ?? "")?.AsArray();

    /// <summary>
    /// 保存项目
    /// </summary>
    /// <returns>是否成功</returns>
    private async ValueTask<bool> SaveSequential()
    {
        try
        {
            _ = await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }
}
