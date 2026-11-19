using Application.Models;
using FluentValidation;

namespace Application.Validators.Auth;

public class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email є обов'язковим полем.")
            .EmailAddress().WithMessage("Введіть коректну електронну адресу.")
            .MaximumLength(256).WithMessage("Email не повинен перевищувати 256 символів.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль є обов'язковим полем.")
            .MinimumLength(6).WithMessage("Пароль має містити щонайменше 6 символів.")
            .MaximumLength(100).WithMessage("Пароль занадто довгий.");
    }
}
