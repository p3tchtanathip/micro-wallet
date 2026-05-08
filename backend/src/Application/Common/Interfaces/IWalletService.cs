using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IWalletService
{
    Task<string> GenerateUniqueWalletNumberAsync();
    Task CreateDefaultWalletAsync(User user);
}
