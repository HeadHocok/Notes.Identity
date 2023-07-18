namespace Notes.Identity.Data
{
    //инициализирует базу данных
    public class DbInitializer
    {
        public static void Initialize(AuthDbContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}