window.plaidLink = {
    open: function (linkToken) {
        const handler = Plaid.create({
            token: linkToken,
            onSuccess: (public_token, metadata) => {
                console.log(`Success! public_token: ${public_token}`);
                // Send the public_token to the C# backend.
                DotNet.invokeMethodAsync('CashCanvas', 'ReceivePlaidPublicToken', public_token);
            },
            onLoad: () => {},
            onExit: (err, metadata) => {
                if (err != null) {
                    console.log(err);
                }
            },
            onEvent: (eventName, metadata) => {
                console.log(`Event: ${eventName}`);
            }
        });
        handler.open();
    }
};
