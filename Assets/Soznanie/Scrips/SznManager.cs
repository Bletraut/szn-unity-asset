using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using AOT;

using UnityEngine;

namespace Soznanie
{
    // Unity methods
    public partial class SznManager : MonoBehaviour
    {
        /// <summary>
        /// Init SznManager.
        /// </summary>
        public static void Init() => instance.InstanceInit();
        /// <summary>
        /// Connect SznManager to Metamask wallet.
        /// </summary>
        public static void ConnectToWallet() => connectToWallet();

        /// <summary>
        /// SznManager emits this event when initialization is successful.
        /// </summary>
        public static event Action<List<string>> Initialized;
        /// <summary>
        /// SznManager emits this event when initialization is failed.
        /// <para>
        /// User need install Metamask. See <see cref="IsMetamaskAvailable"/> property.
        /// </para>
        /// </summary>
        public static event Action InitializationFailed;
        /// <summary>
        /// The MetaMask provider emits this event whenever 
        /// the return value of the eth_accounts RPC method changes. 
        /// Returns an array that is either empty or contains a single account address.
        /// </summary>
        public static event Action<List<string>> AccountsChanged;
        /// <summary>
        /// The MetaMask provider emits this event 
        /// when the currently connected chain changes.
        /// </summary>
        public static event Action<ChainId> ChainChanged;
        /// <summary>
        /// The MetaMask provider emits this event when 
        /// it first becomes able to submit RPC requests to a chain.
        /// <para>
        /// Do not use this method to test connectivity.
        /// </para>
        /// <para>
        /// To check the connection use the <see cref="AccountsChanged"/> event
        /// and <see cref="IsConnected"/> property.
        /// </para>
        /// </summary>
        public static event Action<ChainId> Connect;
        /// <summary>
        /// The MetaMask provider emits this event 
        /// if it becomes unable to submit RPC requests to any chain.
        /// <para>
        /// Do not use this method to test connectivity.
        /// </para>
        /// <para>
        /// To check the connection use the <see cref="AccountsChanged"/> event
        /// and <see cref="IsConnected"/> property.
        /// </para>
        /// </summary>
        public static event Action Disconnect;
        /// <summary>
        /// SznManager emits this event when the connection to the wallet fails.
        /// </summary>
        public static event Action ConnectionFailed;

        /// <summary>
        /// Returns contract address from SznManager instance.
        /// </summary>
        public static string ContractAddress
        {
            get
            {
                if (instance == null)
                    return null;

                return instance.contractAddress;
            }
        }
        /// <summary>
        /// Returns true if SznManager initialized.
        /// </summary>
        public static bool IsInitialized => callbacks.Count > 0;
        /// <summary>
        /// Returns true if user has Metamask.
        /// </summary>
        public static bool IsMetamaskAvailable => isMetamaskAvailable();
        /// <summary>
        /// Returns true if the provider is connected to 
        /// the current chain, and false otherwise.
        /// <para>
        /// If the provider is not connected, the page will have 
        /// to be reloaded in order for connection to be re-established.
        /// </para>
        /// <para>
        /// Do not use this method to check if SznManager is connected to Metamask.
        /// </para>
        /// </summary>
        public static bool IsConnected => isConnected();
        /// <summary>
        /// Returns an array that is either empty or contains a single account address.
        /// </summary>
        public static List<string> Accounts => JsonUtility.FromJson<AccountsData>(getAccounts()).Accounts;

        private static SznManager instance;

        [SerializeField]
        private bool autoInit = true;
        [SerializeField]
        private string contractAddress;
        [SerializeField]
        private bool dontDestroyOnLoad;

