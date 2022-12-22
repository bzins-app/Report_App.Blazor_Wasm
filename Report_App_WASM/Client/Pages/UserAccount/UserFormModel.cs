using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Report_App_WASM.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Report_App_WASM.Client.Pages.UserAccount
{
#nullable enable
    public class UserFormModel
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public IdentityOptions? Options { get; set; }
    }

    public class UserFormModelValidator : AbstractValidator<UserFormModel>
    {
        public UserFormModelValidator(CommonLocalizationService Localizer)
        {
            RuleFor(v => v.UserName)
                 .MaximumLength(256)
                 .NotEmpty();
            RuleFor(v => v.Email)
                 .MaximumLength(256)
                 .NotEmpty()
                 .EmailAddress();

            RuleFor(p => p.Password).NotEmpty().WithMessage(Localizer.Get("Your password cannot be empty"))
                     .MinimumLength(6).WithMessage(Localizer.Get("Your password length must be at least 6"))
                     .Matches(@"[A-Z]+").When(v => v.Options.Password.RequireUppercase).WithMessage(Localizer.Get("Your password must contain at least one uppercase letter"))
                     .Matches(@"[a-z]+").When(v => v.Options.Password.RequireLowercase).WithMessage(Localizer.Get("Your password must contain at least one lowercase letter"))
                     .Matches(@"[0-9]+").When(v => v.Options.Password.RequireDigit).WithMessage(Localizer.Get("Your password must contain at least one number"))
                     .Matches(@"[\!\?\*\.]+").When(v => v.Options.Password.RequireNonAlphanumeric).WithMessage(Localizer.Get("Your password must contain at least one (!? *.)"));
            RuleFor(x => x.ConfirmPassword)
                 .Equal(x => x.Password).WithMessage(Localizer.Get("The passwords entered must be equal"));
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<UserFormModel>.CreateWithOptions((UserFormModel)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}
