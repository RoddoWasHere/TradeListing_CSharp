var coinsReq = {
    "symbol": "BNBBTC",//?
    "status": "TRADING",//?
    "baseAsset": "BNB",
    "quoteAsset": "BTC",
    "baseAssetChar": "",
    "quoteAssetChar": "฿",
    "baseAssetName": "BNB",
    "quoteAssetName": "Bitcoin",
    "open": 0.008549,
    "high": 0.008662,
    "low": 0.008363,
    "close": 0.008504,
    "baseVolume": 156100.319,
    "quoteVolume": 1329.092301,
    "circulatingSupply": 168137036
};

interface IInstrument{

}

var query = {
    serverTime:0,
    //...
}

var instrument = {//not quoted
    //static
    "symbol": "BNB",
    "symbolChar": "",
    "symbolName": "BNB",

    "type":"currency | crytocurrency | stock | CFD | ....",//<-detemine ? ...

    //dynamic (fetch on request)
    "volume": 156100.319,
    "circulatingSupply": 168137036,//?static (is base volume)
};

var intrumentQuoted = {
    //static
    //------
    symbol: "BNBBTC",
    baseInstrument: "",//instrument(ref)
    quoteInstrument: "",//instrument(ref)
    
    iceBergAllowed: true,
    isSpotTradingAllowed: true,
    isMarginTradingAllowed: true,
    ocoAllowed: true,
    quoteOrderQuantityMarketAllowed: true,
    baseCommissionPrecision: 8,
    quoteCommissionPrecision: 8,
    permissions: [
        0,
        1
    ],

    //semi-static?
    //--------
    "orderTypes": [//???
        0,
        9,
        1,
        3,
        8
    ],
    filters: [],//?????

    //filters ?? (Can leave these out)
    "iceBergPartsFilter": {},
    "lotSizeFilter": {},
    "marketLotSizeFilter": {},
    "maxOrdersFilter": {},
    "maxAlgorithmicOrdersFilter": {},
    "minNotionalFilter": {},
    "priceFilter": {},
    "pricePercentFilter": {},
    "maxPositionFilter": null,


    //dynamic 
    //----------
    status: "TRADING",//?

    //interval
    open: 0.008549,
    high: 0.008662,
    low: 0.008363,
    close: 0.008504,
    
    //?
    lastPrice: 0,//???

}

//-------------------




var exchangeRegSym = {
    "name": "ETHBTC",
    "status": 2,
    "baseAsset": "ETH",
    "baseAssetPrecision": 8,
    "quoteAsset": "BTC",
    "quoteAssetPrecision": 8,
    "orderTypes": [
        0,
        9,
        1,
        3,
        8
    ],
    "iceBergAllowed": true,
    "isSpotTradingAllowed": true,
    "isMarginTradingAllowed": true,
    "ocoAllowed": true,
    "quoteOrderQuantityMarketAllowed": true,
    "baseCommissionPrecision": 8,
    "quoteCommissionPrecision": 8,
    "permissions": [
        0,
        1
    ],
    "filters": [
        {
            "filterType": 1
        },
        {
            "filterType": 2
        },
        {
            "filterType": 3
        },
        {
            "filterType": 5
        },
        {
            "filterType": 8
        },
        {
            "filterType": 4
        },
        {
            "filterType": 6
        },
        {
            "filterType": 7
        }
    ],
    "iceBergPartsFilter": {
        "limit": 10,
        "filterType": 8
    },
    "lotSizeFilter": {
        "minQuantity": 0.0001,
        "maxQuantity": 100000,
        "stepSize": 0.0001,
        "filterType": 3
    },
    "marketLotSizeFilter": {
        "minQuantity": 0,
        "maxQuantity": 921.75933488,
        "stepSize": 0,
        "filterType": 4
    },
    "maxOrdersFilter": {
        "maxNumberOrders": 200,
        "filterType": 6
    },
    "maxAlgorithmicOrdersFilter": {
        "maxNumberAlgorithmicOrders": 5,
        "filterType": 7
    },
    "minNotionalFilter": {
        "minNotional": 0.0001,
        "applyToMarketOrders": true,
        "averagePriceMinutes": 5,
        "filterType": 5
    },
    "priceFilter": {
        "minPrice": 0.000001,
        "maxPrice": 922327,
        "tickSize": 0.000001,
        "filterType": 1
    },
    "pricePercentFilter": {
        "multiplierUp": 5,
        "multiplierDown": 0.2,
        "averagePriceMinutes": 5,
        "filterType": 2
    },
    "maxPositionFilter": null
};


//----------



