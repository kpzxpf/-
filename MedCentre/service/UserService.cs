using MedCentre.db;
using MedCentre.models;
using Microsoft.EntityFrameworkCore;

namespace MedCentre.service;

public class UserService
    {
        ApplicationDbContext _context = new();

        public async Task<(bool Success, User User, string Message)> AuthenticateAsync(string login, string password)
        {
            User? user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == login)
                .ConfigureAwait(false);

            if (user == null)
            {
                return (false, null, "Пользователь не найден");
            }
            
            if (user.Password != password)
            {
                return (false, null, "Неверный пароль");
            }

            return (true, user, "Успешная авторизация");
        }

        public async Task<(bool Success, User User, string Message)> RegisterAsync(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Login == user.Login))
            {
                return (false, null, "Пользователь с таким логином уже существует");
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return (false, null, "Пользователь с таким email уже существует");
            }
            
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            
            user.RoleId = 1;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, user, "Регистрация успешна");
        }
    }