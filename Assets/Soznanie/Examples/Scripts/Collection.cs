using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using Soznanie;
using System;

namespace Demo
{
    public class Collection : MonoBehaviour
    {
        [SerializeField]
        Image Image;
        [SerializeField]
        TMP_Text Symbol;
        [SerializeField]
        TMP_Text Type;
        [SerializeField]
        TMP_Text Price;
        [SerializeField]
        TMP_Text TotalSupply;
        [SerializeField]
        TMP_Text ItemsOnSale;

        string symbol;
        Action<string> pressedAction;

        public void Setup(CollectionData collectionData, Action<string> pressedAction)
        {
            symbol = collectionData.Symbol;
            this.pressedAction = pressedAction;

            Symbol.text = $"Symbol: {collectionData.Symbol}";
            Type.text = $"Type: {collectionData.Type}"; ;
            Price.text = $"Price: {collectionData.Price}";
            TotalSupply.text = $"Total Supply: {collectionData.TotalSupply}";
            ItemsOnSale.text = $"Items On Sale: {collectionData.ItemsOnSale}";

            SznManager.GetItemImage(collectionData.ImageHash, responseData =>
            {
                if (responseData.IsSuccessed)
                {
                    Image.sprite = Sprite.Create(responseData.Texture2D,
                        new Rect(0, 0, responseData.Texture2D.width, responseData.Texture2D.height),
                        Vector2.one / 2f);
                }
            });
        }

        public void OnBuyBtnPressed()
        {
            pressedAction?.Invoke(symbol);
        }
    }
}
