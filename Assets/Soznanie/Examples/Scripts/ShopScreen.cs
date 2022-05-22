using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Soznanie;
using System;

namespace Demo
{
    public class ShopScreen : MonoBehaviour
    {
        [SerializeField]
        Transform collectionsContainer;
        [SerializeField]
        Collection collectionPrefab;

        void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            ClearCollections();

            SznManager.GetCollections(collections =>
            {
                collections.ForEach(data =>
                {
                    var collection = Instantiate(collectionPrefab, collectionsContainer);
                    collection.Setup(data, OnCollectionPressed);
                });
            });
        }

        private void ClearCollections()
        {
            collectionsContainer.GetComponentsInChildren<Transform>()
                .ToList().ForEach(t =>
                {
                    if (t != collectionsContainer)
                        Destroy(t.gameObject);
                });
        }

        private void OnCollectionPressed(string symbol)
        {
            SznManager.BuyItem(symbol, purchaseData =>
            {
                if (purchaseData.IsSuccessed)
                    Refresh();
                else
                    Debug.LogError(purchaseData.ErrorMessage);
            });
        }
    }
}
