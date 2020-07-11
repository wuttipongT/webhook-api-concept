namespace CustomizedClickOnce.Common
{

    //These parameters should be read from some config in real applciation
    //Here they're just hard coded
    public class Globals
    {
        public static string PublisherName
        {
            get { return "World Electric"; }
        }

        public static string ProductName
        {
            get { return "SFC Message Center"; }
        }

        public static string Host
        {
            get { return @"\\129.3.25.14\public\SFC Message Center\"; }
        }

        public static string HelpLink
        {
            get { return @"\\129.3.25.14\public\SFC Message Center\"; }
        }

        public static string AboutLink
        {
            get { return @"\\129.3.25.14\public\SFC Message Center\"; }
        }
    }
}
