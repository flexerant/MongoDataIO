using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flexerant.MongoDataIO.Core
{
    class Helpers
    {
        public static string FormatUri(string uri)
        {
            return uri.Trim().Trim('/');
        }
    }
}
