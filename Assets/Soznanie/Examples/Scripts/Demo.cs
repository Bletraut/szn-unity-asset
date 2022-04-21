using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Soznanie;

namespace Demo
{
    public class Demo : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField]
        GameObject connectScreenObj;
        [SerializeField]
        GameObject testScreenObj;

        [Header("Objects")]
        [SerializeField]
        GameObject chainErrorMessage;

        void Start()
        {
            testScreenObj.SetActive(false);

            SznManager.ConnectionFailed += SznManager_ConnectionFailed;
            SznManager.AccountsChanged += OnAccountsChanged;
            SznManager.ChainChanged += SznManager_ChainChanged;
        }

        private void Connected()
        {
            connectScreenObj.SetActive(false);
            testScreenObj.SetActive(true);
        }
        private void Disconnected()
        {
            connectScreenObj.SetActive(true);
            testScreenObj.SetActive(false);
        }

        private void SznManager_ConnectionFailed()
        {
            Debug.LogError("Install Metamask wallet and reload this page.");
        }

        private void OnAccountsChanged(List<string> accounts)
        {
            if (accounts.Count > 0) Connected();
            else Disconnected();
        }

        private void SznManager_ChainChanged(ChainId chainId)
        {
            chainErrorMessage.SetActive(chainId != ChainId.Ropsten);
        }

        public void OnConnectBtnPressed()
        {
            SznManager.ConnectToWallet();
        }
    }
}
