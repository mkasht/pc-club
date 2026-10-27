using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PcClub.Models;

namespace PcClub.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider provider)
    {
        var db = provider.GetRequiredService<ClubDbContext>();
        await db.Database.MigrateAsync();
        var roles = provider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "User" })
            if (!await roles.RoleExistsAsync(role))
            {
                var result = await roles.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                    throw new InvalidOperationException("Не удалось создать роли.");
            }
        if (await db.Categories.AnyAsync())
            return;
        var arena = new ServiceCategory
        {
            Name = "Общий зал",
            SortOrder = 1
        };
        var privateZone = new ServiceCategory
        {
            Name = "Приватные комнаты",
            SortOrder = 2
        };
        var console = new ServiceCategory
        {
            Name = "Консольная зона",
            SortOrder = 3
        };
        db.Categories.AddRange(arena, privateZone, console);
        db.Services.AddRange(
            new ClubService
            {
                Name = "ARENA",
                Category = arena,
                SortOrder = 1,
                Price = 150,
                Summary = "Твой следующий уровень. Место, где начинается игра.",
                Description = "Открытое игровое пространство для соло-сессий и встреч с друзьями. Удобное рабочее место, большой монитор и всё необходимое, чтобы сосредоточиться на игре. Концепция оснащения и стоимость приведены для демонстрации сайта.",
                Specs = "1 игровое место • 27″ монитор • Механическая клавиатура",
                ImagePath = "/images/arena.webp"
            },

            new ClubService
            {
                Name = "ARENA NIGHT",
                Category = arena,
                SortOrder = 4,
                Price = 650,
                PriceUnit = "ночной пакет",
                Summary = "Больше времени на любимые игры.",
                Description = "Ночной пакет для продолжительной игровой сессии в общем зале. Время начала и продолжительность пакета необходимо уточнить перед посещением. Это демонстрационный тариф учебного проекта.",
                Specs = "1 игровое место • Ночной пакет • Общий зал",
                ImagePath = "/images/arena-night.webp"
            },

            new ClubService
            {
                Name = "PRIVATE",
                Category = privateZone,
                SortOrder = 2,
                Price = 1200,
                Summary = "Своя команда. Свои правила. Отдельное пространство.",
                Description = "Приватная комната для командной игры и спокойных встреч. Общий стол, игровые места и зона отдыха помогают провести время вместе. Вместимость, оборудование и стоимость показаны как пример для первой версии.",
                Specs = "Командная комната • 6 игровых мест • Зона отдыха",
                ImagePath = "/images/private.jpg"
            },

            new ClubService
            {
                Name = "LOUNGE",
                Category = console,
                SortOrder = 3,
                Price = 400,
                Summary = "Большой экран. Удобный диван. Игра без спешки.",
                Description = "Отдельная атмосфера для консольных игр: большой экран, мягкий диван и место для друзей. Подходит для дружеских матчей и спокойного вечера. Конкретные игры и доступность нужно будет подтвердить у клуба.",
                Specs = "Большой экран • 2 контроллера • Мягкий диван",
                ImagePath = "/images/lounge.jpg"
            }
        );
        await db.SaveChangesAsync();
    }
}
