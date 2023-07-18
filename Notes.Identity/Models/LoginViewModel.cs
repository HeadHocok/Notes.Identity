using System.ComponentModel.DataAnnotations;

namespace Notes.Identity.Models
{
    //Логин пользователя
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)] //чтобы пароль не отображался
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}