using System.ComponentModel.DataAnnotations;

namespace PcClub.Models;

public class RegisterForm
{
    [Required(ErrorMessage = "Укажите email"), EmailAddress(ErrorMessage = "Проверьте адрес email"), StringLength(254)]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Укажите пароль"),
    MinLength(10,
        ErrorMessage = "Минимум 10 символов"),
    StringLength(128),
    DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = "";
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают"), DataType(DataType.Password)]
    [Display(Name = "Повторите пароль")]
    public string ConfirmPassword { get; set; } = "";
}

public class LoginForm
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), StringLength(128)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = "";
    public string? ReturnUrl
    {
        get; set;
    }
}
