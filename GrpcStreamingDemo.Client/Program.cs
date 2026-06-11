using Grpc.Core;
using Grpc.Net.Client;
using GrpcStreamingDemo;

Console.Title = "gRPC Full Features Demo Client";
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("====================================================");
Console.WriteLine("🚀 STARTING gRPC COMPREHENSIVE DEMO - DAY 20 CLIENT");
Console.WriteLine("====================================================\n");
Console.ResetColor();

// 1. Establish connection channel to the gRPC Server
using var channel = GrpcChannel.ForAddress("https://localhost:7007");
var client = new Greeter.GreeterClient(channel);

try
{
    // ====================================================
    // PART 1: Unary RPC (Simple Request-Response)
    // ====================================================
    PrintSectionHeader("1. TESTING UNARY RPC (SayHello)");
    var unaryReply = await client.SayHelloAsync(new HelloRequest { Name = "Arash" });
    Console.WriteLine($"[Server Response]: {unaryReply.Message}");
    Console.WriteLine($"[Timestamp]: {unaryReply.Timestamp}");


    // ====================================================
    // PART 2: Server Streaming RPC
    // ====================================================
    PrintSectionHeader("2. TESTING SERVER STREAMING (SayHelloServerStream)");
    var serverStreamRequest = new HelloRequest { Name = "Sara", Count = 4 };
    using var serverCall = client.SayHelloServerStream(serverStreamRequest);

    Console.WriteLine("📥 Streaming data from server...");
    await foreach (var reply in serverCall.ResponseStream.ReadAllAsync())
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($" -> Received: {reply.Message} (Msg #{reply.MessageCount})");
        Console.ResetColor();
    }
    Console.WriteLine("✅ Server Streaming finished.");


    // ====================================================
    // PART 3: Client Streaming RPC
    // ====================================================
    PrintSectionHeader("3. TESTING CLIENT STREAMING (SayHelloClientStream)");
    using var clientCall = client.SayHelloClientStream();

    string[] namesToStream = { "Alice", "Bob", "Charlie" };
    Console.WriteLine("📤 Streaming data to server...");
    foreach (var name in namesToStream)
    {
        Console.WriteLine($" -> Sending: Hello request for {name}");
        await clientCall.RequestStream.WriteAsync(new HelloRequest { Name = name, Message = $"Stream msg from {name}" });
        await Task.Delay(400); // Simulate network gap
    }

    // Complete the stream and get the single aggregated response
    await clientCall.RequestStream.CompleteAsync();
    var clientStreamResult = await clientCall.ResponseAsync;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[Server Final Response]: {clientStreamResult.Message}");
    Console.ResetColor();


    // ====================================================
    // PART 4: Bidirectional Streaming RPC
    // ====================================================
    PrintSectionHeader("4. TESTING BIDIRECTIONAL STREAMING (SayHelloBidirectional)");
    using var biCall = client.SayHelloBidirectional();

    // Start a background task to listen to incoming server responses concurrently
    var readTask = Task.Run(async () =>
    {
        await foreach (var response in biCall.ResponseStream.ReadAllAsync())
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"   <- [Server Broadcast]: {response.Message}");
            Console.ResetColor();
        }
    });

    // Simultaneously send requests from the client side
    string[] chatMessages = { "Ping 1", "Ping 2", "Ping 3" };
    foreach (var msg in chatMessages)
    {
        Console.WriteLine($" -> [Client Sending]: {msg}");
        await biCall.RequestStream.WriteAsync(new HelloRequest { Message = msg });
        await Task.Delay(500);
    }

    await biCall.RequestStream.CompleteAsync();
    await readTask; // Ensure all incoming data is fully processed
    Console.WriteLine("✅ Bidirectional Streaming finished.");


    // ====================================================
    // PART 5: Robust Validation & Error Handling
    // ====================================================
    PrintSectionHeader("5. TESTING ERROR HANDLING & VALIDATION");

    // Case A: Invalid Name (Triggering ValidationInterceptor/Service checks)
    try
    {
        Console.WriteLine("🔥 Sending invalid request (Empty Name)...");
        await client.SayHelloWithValidationAsync(new HelloRequest { Name = "" });
    }
    catch (RpcException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Caught expected RpcException!");
        Console.WriteLine($"   Status Code: {ex.StatusCode}");
        Console.WriteLine($"   Detail: {ex.Status.Detail}");

        // Read custom error metadata if present
        var detailError = ex.Trailers.GetValue("error-detail");
        if (detailError != null) Console.WriteLine($"   Meta Detail: {detailError}");
        Console.ResetColor();
    }


    // ====================================================
    // PART 6: Deadlines & Client Cancellation
    // ====================================================
    PrintSectionHeader("6. TESTING DEADLINE & CANCELLATION");

    using var cts = new CancellationTokenSource();
    try
    {
        Console.WriteLine("⏱️ Requesting a 5-second operation, but client will cancel it after 2 seconds...");
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        await client.SayHelloWithDelayAsync(
            new HelloRequest { Name = "Iman", DelaySeconds = 5 },
            cancellationToken: cts.Token
        );
    }
    catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠️ Operation successfully cancelled by the client as expected!");
        Console.ResetColor();
    }


    // ====================================================
    // PART 7: Metadata, Custom Headers & Trailers
    // ====================================================
    PrintSectionHeader("7. TESTING METADATA (Headers & Trailers)");

    var clientHeaders = new Metadata
    {
        { "x-custom-header", "ConsoleDemoClient-v20" },
        { "accept-language", "fa-IR" },
        { "authorization", "Bearer FakeJwtTokenForDemoPurposeOnly" },
        { "x-api-key", "my-secret-api-key-12345" } // Matches AuthInterceptor check
    };

    using var metadataCall = client.SayHelloWithMetadataAsync(new HelloRequest { Name = "Reza" }, clientHeaders);

    // Read Response Headers (Sent before the main body response)
    var responseHeaders = await metadataCall.ResponseHeadersAsync;
    Console.WriteLine("📋 [Response Headers received from Server]:");
    foreach (var entry in responseHeaders)
    {
        Console.WriteLine($"   {entry.Key} : {entry.Value}");
    }

    var metadataReply = await metadataCall.ResponseAsync;
    Console.WriteLine($"[Body Message]: {metadataReply.Message}");

    // Read Response Trailers (Sent after the main body response)
    var responseTrailers = metadataCall.GetTrailers();
    Console.WriteLine("📋 [Response Trailers received from Server]:");
    foreach (var entry in responseTrailers)
    {
        Console.WriteLine($"   {entry.Key} : {entry.Value}");
    }


    // ====================================================
    // PART 8: Complete CRUD Operations Workflow
    // ====================================================
    PrintSectionHeader("8. TESTING USER CRUD WORKFLOW & USER STREAMING");

    // 1. CREATE USER
    Console.WriteLine("➕ 1. Creating a new user...");
    var createResult = await client.CreateUserAsync(new CreateUserRequest
    {
        Name = "Fateme Mohammadi",
        Email = "fateme@example.com",
        Role = "Moderator"
    });
    int newUserId = createResult.User.Id;
    Console.WriteLine($"   ✅ Created - ID: {newUserId} | Name: {createResult.User.Name} | Role: {createResult.User.Role}");

    // Add another user to populate the list
    await client.CreateUserAsync(new CreateUserRequest { Name = "Ali Alavi", Email = "ali@example.com", Role = "User" });

    // 2. GET USER
    Console.WriteLine($"\n🔍 2. Fetching User with ID {newUserId}...");
    var getResult = await client.GetUserAsync(new GetUserRequest { Id = newUserId });
    Console.WriteLine($"   ✅ Retrieved - Name: {getResult.User.Name} | Email: {getResult.User.Email}");

    // 3. UPDATE USER
    Console.WriteLine($"\n🔄 3. Updating User with ID {newUserId}...");
    var updateResult = await client.UpdateUserAsync(new UpdateUserRequest
    {
        Id = newUserId,
        Name = "Fateme Mohammadi (Updated)",
        Role = "Admin"
    });
    Console.WriteLine($"   ✅ Updated - New Name: {updateResult.User.Name} | New Role: {updateResult.User.Role}");

    // 4. STREAM USERS (Server Streaming over CRUD Repository)
    Console.WriteLine("\n👥 4. Streaming all current users in repository...");
    using var userStream = client.StreamUsers(new Empty());
    await foreach (var userReply in userStream.ResponseStream.ReadAllAsync())
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"   -> User #{userReply.User.Id}: {userReply.User.Name} ({userReply.User.Role})");
        Console.ResetColor();
    }

    // 5. DELETE USER
    Console.WriteLine($"\n❌ 5. Deleting User with ID {newUserId}...");
    var deleteResult = await client.DeleteUserAsync(new DeleteUserRequest { Id = newUserId });
    Console.WriteLine($"   ✅ Server Message: {deleteResult.Message} | Success status: {deleteResult.Success}");

}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n💥 Global Exception Occurred: {ex.Message}");
    Console.ResetColor();
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\n====================================================");
Console.WriteLine("🎉 ALL gRPC CAPABILITIES TESTED SUCCESSFULLY!");
Console.WriteLine("====================================================");
Console.ResetColor();
Console.WriteLine("Press any key to exit the demo application...");
Console.ReadKey();

// Helper method for clean visual styling
void PrintSectionHeader(string title)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"📌 {title}");
    Console.WriteLine(new string('-', 60));
    Console.ResetColor();
}