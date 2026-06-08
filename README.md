# gRPC Streaming Demo

A professional portfolio project demonstrating proficiency in **gRPC, Protobuf, and Streaming** with **.NET 8**.

## Implemented Features (Day 1)

* Set up a gRPC Service project
* Defined service contracts using Protobuf
* Implemented Unary RPC communication
* Created a Console Client application

## Implemented Features (Day 2)

* Advanced `.proto` definition with multiple RPC methods and message types
* Automatic compilation of Protobuf into C# generated code
* Extended message schemas with additional fields for scalability
* Preparation for different types of streaming (server, client, and bidirectional)

## Implemented Features (Day 3)

* Full implementation of Unary RPC
* Basic server-side error handling
* Fundamental logging integration
* Type-safe request and response messaging
* Professional Console Client with try-catch error handling


## Implemented Features (Day 4)

* Full Unary RPC implementation
* Server Streaming with multiple message responses
* Usage of `IServerStreamWriter` for streaming responses
* Client-side consumption using `ReadAllAsync`
* Cancellation handling and graceful stream termination

## Implemented Features (Day 5)

* Unary RPC implementation
* Server Streaming implementation
* Client Streaming (sending multiple messages from client to server)
* Usage of `IAsyncStreamReader` and `RequestStream.WriteAsync`
* Full stream lifecycle management on both client and server sides

## Implemented Features (Day 6)

* Unary RPC
* Server Streaming
* Client Streaming
* **Bidirectional Streaming (full duplex communication)**
* Concurrent handling of sending and receiving messages

## Implemented Features (Day 7)

* Unary, Server Streaming, Client Streaming, and Bidirectional Streaming
* **Robust error handling using `RpcException` and `StatusCode`**
* Server-side input validation
* Client-side handling of `RpcException`
* Use of `Metadata` for enriched error details

## Implemented Features (Day 8)

* All four RPC types
* Advanced error handling
* **Deadlines and cancellation support**
* Handling of `OperationCanceledException` and `DeadlineExceeded` exceptions
