using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SystemMonitor
{
    public static class LogWrapper
    {
        public static void Execute(Action action, string module, [CallerMemberName] string function = "")
        {
            SQLiteHelper.WriteLog(module, function, "Jarayon boshlandi");
            try
            {
                action();
                SQLiteHelper.WriteLog(module, function, "Jarayon muvaffaqiyatli yakunlandi");
            }
            catch (Exception ex)
            {
                SQLiteHelper.WriteLog($"Xatolik yuz berdi: {ex.Message}", module, function);
                throw; 
            }
        }

        public static async Task ExecuteAsync(Func<Task> action, string module, [CallerMemberName] string function = "")
        {
            SQLiteHelper.WriteLog(module, function, "Jarayon boshlandi");
            try
            {
                await action();
                SQLiteHelper.WriteLog( module, function, "Jarayon muvaffaqiyatli yakunlandi");
            }
            catch (Exception ex)
            {
                SQLiteHelper.WriteLog($"Xatolik yuz berdi: {ex.Message}", module, function);
                throw; // Xatoni qayta tashlash
            }
        }

        public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string module, [CallerMemberName] string function = "")
        {
            SQLiteHelper.WriteLog(module, function, "Jarayon boshlandi");
            try
            {
                var result = await action();
                SQLiteHelper.WriteLog(module, function, "Jarayon muvaffaqiyatli yakunlandi");
                return result;
            }
            catch (Exception ex)
            {
                SQLiteHelper.WriteLog($"Xatolik yuz berdi: {ex.Message}", module, function);
                throw;
            }
        }
    }
}