using System.Text;
using Newtonsoft.Json;

namespace RestaFlowApi.Service
{
   
    public class AccountService
    {
        private readonly HttpClient _httpClient;

        public AccountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var loginData = new
            {
                Username = username,
                Password = password
            };

            var jsonContent = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7253/api/Account/login", content);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                return token;
            }

            return null; 
        }

        public async Task RegisterAsync(string username, string password, string email)
        {
            var registerData = new
            {
                Username = username,
                Password = password, 
                Email = email
            };

            var jsonContent = JsonConvert.SerializeObject(registerData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7253/api/account/register", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Kullanıcı kaydı başarıyla oluşturuldu.");
            }
            else
            {
                Console.WriteLine("Kayıt başarısız.");
            }
        }


    }

}
