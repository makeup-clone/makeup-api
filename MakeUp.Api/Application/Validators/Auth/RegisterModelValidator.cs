using Application.Models;
using FluentValidation;

namespace Application.Validators.Auth;

public class RegisterModelValidator : AbstractValidator<RegisterModel>
{
    public RegisterModelValidator()
    {
        //Деякі параметри закоментовані спеціально, на момент розробки, для полекшення тестування додатку. Пізніше вони будуть синхронізовані із налаштуваннями Identity та взяті на озброєння
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email є обов'язковим полем.")
            .EmailAddress().WithMessage("Введіть коректну електронну адресу.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль є обов'язковим полем.")
            .MinimumLength(6).WithMessage("Пароль має містити щонайменше 6 символів.");
            //.Matches("[A-Z]").WithMessage("Пароль повинен містити хоча б одну велику латинську літеру.")
            //.Matches("[0-9]").WithMessage("Пароль повинен містити хоча б одну цифру.");

        // Якщо в моделі є імена (як у твоєму проєкті раніше)
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Ім'я не повинно перевищувати 100 символів.");

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Прізвище не повинно перевищувати 100 символів.");
    }
}
