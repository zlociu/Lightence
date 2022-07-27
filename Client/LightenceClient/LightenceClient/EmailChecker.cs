using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;

namespace LightenceClient
{
    static class EmailChecker
    {
        static public bool IsValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

    }
}
