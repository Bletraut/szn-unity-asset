let sznPlugin = {
    
    $sznData: {
        jsonCallbackHandler: 0,

        isInitiated: false,

        itemRegistry: null,
        itemRegistryContract: null,
        contractAddress: "",

        eth: null,
        web3: null,
        accounts: [],

        sendJsonCallback: function (name, hashCode, data) {
            let callbackObj = {
                Name: name,
                HashCode: hashCode,
                Data: data
            };
            dynCall_vi(sznData.jsonCallbackHandler, sznData.strToUTF8(JSON.stringify(callbackObj)));
        },

        init: function (jsonCallback) {
            this.jsonCallbackHandler = jsonCallback;

            if (this.hasMetamask()) {
                this.isInitiated = true;
                this.eth = window.ethereum;

                this.web3 = new Web3(this.eth);

                this.initHandlers();

                fetch("TemplateData/SznData/ItemRegistry.json")
                    .then(response => {
                        response.json()
                            .then(json => {
                                sznData.itemRegistry = json;

                                sznData.itemRegistryContract = new sznData.web3.eth.Contract(sznData.itemRegistry.abi, sznData.contractAddress);

                                ethereum.request({ method: 'eth_accounts' })
                                  .then(accounts => {
                                        sznData.handleInitialized(accounts);
                                        if (accounts.length !== 0) {
                                            sznData.eth.request({ method: 'eth_chainId' })
                                                .then(sznData.handleChainChanged);
                                        }
                                  })
                                  .catch((err) => {
                                        sznData.sendJsonCallback("OnInitializationFailed", 0, "");
                                  });
                            });
                    })
                    .catch((err) => {
                        sznData.sendJsonCallback("OnInitializationFailed", 0, "");
                    });
            }
            else {
                sznData.sendJsonCallback("OnInitializationFailed", 0, "")
            }
        },

        initHandlers: function () {
            this.eth.on('accountsChanged', this.handleAccountsChanged);
            this.eth.on('chainChanged', this.handleChainChanged);
            this.eth.on('connect', this.handleConnect);
            this.eth.on('disconnect', this.handleDisconnect);
        },

        // Event handlers
        handleInitialized: function (accounts) {
            let dataObj = {
                Accounts: accounts,
            };
            sznData.accounts = accounts;
            sznData.sendJsonCallback("OnInitialized", 0, JSON.stringify(dataObj));
        },
        handleAccountsChanged: function (accounts) {
            let dataObj = {
                Accounts: accounts,
            };
            sznData.accounts = accounts;
            sznData.sendJsonCallback("OnAccountsChanged", 0, JSON.stringify(dataObj));
        },
        handleChainChanged: function (chainId) {
            sznData.sendJsonCallback("OnChainChanged", 0, chainId);
        },
        handleConnect: function (connectInfo) {
            sznData.sendJsonCallback("OnConnect", 0, connectInfo.chainId);
        },
        handleDisconnect: function (error) {
            sznData.sendJsonCallback("OnDisconnect", 0, "");
        },

        hasMetamask: function () {
            return typeof window.ethereum !== 'undefined';
        },

        getCollections: function (callback) {
            if (sznData.hasMetamask()) {

                sznData.itemRegistryContract.methods.getTypes().call({from: sznData.accounts[0]}, function(error, result) {

                    let itemSymbols = result.itemSymbols.map(s => sznData.web3.utils.hexToUtf8(s));
                    let itemTypes = result.itemTypes.map(t => sznData.web3.utils.hexToUtf8(t));
                    let itemPrices = result.itemPrices.map(p => sznData.web3.utils.fromWei(p));

                    let collections = {
                        CollectionDatas: result.itemSymbols.map((e, i) => ({
                            Symbol: itemSymbols[i],
                            Type: itemTypes[i],
                            Price: itemPrices[i],
                            Contract: result.itemContracts[i],
                            TotalSupply: result.itemTotalSupply[i],
                            ItemsOnSale: result.itemsOnSale[i],
                            ImageHash: result.itemImages[i],
                        }))
                    }

                    callback(collections);
                });

            }
            else {
                let collections = {
                    CollectionDatas: []
                }
                callback(collections);
            }
        },

        // Utils
        strToUTF8: function (str) {
           let bufferSize = lengthBytesUTF8(str) + 1;
           let buffer = _malloc(bufferSize);
           stringToUTF8(str, buffer, bufferSize);

           return buffer;
        },
    },

    init: function (contractAddress, jsonCallback) {
        sznData.contractAddress = UTF8ToString(contractAddress);
        sznData.init(jsonCallback);
    },

    connectToWallet: function () {
        if (sznData.hasMetamask()) {
            sznData.eth = window.ethereum;

            if (!sznData.isInitiated)
            {
                sznData.initHandlers();
            }

            sznData.eth.request({ method: "eth_requestAccounts" })
              .then(accounts => {
                    sznData.eth.request({ method: 'eth_chainId' })
                        .then(sznData.handleChainChanged);
              })
              .catch((err) => {
                    sznData.sendJsonCallback("OnConnectionFailed", 0, "");
              });
        }
        else {
            sznData.sendJsonCallback("OnConnectionFailed", 0, "");
        }
    },

    isMetamaskAvailable: function () {
        return sznData.hasMetamask();
    },

    isConnected: function () {
        if (sznData.hasMetamask()) {
            return window.ethereum.isConnected();
        }
        else {
            return false;
        }
    },

    getAccounts: function () {
        let dataObj = {
            Accounts: sznData.accounts,
        };

        return sznData.strToUTF8(JSON.stringify(dataObj));
    },

    getCollections: function (jsonCallbackHash) {
        sznData.getCollections(collections => {
            sznData.sendJsonCallback("", jsonCallbackHash, JSON.stringify(collections));
        });
    },

    itemsOf: function (address, jsonCallbackHash) {
        let itemsOwner = UTF8ToString(address);

        sznData.getCollections(collections => {
            sznData.itemRegistryContract.methods.itemsOf(itemsOwner).call({from: sznData.accounts[0]}, function(error, result) {

                let items = {
                    ItemDatas: [],
                };
                let mintCounter = 0;
                for (let i = 0; i < result.itemsOfType.length; i++) {
                    let symbolIndex = result.itemsOfType[i];
                    if (symbolIndex > 0) {
                        for (let j = 0; j < symbolIndex; j++) {
                            let item = {
                                CollectionData: collections.CollectionDatas[i],
                                MintNumber: result.ownedItems[mintCounter],
                            };
                            items.ItemDatas.push(item);

                            mintCounter++;
                        }
                    }
                }

                sznData.sendJsonCallback("", jsonCallbackHash, JSON.stringify(items));
            });
        });
    },

    buyItem: function (symbol, jsonCallbackHash) {
        let collectionSymbol = UTF8ToString(symbol);

        sznData.getCollections(collections => {
            var symbolIndex = collections.CollectionDatas.map(c => c.Symbol).indexOf(collectionSymbol);

            let purchaseData = {
                ErrorMessage: "",
                TransactionHash: "",
            };

            if (symbolIndex < 0) {
                purchaseData.ErrorMessage = "There are not any collections with this symbol.";
                sznData.sendJsonCallback("", jsonCallbackHash, JSON.stringify(purchaseData));
            }
            else {
                var price = sznData.web3.utils.toWei(collections.CollectionDatas[symbolIndex].Price);

                sznData.itemRegistryContract.methods.buyItem(symbolIndex).send({from: sznData.accounts[0], value: price})
                .on('confirmation', function(confirmationNumber, receipt) {
                    purchaseData.TransactionHash = receipt.transactionHash;
                    sznData.sendJsonCallback("", jsonCallbackHash, JSON.stringify(purchaseData));
                })
                .on('error', function(error, receipt) {
                    purchaseData.ErrorMessage = error.message;
                    sznData.sendJsonCallback("", jsonCallbackHash, JSON.stringify(purchaseData));
                });
            }
        });
    },
};

autoAddDeps(LibraryManager.library, "$sznData");
mergeInto(LibraryManager.library, sznPlugin);