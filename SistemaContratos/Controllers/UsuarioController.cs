using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SistemaContratos.Data;
using SistemaContratos.Models;
using System.Security.Claims;

namespace SistemaContratos.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _banco;

        public UsuarioController(AppDbContext banco)
        {
            _banco = banco;
        }

        // Tela de login
        public IActionResult Login()
        {
            return View();
        }

        // Recebe os dados, valida e cria o cookie
        [HttpPost]
        public async Task<IActionResult> Login(string login, string senha)
        {
            // Verifica se existe um usuario com essas informações
            var usuario = _banco.Usuarios.FirstOrDefault(u => u.Login == login && u.Senha == senha);

            if (usuario == null)
            {
                TempData["Erro"] = "Usuário ou senha inválido.";
                return View();
            }

            // Cria o cookie do usuario, armazena no navegador do usuario e envia pra tela inicial do sistema
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.NameIdentifier, usuario.Login)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // Tela de cadastro
        public IActionResult Cadastrar()
        {
            return View();
        }

        // Recebe as informações inseridas, verifica se ja tem algum usuario com os dados e salva no banco de dados
        [HttpPost]
        public IActionResult Cadastrar(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                if (_banco.Usuarios.Any(u => u.Login == usuario.Login))
                {
                    TempData["Erro"] = "Email já em uso, coloque outro ou faça login.";
                    return View(usuario);
                }

                _banco.Usuarios.Add(usuario);
                _banco.SaveChanges();

                TempData["Sucesso"] = "Cadastro realizado com sucesso.";
                return RedirectToAction("Login");
            }

            return View(usuario);
        }

        // É o 
        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
