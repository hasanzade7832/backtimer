using backtimetracker.Models.Auth;
using backtimetracker.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backtimetracker.Controllers.Account;

[AllowAnonymous]
[Route("Register")]
[ApiController]
public class RegisterController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("ورودی‌های فرم معتبر نیستند.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = DateTime.Now
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var firstError = result.Errors.FirstOrDefault();
            var message = firstError is not null ? TranslateIdentityError(firstError) : "خطای ناشناخته هنگام ثبت‌نام.";
            return BadRequest(message);
        }

        return Ok("ثبت‌نام با موفقیت انجام شد");
    }


    private string TranslateIdentityError(IdentityError error)
    {
        return error.Code switch
        {
            "DuplicateUserName" => "این نام کاربری قبلاً ثبت شده است.",
            "DuplicateEmail" => "این ایمیل قبلاً استفاده شده است.",
            "PasswordTooShort" => "رمز عبور خیلی کوتاه است.",
            "PasswordRequiresDigit" => "رمز عبور باید حداقل شامل یک عدد باشد.",
            "PasswordRequiresUpper" => "رمز عبور باید شامل حداقل یک حرف بزرگ باشد.",
            "PasswordRequiresLower" => "رمز عبور باید شامل حداقل یک حرف کوچک باشد.",
            "PasswordRequiresNonAlphanumeric" => "رمز عبور باید شامل حداقل یک کاراکتر خاص باشد.",
            _ => "خطا: " + error.Description
        };
    }

}
