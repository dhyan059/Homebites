using Homebites.Data;

namespace Homebites.Data
{
    public static class DbInitializer
    {
        public static void Initialize(HomebitesDbContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}