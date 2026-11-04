using Microsoft.AspNetCore.Mvc;
using RestaurantApp;
using RestaurantSharedLib;
using RestaurantSharedLib.Models;

namespace RestoranApi.Controllers
{ 
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto loginRequest)
        {
            
            using var dbContext = new DBContext();
            var accountRepository = new AccountRepository(dbContext);

            var user = accountRepository.GetAccountByUsername(loginRequest.Username);

            Console.WriteLine(
                user != null
        ?$"Kullanıcı bulundu: {user.Username}"
        : "Kullanıcı bulunamadı.");

            if (user != null && user.Password == loginRequest.Password)
            {
                string tokenGuid = Guid.NewGuid().ToString();   
                var token = tokenGuid + user.Username;
                return Ok(new { success = true, 
                    token = token, 
                    message = "Giriş başarılı" , 
                    role = user.Role, 
                    userId = user.Id, 
                    userFName = user.Name, 
                    userLName = user.Surname,
                    isAuth = user.IsAuthorized,
                });
            }

            return Unauthorized(new { success = false, message = "Geçersiz kullanıcı adı veya şifre." });
        }


        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest registerRequest)
        {
            using var dbContext = new DBContext();
            var accountRepository = new AccountRepository(dbContext);

            //token tekrar oluşturuluyor kayıt sırasında
            string tokenGuid = Guid.NewGuid().ToString();
            var token = tokenGuid ;

            // Check if the username already exists
            var existingUser = accountRepository.GetAccountByUsername(registerRequest.Username);
            if (existingUser != null)
            {
                return BadRequest(new { success = false, message = "Kullanıcı adı zaten mevcut.",token });
            }

           
            var newAccount = new Account
            {
                Username = registerRequest.Username,
                //Password = Utils.CreateMD5(registerRequest.Password), 
                Password = registerRequest.Password,
                Mail = registerRequest.Email,
                IsAuthorized = false,
                Role = "Garson"
            };

            accountRepository.RegisterAccount(newAccount);

            return Ok(new { success = true, message = "Kayıt başarılı." });
        }

    }

}
              
