using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SystemMonitor.Models;

namespace SystemMonitor
{
    public class ApiClient
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private const string BaseUrl = "http://13.51.199.15:4000/computers/create";

        public static async Task<string> GetJwtTokenFromApi()
        {
            var computerInfo = ComputerInfo.GetComputerInfo(); 
            var jsonContent = JsonConvert.SerializeObject(computerInfo); 
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json"); 

            try
            {
                var response = await client.PostAsync(BaseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();
                    return token;
                    Console.WriteLine("");

                }
                else
                {
                    Console.WriteLine($"JWT olishda xatolik: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API ga so'rov yuborishda xatolik: {ex.Message}");
                return null;
            }
        }

        /*public static async Task<string> GetJwtTokenFromApi()
        {

            var response = await client.PostAsync("http://13.51.199.15:4000/computers/create/authenticate", null);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                return token;
            }

            Console.WriteLine("JWT token olishda xatolik");
            return null;
        }*/


        private static async Task<bool> SendData<T>(string url, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // JWT tokenni olish va headerga qo'shish
                var token = await GetJwtTokenFromApi();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new 
                        System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return true; // So'rov muvaffaqiyatli bo'ldi
                }

                Console.WriteLine($"[Xatolik]: {response.StatusCode} - {response.ReasonPhrase}");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"[HTTP Xatolik]: {httpEx.Message}");
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("[HTTP Xatolik]: So‘rov vaqt chegarasidan oshib ketdi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Noma'lum xatolik]: {ex.Message}");
            }

            return false; // Agar xatolik bo'lsa false qaytariladi
        }

            
       /* public static async Task<bool> SendComputerInfo(ComputerInfoDetails info)
        {
            return await SendData($"{BaseUrl}/computer-info", info);
        }*/

        public static async Task<bool> SendProgramInfo(List<ProgramDetails> programs)
        {
            return await SendData($"{BaseUrl}/program-info", programs);
        }

        
        public static async Task<bool> SendCommandResult(string command, string result, string error)
        {
            var response = new { command, result, error };
            return await SendData($"{BaseUrl}/command-result", response);
        }
    }
}
