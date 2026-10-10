using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.System;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.API.Extensions;

public static class ApplicationBuilderExtensions
{
    private const string AdminSeededFlag = "admin_seeded";

    public static async Task SeedDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        
        var systemFlagRepository = scope.ServiceProvider.GetRequiredService<ISystemFlagRepository>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Проверяем, был ли уже выполнен seed администратора
        var flagExists = await systemFlagRepository.ExistsByNameAsync(AdminSeededFlag);

        if (flagExists)
        {
            Console.WriteLine("ℹ️ Администратор уже был создан ранее. Seed пропущен.");
            return;
        }

        // Получаем роль администратора
        var adminRole = await roleRepository.GetByNameAsync(RoleName.Administrator);
        if (adminRole == null)
        {
            throw new InvalidOperationException("Роль Administrator не найдена. Убедитесь, что миграции применены.");
        }

        // Хешируем пароль и создаём админа
        var passwordHash = passwordHasher.Hash("admin");

        var adminUser = new User(
            Guid.NewGuid(),
            new Login("admin"),
            passwordHash,
            new LastName("Администратор"),
            new FirstName("Системный"),
            null
        );

        adminUser.AssignRole(adminRole.Id);

        await userRepository.AddAsync(adminUser);

        // Устанавливаем флаг, что seed выполнен
        var seedFlag = new SystemFlag(Guid.NewGuid(), AdminSeededFlag, "true");
        await systemFlagRepository.AddAsync(seedFlag);

        await unitOfWork.SaveChangesAsync();
        
        Console.WriteLine("Администратор (login: admin, password: admin) успешно создан.");
    }
}