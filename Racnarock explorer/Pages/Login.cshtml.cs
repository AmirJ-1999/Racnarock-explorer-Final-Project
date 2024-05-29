using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Racnarock_explorer.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        [Required]
        public string Username { get; set; }

        [BindProperty]
        [Required]
        public string Password { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(string.Empty, "Both username and password are required.");
                return Page();
            }

            // Example credentials, should be replaced with actual authentication logic
            if (Username != "admin")
            {
                ModelState.AddModelError(string.Empty, "Incorrect username or password. Please try again.");
                return Page();
            }

            if (Password != "password")
            {
                ModelState.AddModelError(string.Empty, "Incorrect username or password. Please try again.");
                return Page();
            }

            // If credentials are correct, set session and redirect
            HttpContext.Session.SetString("LoggedInUser", Username);
            return RedirectToPage("/Tours");
        }
    }
}