using Microsoft.AspNetCore.Mvc;
using seguridad.Models;
using seguridad.Data;
using System.Diagnostics;
using Microsoft.Extensions.Primitives;

namespace seguridad.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            var captchaResponse = Request.Form["g-recaptcha-response"];
            var isCaptchaValid = await VerifyCaptchaAsync(captchaResponse);

            if (!isCaptchaValid)
            {
                ViewBag.ErrorMessage = "Por favor, verifica que no eres un robot.";
                return View("Index");
            }

            var usuario = _context.Usuarios_Seguridad.FirstOrDefault(u => u.Email == Email);
            if (usuario != null && PasswordHelper.VerifyPassword(Password, usuario.PasswordHash, usuario.PasswordSalt))
            {
                HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
                return RedirectToAction("Bienvenido");
            }
            ViewBag.ErrorMessage = "Usuario o contraseña incorrectos.";
            return View("Index");
        }

        // Nueva acción para mostrar la bienvenida
        public IActionResult Bienvenido()
        {
            var email = HttpContext.Session.GetString("UsuarioEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index");
            }
            ViewBag.Email = email;
            return View("Bienvenidos");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string Email, string Password)
        {
            var captchaResponse = Request.Form["g-recaptcha-response"];
            var isCaptchaValid = await VerifyCaptchaAsync(captchaResponse);

            if (!isCaptchaValid)
            {
                ViewBag.Message = "Por favor, verifica que no eres un robot.";
                return View();
            }

            if (_context.Usuarios_Seguridad.Any(u => u.Email == Email))
            {
                ViewBag.Message = "El usuario ya existe.";
                return View();
            }

            PasswordHelper.CreatePasswordHash(Password, out byte[] hash, out byte[] salt);

            var usuario = new UsuarioSeguridad
            {
                Email = Email,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _context.Usuarios_Seguridad.Add(usuario);
            _context.SaveChanges();

            // Auditoría: después de guardar el usuario
            var cambios = new Dictionary<string, (string, string)>
            {
                { "Email", (null, Email) },
                { "PasswordHash", (null, Convert.ToBase64String(hash)) }
            };
            var auditoria = new AuditoriaHelper(_context);
            auditoria.Registrar(Email, "INSERT", "Usuarios_Seguridad", cambios);

            ViewBag.Message = "Usuario registrado correctamente.";
            return View();
        }

        private async Task<bool> VerifyCaptchaAsync(string captchaResponse)
        {
            var secret = "6LfzMswrAAAAADFsgmb4KiveE1fJNOUcrMQhtEfB"; //  clave secreta de Google reCAPTCHA
            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", secret),
                new KeyValuePair<string, string>("response", captchaResponse)
            });
            var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
            var json = await response.Content.ReadAsStringAsync();
            return json.Contains("\"success\": true");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string Email)
        {
            var usuario = _context.Usuarios_Seguridad.FirstOrDefault(u => u.Email == Email);
            if (usuario != null)
            {
                // Genera un enlace de recuperación (puedes usar un token único)
                var recoveryLink = Url.Action("ResetPassword", "Home", new { email = Email }, Request.Scheme);
                var emailHelper = new EmailHelper(this.HttpContext.RequestServices.GetService<IConfiguration>());
                emailHelper.SendPasswordRecoveryEmail(Email, recoveryLink);
                ViewBag.Message = "Se ha enviado un correo de recuperación.";
            }
            else
            {
                ViewBag.Message = "Si el correo existe, se enviará un enlace para restablecer la contraseña.";
            }
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            // Puedes mostrar el email en la vista y pedir la nueva contraseña
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email, string newPassword)
        {
            var usuario = _context.Usuarios_Seguridad.FirstOrDefault(u => u.Email == email);
            if (usuario != null)
            {
                PasswordHelper.CreatePasswordHash(newPassword, out byte[] hash, out byte[] salt);
                usuario.PasswordHash = hash;
                usuario.PasswordSalt = salt;
                _context.SaveChanges();
                ViewBag.Message = "Contraseña restablecida correctamente.";
            }
            else
            {
                ViewBag.Message = "Usuario no encontrado.";
            }
            ViewBag.Email = email;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Edit(int id, string nuevoEmail)
        {
            var usuario = _context.Usuarios_Seguridad.Find(id);
            if (usuario == null)
            {
                ViewBag.Message = "Usuario no encontrado.";
                return View();
            }

            var emailAnterior = usuario.Email;
            usuario.Email = nuevoEmail;
            _context.SaveChanges();

            // Auditoría
            var cambios = new Dictionary<string, (string, string)>
            {
                { "Email", (emailAnterior, nuevoEmail) }
            };
            var auditoria = new AuditoriaHelper(_context);
            auditoria.Registrar(nuevoEmail, "UPDATE", "Usuarios_Seguridad", cambios);

            ViewBag.Message = "Usuario actualizado correctamente.";
            return View();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var usuario = _context.Usuarios_Seguridad.Find(id);
            if (usuario == null)
            {
                ViewBag.Message = "Usuario no encontrado.";
                return View();
            }

            var email = usuario.Email;
            _context.Usuarios_Seguridad.Remove(usuario);
            _context.SaveChanges();

            // Auditoría
            var cambios = new Dictionary<string, (string, string)>
            {
                { "Email", (usuario.Email, null) }
            };
            var auditoria = new AuditoriaHelper(_context);
            auditoria.Registrar(usuario.Email, "DELETE", "Usuarios_Seguridad", cambios);

            ViewBag.Message = "Usuario eliminado correctamente.";
            return View();
        }
    }
}
