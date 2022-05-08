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

            SznManager.Initialized += OnAccountsChanged;
            SznManager.InitializationFailed += OnInitializationFailed;

            SznManager.AccountsChanged += OnAccountsChanged;
            SznManager.ChainChanged += OnChainChanged;
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

        private void OnInitializationFailed()
        {
            Debug.LogError("Install Metamask wallet and reload this page.");
        }

        private void OnAccountsChanged(List<string> accounts)
        {
            if (accounts.Count > 0) Connected();
            else Disconnected();
        }

        private void OnChainChanged(ChainId chainId)
        {
            chainErrorMessage.SetActive(chainId != ChainId.Ropsten);
        }

        public void OnConnectBtnPressed()
        {
            SznManager.ConnectToWallet();
        }

        public void OnShopBtnPressed()
        {
            SznManager.GetCollections(collections =>
            {
                collections.ForEach(collection => Debug.Log(collection));
            });
        }

        public void OnInventoryBtnPressed()
        {
            SznManager.ItemsOf(SznManager.SelectedAccount, items =>
            {
                items.ForEach(item => Debug.Log(item));
            });
        }

        public void OnBuyBtnPressed()
        {
            if (string.IsNullOrEmpty(SznManager.SelectedAccount))
                return;

            SznManager.BuyItem("Gun_1", purchaseData =>
            {
                Debug.Log($"Error:{purchaseData.ErrorMessage}, TransactionHash: {purchaseData.TransactionHash}");
            });
        }    
    }
}
