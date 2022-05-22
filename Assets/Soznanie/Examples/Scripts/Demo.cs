using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using TMPro;

using Soznanie;

namespace Demo
{
    public class Demo : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField]
        GameObject connectScreenObj;
        [SerializeField]
        GameObject menuScreenObj;
        [SerializeField]
        GameObject shopScreenObj;
        [SerializeField]
        GameObject inventoryScreenObj;

        [Header("Objects")]
        [SerializeField]
        GameObject chainErrorMessageObj;
        [SerializeField]
        TMP_Text chainErrorMessageText;

        [Header("Settings")]
        [SerializeField]
        ChainId targetChainId;

        GameObject currentScreen;

        void Start()
        {
            CurrentScreen = connectScreenObj;

            chainErrorMessageText.text = $"Please change net to {targetChainId}";

            SznManager.Initialized += OnAccountsChanged;
            SznManager.InitializationFailed += OnInitializationFailed;

            SznManager.AccountsChanged += OnAccountsChanged;
            SznManager.ChainChanged += OnChainChanged;
        }

        private void Connected()
        {
            CurrentScreen = menuScreenObj;
        }
        private void Disconnected()
        {
            CurrentScreen = connectScreenObj;
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
            chainErrorMessageObj.SetActive(chainId != targetChainId);
        }

        public void OnConnectBtnPressed()
        {
            SznManager.ConnectToWallet();
        }

        public void OnShopBtnPressed()
        {
            CurrentScreen = shopScreenObj;
        }

        public void OnInventoryBtnPressed()
        {
            CurrentScreen = inventoryScreenObj;
        }
        
        public void OnBackBtnPressed()
        {
            CurrentScreen = menuScreenObj;
        }
        
        GameObject CurrentScreen
        {
            get => currentScreen;
            set
            {
                if (currentScreen != value)
                {
                    if (currentScreen != null)
                        currentScreen.SetActive(false);

                    currentScreen = value;
                    currentScreen.SetActive(true);
                }
            }
        }
    }
}
