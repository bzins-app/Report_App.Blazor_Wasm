namespace Report_App_WASM.Shared
{
    public class IdentityDefaultOptions
    {
        //password settings
        public bool PasswordRequireDigit { get; set; }
        public int PasswordRequiredLength { get; set; }
        public bool PasswordRequireNonAlphanumeric { get; set; }
        public bool PasswordRequireUppercase { get; set; }
        public bool PasswordRequireLowercase { get; set; }
        public int PasswordRequiredUniqueChars { get; set; }

        //lockout settings
        public double LockoutDefaultLockoutTimeSpanInMinutes { get; set; }
        public int LockoutMaxFailedAccessAttempts { get; set; }
        public bool LockoutAllowedForNewUsers { get; set; }

        //user settings
        public bool UserRequireUniqueEmail { get; set; }
        public bool SignInRequireConfirmedEmail { get; set; }

        //cookie settings
        public bool CookieHttpOnly { get; set; }
        public double CookieExpiration { get; set; }
        public bool SlidingExpiration { get; set; }

        //email auto confirmed
        public bool EmailAutoConfirmed { get; set; }

        //default return url
        public string? DefaultReturnUrl { get; set; }

    }
}
