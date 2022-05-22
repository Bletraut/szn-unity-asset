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

        [Serializable]
        private struct Collections
        {
            public List<CollectionData> CollectionDatas;
        }

        [Serializable]
        private struct Items
        {
            public List<ItemData> ItemDatas;
        }
    }

    [Serializable]
    public struct CollectionData
    {
        public string Symbol;
        public string Type;
        public float Price;
        public string Contract;
        public int TotalSupply;
        public int ItemsOnSale;
        public string ImageHash;

        public override string ToString()
        {
            return @$"{{ Symbol:{Symbol}, Type: {Type}, Price: {Price}, Contract: {Contract}, TotalSupply:{TotalSupply}, ItemsOnSale{ItemsOnSale}, ImageHash:{ImageHash} }}";
        }
    }

    [Serializable]
    public struct ItemData
    {
        public CollectionData CollectionData;
        public int MintNumber;

        public override string ToString()
        {
            return $"{{ Collection symbol:{CollectionData.Symbol}, Collection type: {CollectionData.Type}, MintNumber:{MintNumber} }}";
        }
    }

    [Serializable]
    public struct PurchaseData
    {
        public string ErrorMessage;
        public string TransactionHash;

        public bool HasError => !IsSuccessed;
        public bool IsSuccessed => string.IsNullOrEmpty(ErrorMessage);
    }

    [Serializable]
    public struct ImageResponseData
    {
        public string ErrorMessage;
        public Texture2D Texture2D;

        public bool HasError => !IsSuccessed;
        public bool IsSuccessed => string.IsNullOrEmpty(ErrorMessage);
    }
}
