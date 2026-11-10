using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

public interface IAppDbContext
{
    DbSet<CategoryEntity> Categories { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
