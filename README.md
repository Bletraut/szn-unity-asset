# szn-unity-asset
 Unity asset for Soznanie project

# Installation
1. Загрузите [SoznanieAsset.unitypackage](SoznanieAsset.unitypackage) и ипортируйте его в юнити.
2. Из главной папки Soznanie возьмите префаб SznManager и поместите его на сцену в любое место.

   ![alt-text](Readme/SznManager.png "Title")
3. Подготовьте SznManager под ваше приложение. В инспекторе свойств у объекта SznManager настройки следующие параметры:
   
   `Auto Init` `(по умолчанию true)` указывает на то, что объект SznManager будет иницилизирован автоматически.
   > Tip: Если вам необходимо провести инициализацию в определенном месте кода, то отключите этот параметр и используйте `SznManager.Init()` в скрипте.

   `Contract Address` адрес вашего игрового контракта на платформе Soznanie.

    `Don't Destroy On Load` `(по умолчанию false)` SznManager это Singleton. Включите этот параметр, если не используете собственный менеджер для синглтонов.

   ![alt-text](Readme/Inspector.png "Title")
4. На сцене создайте `Canvas`, добавьте в него кнопку, дайте ей имя _ConnectToWallet_.
5. Создайте скрипт Demo.cs и добавьте в него следующий код:
   ```c#
    using System.Collections.Generic;
    using UnityEngine;

    using Soznanie;

    namespace Demo
    {
        public class Demo : MonoBehaviour
        {
            void Start()
            {
                SznManager.Initialized += OnAccountsChanged;
                SznManager.InitializationFailed += OnInitializationFailed;

                SznManager.AccountsChanged += OnAccountsChanged;
                SznManager.ChainChanged += OnChainChanged;
            }

            private void Connected()
            {
                // Show your game! Metamask is connected!
            }
            private void Disconnected()
            {
                // Ooops. Show disconnect message. There are not any accounts.
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
                if (chainId != ChainId.Ropsten)
                    Debug.LogError("Please select Ropsten network!");
            }

            public void OnConnectBtnPressed()
            {
                SznManager.ConnectToWallet();
            }
        }
    }

   ```
    Немного пояснений по коду.
    
    Добавляем основные обработчики событий. События инициализации вызываются один раз за жизненный цикл SznManager.

    ```c#
    void Start()
    {
        SznManager.Initialized += OnAccountsChanged;
        SznManager.InitializationFailed += OnInitializationFailed;

        SznManager.AccountsChanged += OnAccountsChanged;
        SznManager.ChainChanged += OnChainChanged;
    }
    ```

    Для проверки подключения к Metamask используйте событие `SznManager.AccountsChanged`. Данное событие вызывается каждый раз когда пользователь предоставил доступ к аккаунту, либо изменил текущий аккаунт, а так же при инициализации если пользователь уже подключил Metamask к приложению. Проверяйте массив `accounts` чтобы убедиться в наличии доступа к Metamask.
    > Tip: Фактически в массиве `accounts` может быть только один элемент.
    ```c#
    private void OnAccountsChanged(List<string> accounts)
    {
        if (accounts.Count > 0) Connected();
        else Disconnected();
    }
    ```

    SznManager не сможет взаимодействовать со смарт-контрактом если сеть, к которой подключен Metamask не совпадает с сетью, в которую выложен смарт-контракт. Для проверки сети к которой подключен Metamask используйте событие `SznManager.ChainChanged`. Данное событие вызывается каждый раз когда пользователь изменил сеть, либо при инициализации если пользователь уже подключил Metamask к приложени. Проверяйте параметр `chainId` чтобы убедиться в том, что пользователь подключен к нужной сети (в данном случае `Ropsten`). 
    > Tip: Сеть `Ropsten` используется при разработке проекта в качестве тестовой сети. Для проекта в продакшене используйте проверку подключения к сети `Mainnet`.
    ```c#
    private void OnChainChanged(ChainId chainId)
    {
        if (chainId != ChainId.Ropsten)
            Debug.LogError("Please select Ropsten network!");
    }
    ```
6. Добавьте метод `OnConnectBtnPressed` в событие `OnClick` на кнопке `ConnectToWallet`.