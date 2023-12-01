# TwoFactorAuthAPI
Two-Factor Authentication Service
This service provides APIs for two-factor authentication using .NET Core and Redis as a caching mechanism.

**Prerequisites**

.NET Core SDK
Redis

**Getting Started**

Clone this repository:
git clone <repository_url>

cd TwoFactorAuthAPI

**Configure Redis:**

Ensure Redis is installed and running.

Update the Redis connection string in appsettings.json:

"RedisConnectionString":  "your_redis_connection_string"

**Run the application:**

dotnet run

Use an API testing tool like Postman to interact with the following endpoints:

use the postman collection checked-in repository to test the API.

Generate Access token: POST /connect/token

Send Confirmation Code: POST /api/2fa/send-code

Check Confirmation Code: POST /api/2fa/check-code

**Configuration**

Modify appsettings.json to set code lifetime and other configurations:

"CodeLifetimeMinutes": 5,

"ConcurrentCodesPerPhone": 3


Additional Notes
Ensure proper security configurations, such as HTTPS, CORS settings, and input validation, before deploying this service in production.
For development purposes, consider setting up secure environments, managing secrets securely, and conducting security reviews.
