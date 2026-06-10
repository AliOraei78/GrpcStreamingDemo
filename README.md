# gRPC Streaming Demo

A professional portfolio project demonstrating proficiency in **gRPC, Protobuf, and Streaming** with **.NET 8**.

## Day 1: Implemented Features

* Set up a gRPC Service project
* Defined service contracts using Protobuf
* Implemented Unary RPC communication
* Created a Console Client application

## Day 2 & 3: Implemented Features

* Advanced `.proto` definition with multiple RPC methods and message types
* Automatic compilation of Protobuf into C# generated code
* Extended message schemas with additional fields for scalability
* Preparation for different types of streaming (server, client, and bidirectional)

## Day 4: Implemented Features

* Full implementation of Unary RPC
* Basic server-side error handling
* Fundamental logging integration
* Type-safe request and response messaging
* Professional Console Client with try-catch error handling


## Day 5: Implemented Features

* Full Unary RPC implementation
* Server Streaming with multiple message responses
* Usage of `IServerStreamWriter` for streaming responses
* Client-side consumption using `ReadAllAsync`
* Cancellation handling and graceful stream termination

## Day 6: Implemented Features

* Unary RPC implementation
* Server Streaming implementation
* Client Streaming (sending multiple messages from client to server)
* Usage of `IAsyncStreamReader` and `RequestStream.WriteAsync`
* Full stream lifecycle management on both client and server sides

## Day 7: Implemented Features

* Unary RPC
* Server Streaming
* Client Streaming
* **Bidirectional Streaming (full duplex communication)**
* Concurrent handling of sending and receiving messages

## Day 8:Implemented Features

* Unary, Server Streaming, Client Streaming, and Bidirectional Streaming
* **Robust error handling using `RpcException` and `StatusCode`**
* Server-side input validation
* Client-side handling of `RpcException`
* Use of `Metadata` for enriched error details

## Day 9: Implemented Features

* All four RPC types
* Advanced error handling
* **Deadlines and cancellation support**
* Handling of `OperationCanceledException` and `DeadlineExceeded` exceptions

## Day 10: Metadata and Headers in gRPC

* Implementing sending and receiving Metadata in Request and Response
* Using `context.RequestHeaders` and `context.ResponseHeaders`
* Adding Trailers for post-response information
* Simple authentication demo using the Authorization header
* New method: `SayHelloWithMetadata`

## Day 11: Interceptors (Logging + Validation)

* Implementing a `LoggingInterceptor` for centralized and unified logging
* Implementing a `ValidationInterceptor` for reusable input validation
* Registering interceptors in `Program.cs` using `MapGrpcService(options)`
* Full separation of cross-cutting concerns from business logic
* Basic support for Unary and Server Streaming calls

## Day 12: Authentication (API Key + JWT)

* Implementing `AuthInterceptor` for authentication
* Supporting both API Key and JWT authentication simultaneously
* Handling `Unauthenticated` errors
* Generating and validating JWT tokens
* Integration with previous interceptors

## Day 13: Advanced Logging and Health Checks

* Enhancing `LoggingInterceptor` with Request ID and structured logging
* Implementing `GrpcHealthCheck`
* Adding Health Check endpoints (`/health` and `/healthz`)
* Preparing for production-grade monitoring (Kubernetes, Docker, etc.)

## Day 14: Unit Testing gRPC Services with xUnit

* Creating a separate test project (`GrpcStreamingDemo.Tests`)
* Writing tests for Unary methods (`SayHello` and `SayHelloWithValidation`)
* Using `xUnit` (`Fact` and `Theory`) and `Moq`
* Testing `RpcException` error scenarios
* Creating a helper class for `TestServerCallContext`

## Day 15: Integration Testing and Test Server

* Creating integration tests using `WebApplicationFactory`
* Performing end-to-end testing of gRPC services (Unary, Validation, Authentication)
* Testing the Health Check endpoint
* Using `GrpcChannel` with the test client

## Days 16–18: CRUD + Streaming (Real-World User Management Scenario)

## Days 16:

* Defining the User model and Create/Get methods
* In-memory storage implementation
* Initial CRUD testing

## Day 17:

* Full implementation of `UpdateUser` and `DeleteUser`
* Server streaming using `StreamUsers`
* Handling `NotFound` errors
* Real-world CRUD scenario combined with streaming

## Days 19–20: Clean Architecture + Dependency Injection

## Days 19:

* Applying Clean Architecture principles (Domain, Application, Infrastructure, Presentation layers)
* Introducing the Repository Pattern for better abstraction
* Improving Dependency Injection setup
* Preparing the project for better scalability and maintainability

## Days 20:

* Full refactoring of `GreeterService` using `UserService`
* Complete separation of layers (Domain, Application, Infrastructure)
* Improved Dependency Injection and testability
* Professional structure ready for scaling and deployment

## Day 21: Dockerizing the Service

* Creating a multi-stage `Dockerfile` for optimized builds and smaller image sizes
* Supporting Solution/Project-based application structures
* Using Docker build and run commands
* Fully preparing the service for deployment