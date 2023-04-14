using System;
using System.Collections.Generic;

namespace TusWebApplication.TusAzure
{
    sealed class TusAzureStoreDictionary : Dictionary<string, TusAzureStore>
    {

        public TusAzureStoreDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        { }

    }
}
