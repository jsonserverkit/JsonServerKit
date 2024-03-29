﻿using JsonServerKit.AppServer.Data;
using Your.CliClient;

// Run a number of clients in parallel to send a load (of Payload's) to the server.
Client.Startup(2, () =>
{
    var payloads = new List<Payload>();
    // Create some payload objects.
    foreach (var i in Enumerable.Range(1, 100))
        payloads.AddRange(new Data().GetLoadTestPayload());

    return payloads.ToArray();
});
