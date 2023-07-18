using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Notes.Identity.Models;

namespace Notes.Identity.Controllers
{
    //Возвращает view форму логина и пароля
    public class AuthController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager; //Реализация входа пользователя
        private readonly UserManager<AppUser> _userManager; //Поиск
        private readonly IIdentityServerInteractionService _interactionService; //Логаут

        public AuthController(SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IIdentityServerInteractionService interactionService) =>
            (_signInManager, _userManager, _interactionService) =
            (signInManager, userManager, interactionService); //реализация вышеперечисленного с помощью конструктора

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            var viewModel = new LoginViewModel //передаем view логин пользователю
            {
                ReturnUrl = returnUrl
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid) //проверяем модель на валидность данных
            {
                return View(viewModel); //перенаправляет туда, откуда пришел запрос
            }

            var user = await _userManager.FindByNameAsync(viewModel.Username); //ищем пользователя по имени пользователя
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found");
                return View(viewModel);
            }

            var result = await _signInManager.PasswordSignInAsync(viewModel.Username,
                viewModel.Password, false, false); //принимает имя, пароль, и параметры ispersistant и lockoutorfailor (сохранение куки после закрытия, блок после неудачных попыток)
            if (result.Succeeded)
            {
                return Redirect(viewModel.ReturnUrl);
            }
            ModelState.AddModelError(string.Empty, "Login error");
            return View(viewModel);
        }

        [HttpGet] //перед заполнением формы
        public IActionResult Register(string returnUrl) 
        {
            var viewModel = new RegisterViewModel 
            {
                ReturnUrl = returnUrl
            };
            return View(viewModel);
        }

        [HttpPost] //после заполнения формы
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid) //проверяем модель на валидность данных
            {
                return View(viewModel); //создаем пользователя с помощью usermanager
            }

            var user = new AppUser //вход с помощью singin менеджера
            {
                UserName = viewModel.Username
            };

            var result = await _userManager.CreateAsync(user, viewModel.Password); 
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false); 
                return Redirect(viewModel.ReturnUrl); //перенаправление
            }
            ModelState.AddModelError(string.Empty, "Error occurred");
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await _signInManager.SignOutAsync();
            var logoutRequest = await _interactionService.GetLogoutContextAsync(logoutId);
            return Redirect(logoutRequest.PostLogoutRedirectUri);
        }
    }
}