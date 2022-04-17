using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Soznanie
{
    public partial class SznManager
    {
        private struct AccountsData
        {
            public List<string> Accounts { get; set; }
        }

        private struct JsonCallbackData
        {
            public string Name { get; set; }
            public int HashCode { get; set; }
            public string Data { get; set; }
        }
    }
}
