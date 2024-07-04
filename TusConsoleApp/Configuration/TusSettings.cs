using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace TusConsoleApp.Configuration
{
    class TusSettings : ConfigurationSection
    {

        [ConfigurationProperty("baseAddress", IsRequired = true)]
        public string BaseAddress
        {
            get => this["baseAddress"]?.ToString();
            set => this["baseAddress"] = value;
        }

        [ConfigurationProperty("userName", IsRequired = true)]
        public string UserName
        {
            get => this["userName"]?.ToString();
            set => this["userName"] = value;
        }

        [ConfigurationProperty("login", IsRequired = true)]
        public string Login
        {
            get => this["login"]?.ToString();
            set => this["login"] = value;
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get => this["password"]?.ToString();
            set => this["password"] = value;
        }

    }
}
