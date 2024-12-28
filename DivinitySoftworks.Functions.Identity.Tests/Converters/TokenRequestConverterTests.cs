using DivinitySoftworks.Functions.Identity.Contracts.Requests;
using System.Text.Json;
using Xunit;

namespace DivinitySoftworks.Functions.Identity.Tests.Converters {
    public class TokenRequestConverterTests {
        [Theory]
        [InlineData("{\"grant_type\": \"password\", \"client_id\": \"myClient\", \"client_secret\": \"mySecret\", \"code\": \"1234\", \"refresh_token\": \"abcd\"}", "password", "myClient", "mySecret", "1234", "abcd")]
        [InlineData("{\"GrantType\": \"client_credentials\", \"ClientId\": \"yourClient\", \"ClientSecret\": \"yourSecret\", \"Code\": \"5678\", \"RefreshToken\": \"efgh\"}", "client_credentials", "yourClient", "yourSecret", "5678", "efgh")]
        [InlineData("{\"grant_type\": \"authorization_code\", \"client_id\": \"theirClient\", \"code\": \"91011\"}", "authorization_code", "theirClient", null, "91011", null)]
        [InlineData("{\"GrantType\": \"refresh_token\", \"ClientSecret\": \"theirSecret\", \"refresh_token\": \"ijkl\"}", "refresh_token", null, "theirSecret", null, "ijkl")]
        public void CanDeserializeTokenRequest(string json, string expectedGrantType, string expectedClientId, string expectedClientSecret, string expectedCode, string expectedRefreshToken) {
            // Arrange
            var options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            };

            // Act
            var tokenRequest = JsonSerializer.Deserialize<TokenRequest>(json, options);

            // Assert
            Assert.NotNull(tokenRequest);
            Assert.Equal(expectedGrantType, tokenRequest.GrantType);
            Assert.Equal(expectedClientId, tokenRequest.ClientId);
            Assert.Equal(expectedClientSecret, tokenRequest.ClientSecret);
            Assert.Equal(expectedCode, tokenRequest.Code);
            Assert.Equal(expectedRefreshToken, tokenRequest.RefreshToken);
        }

        [Fact]
        public void CanSerializeTokenRequest() {
            // Arrange
            var tokenRequest = new TokenRequest {
                GrantType = "password",
                ClientId = "myClient",
                ClientSecret = "mySecret",
                Code = "1234",
                RefreshToken = "abcd"
            };

            var options = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Act
            var json = JsonSerializer.Serialize(tokenRequest, options);
            var expectedJson = "{\"grant_type\":\"password\",\"client_id\":\"myClient\",\"client_secret\":\"mySecret\",\"code\":\"1234\",\"refresh_token\":\"abcd\"}";

            // Assert
            Assert.Equal(expectedJson, json);
        }
    }
}
