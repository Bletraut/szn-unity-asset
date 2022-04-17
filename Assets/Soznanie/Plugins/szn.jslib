let sznPlugin = {
    
    $sznData: {
        jsonCallbackHandler: null,

        isInitiated: false,

        eth: null,
        web3: null,
        accounts: [],

        sendJsonCallback: function (name, hashCode, data) {
            let callbackObj = {
                Name: name,
                HashCode: hashCode,
                Data: data
            };
            Runtime.dynCall("vi", this.jsonCallbackHandler, [this.strToUTF8(JSON.stringify(callbackObj))]);
        },

        init: function (callback) {
            this.jsonCallbackHandler = callback;

            if (this.hasMetamask) {
                isInitiated = true;
                this.eth = window.ethereum;

                this.initHandlers();
            }
        },

        initHandlers: function () {
            this.eth.on('accountsChanged', this.handleAccountsChanged);
            this.eth.on('chainChanged', this.handleChainChanged);
            this.eth.on('connect', this.handleConnect);
            this.eth.on('disconnect', this.handleDisconnect);
        },

        // Event handlers
        handleAccountsChanged: function (accounts) {
            let dataObj = {
                Accounts: accounts,
            };
            this.sendJsonCallback("OnAccountsChanged", 0, JSON.stringify(dataObj));
        },
        handleChainChanged: function (chainId) {
            this.sendJsonCallback("OnChainChanged", 0, chainId);
        },
        handleConnect: function (connectInfo) {
            this.sendJsonCallback("OnConnect", 0, connectInfo.chainId);
        },
        handleDisconnect: function (error) {
            this.sendJsonCallback("OnDisconnect", 0, "");
        },

        hasMetamask: function () {
            return window.ethereum != null;
        },

        // Utils
        strToUTF8: function (str) {
           let bufferSize = lengthBytesUTF8(str) + 1;
           let buffer = _malloc(bufferSize);
           stringToUTF8(str, buffer, bufferSize);

           return buffer;
        },
    },

    init: function () {
        sznData.init();
    },

    connectToWallet: function () {
        if (sznData.hasMetamask()) {
            sznData.eth = window.ethereum;

            sznData.eth.request({ method: 'eth_accounts' })
              .then(sznData.handleAccountsChanged)
              .catch((err) => {
                    sznData.sendJsonCallback("OnConnectionFailed", 0, "");
              });

            if (!sznData.isInitiated)
            {
                sznData.initHandlers();
            }
        }
        else {
            sznData.sendJsonCallback("OnConnectionFailed", 0, "");
        }
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

};

autoAddDeps(LibraryManager.library, "$sznData");
mergeInto(LibraryManager.library, sznPlugin);