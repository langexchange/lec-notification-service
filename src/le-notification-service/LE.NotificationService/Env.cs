using System;

namespace LE.NotificationService
{
    public static class Env
    {
        public readonly static string DB_CONNECTION_STRING = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        public readonly static string SECRET_KEY = Environment.GetEnvironmentVariable("SECRET_KEY") ?? string.Empty;
    }
}
