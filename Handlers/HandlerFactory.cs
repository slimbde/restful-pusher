using Handlers.Interfaces;
using Handlers.MySql;
using Handlers.ScreenCap;

namespace Handlers
{
    public class HandlerFactory
    {
        public static IDataHandler CreateMySqlDataHandler() => new MySqlDataHandler();
        public static IDataHandler CreateScreenCapParser() => new ScreenCapMultipartParser();
    }
}
