using Your.CliClient;

// Run a number of clients in parallel to send a load (of Payload's) to the server.
Client.Startup(2, () => new Data().GetLoadTestPayload());
