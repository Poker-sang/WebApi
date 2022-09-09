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
}
