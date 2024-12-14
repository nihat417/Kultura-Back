using Kultura.Application.Model;
using Kultura.Application.Repository.Abstract;
using Kultura.Domain.Entities;
using Kultura.Persistence.Data;
using Microsoft.AspNetCore.Identity;

namespace Kultura.Application.Repository.Concrete
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenService _jwtTokenService;
        private readonly EmailConfiguration _emailConfiguration;

        private IAuthService? _authService;
        private IRestaurantService _restaurantService;
        private IUserService? _userService;
        private IEmailService? _emailService;
        /*private IAccountService? _accountService;*/


        public UnitOfWork(AppDbContext context,JwtTokenService jwtTokenService,EmailConfiguration emailConfiguration)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _emailConfiguration = emailConfiguration;
        }   

        public IAuthService AuthService => _authService ??= new AuthService(_context ,_jwtTokenService);
        public IRestaurantService RestaurantService => _restaurantService ??= new RestaurantService(_context , _jwtTokenService);
        public IUserService UserService => _userService ??= new UserService(_context);
        public IEmailService EmailService => _emailService ??= new EmailService(_emailConfiguration);
        public IJwtTokenService JwtTokenService => _jwtTokenService;
        /*public IAccountService AccountService => _accountService ??= new AccountService(_context, _userManager);*/

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
        
    }
}
