mergeInto(LibraryManager.library, {

    DDB_Setup: function(region, identityPool, callbackGameObjectName) {

        if (typeof unityInstance == "undefined") {
            throw "Cannot find unityInstance. Make sure you are using the UXF WebGL template."
        }

        AWS.config.region = Pointer_stringify(region);
        AWS.config.credentials = new AWS.CognitoIdentityCredentials({
            IdentityPoolId: Pointer_stringify(identityPool),
        });

        // get credentials to account for lazy loading
        AWS.config.credentials.get(function(err) {
            if (err) console.log(err);
        });

        callbackGameObjectName = Pointer_stringify(callbackGameObjectName);
        window.onbeforeunload = function(e) {
            console.log("Calling OnClose from Browser!");
            unityInstance.SendMessage(callbackGameObjectName, "HandleBeforeUnloadEvent");

            // Cancel the event so the page doesn't close
            (e || window.event).preventDefault();

            // This never shows up correctly for me, but it does prompt
            // the player to close their window with a dialogue box
            var dialogText = "Are you sure you would like to continue unloading the page?";
            (e || window.event).returnValue = dialogText;
            return dialogText;
        };

    },

    DDB_CreateTable: function(tableName, primaryKeyName, sortKeyName, callbackGameObjectName) {
        var ddb = new AWS.DynamoDB({ apiVersion: '2012-08-10' });
        callbackGameObjectName = Pointer_stringify(callbackGameObjectName);

        tableName = Pointer_stringify(tableName);

        if (Pointer_stringify(sortKeyName)) {
            params = {
                AttributeDefinitions: [{
                        AttributeName: Pointer_stringify(primaryKeyName),
                        AttributeType: "S"
                    },
                    {
                        AttributeName: Pointer_stringify(sortKeyName),
                        AttributeType: "N"
                    }
                ],
                KeySchema: [{
                        AttributeName: Pointer_stringify(primaryKeyName),
                        KeyType: 'HASH'
                    },
                    {
                        AttributeName: Pointer_stringify(sortKeyName),
                        KeyType: 'RANGE'
                    }
                ],
                BillingMode: "PAY_PER_REQUEST",
                TableName: tableName
            };
        } else {
            params = {
                AttributeDefinitions: [{
                    AttributeName: Pointer_stringify(primaryKeyName),
                    AttributeType: "S"
                }],
                KeySchema: [{
                    AttributeName: Pointer_stringify(primaryKeyName),
                    KeyType: 'HASH'
                }],
                BillingMode: "PAY_PER_REQUEST",
                TableName: tableName
            };
        }

        ddb.createTable(params, function(err, data) {
            if (err) {
                if (err.code === "ResourceInUseException") {
                    // error OK, just means table is already created
                } else {
                    console.error("Error", err);
                    unityInstance.SendMessage(callbackGameObjectName, "ShowError", "Error in DDB_CreateTable: (" + tableName + "): " + err.message);
                }
            }
        });

    },

    DDB_PutItem: function(tableName, jsonItem, callbackGameObjectName) {

        var ddb = new AWS.DynamoDB({ apiVersion: '2012-08-10' });
        callbackGameObjectName = Pointer_stringify(callbackGameObjectName);
        tableName = Pointer_stringify(tableName);

        var request = JSON.parse(Pointer_stringify(jsonItem));

        var params = {
            Item: AWS.DynamoDB.Converter.marshall(request),
            TableName: tableName
        };

        var tryPutItem = function(tableName, params, callbackGameObjectName) {
            ddb.putItem(params, function(err, data) {
                if (err) {
                    if (err.code === "ResourceNotFoundException") {
                        var ms = 5000;
                        console.log("Table '" + tableName + "' not found - trying again in " + ms + "ms.");
                        setTimeout(tryPutItem, ms, tableName, params, callbackGameObjectName);
                    } else {
                        console.error("Error", err);
                        unityInstance.SendMessage(callbackGameObjectName, "ShowError", "Error in DDB_PutItem (" + tableName + "): " + err.message);
                    }
                }
            });
        };

        tryPutItem(tableName, params, callbackGameObjectName);

    },

    DDB_BatchWriteItem: function(tableName, jsonRequests, callbackGameObjectName) {
        var ddb = new AWS.DynamoDB({ apiVersion: '2012-08-10' });
        callbackGameObjectName = Pointer_stringify(callbackGameObjectName);
        tableName = Pointer_stringify(tableName);

        var reqToParams = function(x) {
            return { PutRequest: { Item: AWS.DynamoDB.Converter.marshall(x) } };
        }

        var request = JSON.parse(Pointer_stringify(jsonRequests));
        var params = { RequestItems: {} };
        params.RequestItems[tableName] = request.map(reqToParams);

        var tryBatchWriteItem = function(tableName, params, callbackGameObjectName) {
            ddb.batchWriteItem(params, function(err, data) {
                if (err) {
                    if (err.code === "ResourceNotFoundException") {
                        var ms = 5000;
                        console.log("Table '" + tableName + "' not found - trying again in " + ms + "ms.");
                        setTimeout(tryBatchWriteItem, ms, tableName, params, callbackGameObjectName);
                    } else {
                        console.error("Error", err);
                        unityInstance.SendMessage(callbackGameObjectName, "ShowError", "Error in DDB_BatchWriteItem (" + tableName + "): " + err.message);
                    }
                }
            });
        };

        tryBatchWriteItem(tableName, params, callbackGameObjectName);
    },


    DDB_GetItem: function(tableName, jsonItem, callbackGameObjectName, guid) {

        var ddb = new AWS.DynamoDB({ apiVersion: '2012-08-10' });
        callbackGameObjectName = Pointer_stringify(callbackGameObjectName);

        var request = JSON.parse(Pointer_stringify(jsonItem));

        var params = {
            Key: AWS.DynamoDB.Converter.marshall(request),
            TableName: Pointer_stringify(tableName)
        };

        guid = Pointer_stringify(guid);

        ddb.getItem(params, function(err, data) {

            var response = {
                guid: guid,
                result: null
            };
            console.log(data);
            if (err) {
                console.error("Error", err);
                unityInstance.SendMessage(callbackGameObjectName, "ShowError", "Error in DDB_GetItem: " + err.message);
                unityInstance.SendMessage(callbackGameObjectName, "HandleUnsuccessfulDBRead", JSON.stringify(response));
            } else {
                response["result"] = AWS.DynamoDB.Converter.unmarshall(data.Item);
                unityInstance.SendMessage(callbackGameObjectName, "HandleSuccessfulDBRead", JSON.stringify(response));
            }
        });
    },

    DDB_Cleanup: function() {

        window.onbeforeunload = function(e) {
            // the absence of a returnValue property on the event will guarantee the browser unload happens
            delete e['returnValue'];
        };

    }

});