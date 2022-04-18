using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Soznanie
{
    public partial class SznManager
    {
        [Serializable]
        private struct AccountsData
        {
            public List<string> Accounts;
        }

        [Serializable]
        private struct JsonCallbackData
        {
            public string Name;
            public int HashCode;
            public string Data;
        }
    }
}