        void Awake()
        {
            if (string.IsNullOrEmpty(contractAddress))
                throw new Exception("The contract address cannot be empty.");

            if (instance == null)
            {
                instance = this;
                if (dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);

                if (autoInit) InstanceInit();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InstanceInit()
        {
            if (IsInitialized)
            {
                Debug.LogError("SznManager already initialized.");
                return;
            }

            CreateCallback("OnInitialized", false, HandleInitialized);
            CreateCallback("OnInitializationFailed", false, HandleInitializationFailed);
            CreateCallback("OnAccountsChanged", false, HandleAccountsChanged);
            CreateCallback("OnChainChanged", false, HandleChainChanged);
            CreateCallback("OnConnect", false, HandleConnect);
            CreateCallback("OnDisconnect", false, HandleDisconnect);
            CreateCallback("OnConnectionFailed", false, HandleConnectionFailed);

            init(OnJsonCallbackHandler);
        }

        // Event handlers
        void HandleInitialized(string data)
        {
            var accounts = JsonUtility.FromJson<AccountsData>(getAccounts()).Accounts;
            Initialized?.Invoke(accounts);
        }
        void HandleInitializationFailed(string data)
        {
            InitializationFailed?.Invoke();
        }
        void HandleAccountsChanged(string data)
        {
            var accounts = JsonUtility.FromJson<AccountsData>(getAccounts()).Accounts;
            AccountsChanged?.Invoke(accounts);
        }
        void HandleChainChanged(string data)
        {
            var chainId = (ChainId)Convert.ToInt32(data, 16);
            ChainChanged?.Invoke(chainId);
        }
        void HandleConnect(string data)
        {
            var chainId = (ChainId)Convert.ToInt32(data, 16);
            Connect?.Invoke(chainId);
        }
        void HandleDisconnect(string data)
        {
            Disconnect?.Invoke();
        }
        void HandleConnectionFailed(string data)
        {
            ConnectionFailed?.Invoke();
        }
    }

    // Szn.jslib methods
    public partial class SznManager
    {
        [DllImport("__Internal")]
        private static extern void init(Action<string> jsonCallback);

        [DllImport("__Internal")]
        private static extern void connectToWallet();

        [DllImport("__Internal")]
        private static extern bool isConnected();

        [DllImport("__Internal")]
        private static extern string getAccounts();

        [DllImport("__Internal")]
        private static extern bool isMetamaskAvailable();
    }

    // Callback's logic
    public partial class SznManager
    {
        private static List<JsonCallback> callbacks = new();

        private static JsonCallback CreateCallback(string name, bool disposable,
            Action<string> callback)
        {
            if (!string.IsNullOrEmpty(name))
                if (callbacks.Any(x => x.Name == name))
                    throw new Exception("A callback with this name already exists.");

            var jsonCallback = new JsonCallback()
            {
                Name = name,
                Disposable = disposable,
                Callback = callback
            };
            callbacks.Add(jsonCallback);

            return jsonCallback;
        }

        private static void InvokeCallback(string name, string data)
        {
            var jsonCallback = callbacks.Find(x => x.Name == name);
            InvokeCallback(jsonCallback, data);
        }
        private static void InvokeCallback(int hashCode, string data)
        {
            var jsonCallback = callbacks.First(x => x.GetHashCode() == hashCode);
            InvokeCallback(jsonCallback, data);
        }
        private static void InvokeCallback(JsonCallback jsonCallback, string data)
        {
            Debug.Log(jsonCallback.Name);
            jsonCallback.Callback?.Invoke(data);

            if (jsonCallback.Disposable)
                callbacks.Remove(jsonCallback);
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnJsonCallbackHandler(string data)
        {
            Debug.Log($"Data = {data}");
            var callbackData = JsonUtility.FromJson<JsonCallbackData>(data);

            if (string.IsNullOrEmpty(callbackData.Name))
                InvokeCallback(callbackData.HashCode, callbackData.Data);
            else
                InvokeCallback(callbackData.Name, callbackData.Data);
        }

        private class JsonCallback
        {
            public string Name { get; set; }
            public bool Disposable { get; set; }
            public Action<string> Callback { get; set; }
        }
    }
}
