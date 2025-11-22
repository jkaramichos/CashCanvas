// wwwroot/js/plaid-interop.js
window.plaidLink = {
    open: function (linkToken) {
        const handler = Plaid.create({
            token: linkToken,
            onSuccess: (public_token, metadata) => {
                console.log(`Success! public_token: ${public_token}`);
                // You can use DotNet.invokeMethodAsync here to send the 
                // public_token back to your C# code to be exchanged for an access token.
                // Example: DotNet.invokeMethodAsync('YourAssemblyName', 'ProcessPlaidSuccess', public_token);
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
