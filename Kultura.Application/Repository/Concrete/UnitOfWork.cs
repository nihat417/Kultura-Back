using Kultura.Application.Model;
using Kultura.Application.Repository.Abstract;
using Kultura.Application.Services;
using Kultura.Domain.Entities;
using Kultura.Persistence.Data;
using Microsoft.AspNetCore.Identity;

namespace Kultura.Application.Repository.Concrete
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly EmailConfiguration _emailConfiguration;

        private IAuthService? _authService;
        private IRestaurantService _restaurantService;
        private IEmailService? _emailService;
        /*private IAccountService? _accountService;*/


        public UnitOfWork(AppDbContext context,UserManager<User> userManager,RoleManager<IdentityRole> roleManager,
            JwtTokenService jwtTokenService,EmailConfiguration emailConfiguration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenService = jwtTokenService;
            _emailConfiguration = emailConfiguration;
        }

        public IAuthService AuthService => _authService ??= new AuthService(_userManager, _roleManager, _jwtTokenService);
        public IRestaurantService RestaurantService => _restaurantService ??= new RestaurantService(_userManager,_context ,_roleManager, _jwtTokenService);
        public IEmailService EmailService => _emailService ??= new EmailService(_emailConfiguration);
        /*public IAccountService AccountService => _accountService ??= new AccountService(_context, _userManager);*/
        public UserManager<User> UserManager => _userManager;
        public RoleManager<IdentityRole> RoleManager => _roleManager;

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
        
    }
}
