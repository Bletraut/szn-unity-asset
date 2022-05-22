using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using Soznanie;
using System;

namespace Demo
{
    public class Item : MonoBehaviour
    {
        [SerializeField]
        Collection collection;
        [SerializeField]
        TMP_Text MintNumber;

        public void Setup(ItemData itemData)
        {
            collection.Setup(itemData.CollectionData, null);
            MintNumber.text = $"Mint number: {itemData.MintNumber}";
        }
    }
}
