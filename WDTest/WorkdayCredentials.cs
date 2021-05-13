using System;

namespace WDTest
{
    public class WorkdayCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class WorkdayOptions : WorkdayCredentials
    {
        public string RootUrl { get; set; }
    }
}
