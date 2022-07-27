using LightenceClient.Models;

namespace LightenceClient
{
    static public class JWToken
    {
        static public string Token;
    }
    static public class Constants
    {
        //tymczasowe, wymienić na własciwy adres
        static public readonly string _serverAddress = "https://localhost:44397";
        public static User currentUser = new User();
        public static string configFileName = "lightence.config";
    }
    static public class Loggers
    {
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static readonly NLog.Logger Comm_logger = NLog.LogManager.GetLogger("Comm_logfile");
        public static readonly NLog.Logger Chat_logger = NLog.LogManager.GetLogger("Chat_logfile");
        public static readonly NLog.Logger Audio_logger = NLog.LogManager.GetLogger("Audio_logfile");
        public static readonly NLog.Logger File_logger = NLog.LogManager.GetLogger("Files_logfile");
    }
}
