using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DevTool01.Data;
using DevTool01.Models;
using System.Diagnostics;
// C:\Users\STUDENT\AppData\Local\User Name\com.companyname.devtool01\Data\
namespace DevTool01
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddDbContext<DataContext>(
                options =>
                {
                    var dbPath = Path.Combine(FileSystem.AppDataDirectory, "devtool01.db3");
                    options.UseSqlite($"Data Source={dbPath}");
                }
                );
#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();

            using (var scope = app.Services.CreateScope()) {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Database.EnsureCreated();

                if (!context.Convos.Any()) {
                    context.Convos.AddRange(
                        new Convo { Location = "EastPort", CharNames = "DJ", Remark = "first" },
                        new Convo { Location = "EastPort", CharNames = "Katta", Remark = "first" }

                    );
                    context.SaveChanges();
                }
                if (!context.Characters.Any()) {
                    context.Characters.AddRange(
                        new obsCharacter { CharName = "DJ", CharSelected = false },
                        new obsCharacter { CharName = "Katta", CharSelected = false },
                        new obsCharacter { CharName = "CJ", CharSelected = false },
                        new obsCharacter { CharName = "Lysander", CharSelected = false }
                        );
                    context.SaveChanges();
                }
                if (!context.Locations.Any()) {
                    context.Locations.AddRange(
                        new obsLocation { LocName = "EastPort", LocSelected = false },
                        new obsLocation { LocName = "WestVale", LocSelected = false },
                        new obsLocation { LocName = "NorthHaven", LocSelected = false },
                        new obsLocation { LocName = "SouthRidge", LocSelected = false },
                        new obsLocation { LocName = "CentralCity", LocSelected = false },
                        new obsLocation { LocName = "OldTown", LocSelected = false }
                        );
                    context.SaveChanges();
                }
            }
            return app;
        }
        public static void AddToDb<T>(DataContext context, T item) where T : class
        {
            context.Set<T>().Add(item);
            context.SaveChanges();
        }
    }
}
