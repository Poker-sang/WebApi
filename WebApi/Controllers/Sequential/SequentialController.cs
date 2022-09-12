using Microsoft.AspNetCore.Mvc;
using WebApi.Database;

namespace WebApi.Controllers.Sequential;

[ApiController]
[Route("api/[controller]")]
public partial class SequentialController : ControllerBase
{
    private readonly CnnDatabaseContext _dbContext;

    public SequentialController(CnnDatabaseContext context) => _dbContext = context;

    private ValueTask<SequentialRecord?> FindSequential(string sequentialName) =>
        _dbContext.SequentialRecord.FindAsync(sequentialName);

    private async ValueTask<bool> ContainsSequential(string sequentialName) =>
        (await FindSequential(sequentialName)) is { };

    private async ValueTask<bool> SaveSequential(int check = -1)
    {
        int result;
        try
        {
            result = await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        if (check < 0)
            return true;

        return check == result;
    }
}
