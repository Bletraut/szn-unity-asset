using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Soznanie.Demo
{
    public class Demo : MonoBehaviour
    {
        public void OnTestPressed()
        {
            SznManager.ConnectToWallet();
        }
    }
}
