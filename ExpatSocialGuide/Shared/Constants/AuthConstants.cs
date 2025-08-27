namespace Shared.Constants
{
    public static class AuthConstants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Employee = "Employee";
            public const string Guest = "Guest";
        }

        public static class AccountTypes
        {
            public const string Internal = "Internal";
            public const string Google = "Google";
            public const string External = "External";
        }

        public static class EmployeeStatus
        {
            public const string Active = "Active";
            public const string Inactive = "Inactive";
            public const string Suspended = "Suspended";
            public const string Resigned = "Resigned";
        }
    }
}