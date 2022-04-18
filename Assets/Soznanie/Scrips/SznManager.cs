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
        public static void ConnectToWallet()
        {
            connectToWallet();
        }

        public event Action<List<string>> AccountsChanged;
        public event Action<ChainId> ChainChanged;
        public event Action<ChainId> Connect;
        public event Action Disconnect;
        public event Action ConnectionFailed;

        public static bool IsConnected => isConnected();
        public static List<string> Accounts => JsonUtility.FromJson<AccountsData>(getAccounts()).Accounts;

        private static SznManager instance;

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

                Init();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Init()
        {
            CreateCallback("OnAccountsChanged", false, HandleAccountsChanged);
            CreateCallback("OnChainChanged", false, HandleChainChanged);
            CreateCallback("OnConnect", false, HandleConnect);
            CreateCallback("OnDisconnect", false, HandleDisconnect);
            CreateCallback("OnConnectionFailed", false, HandleConnectionFailed);

            init(OnJsonCallbackHandler);
        }

        // Event handlers
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

        public string ContractAddress => contractAddress;
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
            Debug.Log($"Invoke name: {name}, data: {data}");

            var jsonCallback = callbacks.Find(x => x.Name == name);
            InvokeCallback(jsonCallback, data);
        }
        private static void InvokeCallback(int hashCode, string data)
        {
            Debug.Log($"Invoke hashCode: {hashCode}, data: {data}");

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
