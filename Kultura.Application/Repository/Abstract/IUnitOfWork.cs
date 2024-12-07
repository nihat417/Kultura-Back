using Kultura.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Kultura.Application.Repository.Abstract
{
    public interface IUnitOfWork : IDisposable
    {
        IAuthService AuthService { get; }
        IRestaurantService RestaurantService { get; }
        IEmailService EmailService { get; }
        IJwtTokenService JwtTokenService { get; }
            
        /*IAccountService AccountService { get; }*/
        UserManager<User> UserManager { get; }
        RoleManager<IdentityRole> RoleManager { get; }
        Task<int> SaveChangesAsync();
    }
}
