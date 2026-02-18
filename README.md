## Architecture and Structure

The application follows a layered architecture with a clear separation of responsibilities between the frontend and backend.

On the backend, the codebase is organized as follows:

### Controllers
- Expose HTTP endpoints
- Handle request parameters and HTTP responses
- Delegate all business logic to the service layer

### Services
- Contain the core business logic of the application
- Implement authorization and access control rules
- Apply filtering and pagination
- Interact with the database through Entity Framework Core

### Interfaces
- Define contracts for the services
- Decouple controllers from concrete implementations
- Improve maintainability and extensibility of the codebase

This structure keeps the codebase clean, modular, and easier to maintain.

---

## Authentication and Authorization

Authentication is implemented using **JWT (JSON Web Tokens)**.

- During login, the backend validates user credentials and generates a JWT
- The token is stored on the frontend and sent with each authenticated request via the `Authorization` header
- The backend validates the token on every request before processing it

Authorization is role-based, ensuring that users can only perform actions permitted by their assigned role.

---

## Claims

JWT tokens include claims that represent the user’s identity, such as:

- User identifier
- Username
- Role

These claims are used by the backend to:
- Identify the authenticated user
- Enforce authorization rules
- Avoid unnecessary database queries on each request

---

## Access Control

Access control is enforced at the service layer, ensuring that business rules remain independent of the HTTP layer.

Permissions are evaluated based on the user’s role and task ownership, providing consistent and centralized authorization logic.
