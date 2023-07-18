using System.ComponentModel.DataAnnotations;

namespace Notes.Identity.Models
{
    //регистрация пользователя
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)] //скрывает пароль при вводе
        [Compare("Password")] //должно соответствовать полю пароль
        public string ConfirmPassword { get; set; }
        public string ReturnUrl { get; set; }
    }
}