using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Soznanie;

namespace Demo
{
    public class InventoryScreen : MonoBehaviour
    {
        [SerializeField]
        Transform itemsContainer;
        [SerializeField]
        Item itemPrefab;

        void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            ClearItems();

            SznManager.ItemsOf(SznManager.SelectedAccount, items =>
            {
                items.ForEach(data =>
                {
                    var item = Instantiate(itemPrefab, itemsContainer);
                    item.Setup(data);
                });
            });
        }

        private void ClearItems()
        {
            itemsContainer.GetComponentsInChildren<Transform>()
                .ToList().ForEach(t =>
                {
                    if (t != itemsContainer)
                        Destroy(t.gameObject);
                });
        }
    }
}
