using DivinitySoftworks.Functions.Identity.Data;
using DivinitySoftworks.Functions.Identity.Security;
using Xunit;

namespace DivinitySoftworks.Functions.Identity.Tests.Security;
public sealed class TokenGeneratorTests {
    readonly string refreshToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IkJEMDUwQzUyNzI3ODE4RTcxOEMxQUFERjdBMjlDQkFFQjlDMEY3QzkiLCJ4NXQiOiJ2UVVNVW5KNEdPY1l3YXJmZWluTHJybkE5OGsiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiJmNmUzZThlMC0zNWUwLTQzMDktYjBiOC1mMjc2NDIwNDczZTQiLCJqdGkiOiI3ZTAxNzI4Yy1mYTczLTQwZGEtODk2Mi0zYzM2YTIzNjliMTEiLCJ0dHkiOiJyZWZyZXNoX3Rva2VuIiwidGFmIjoiYXV0aG9yaXphdGlvbl9jb2RlIiwibmJmIjoxNzE5NjY5NjA1LCJleHAiOjE3MjAyNzQ0MDUsImlzcyI6Imh0dHBzOi8vaWRlbnRpdHkuZGl2aW5pdHktc29mdHdvcmtzLmNvbSIsImF1ZCI6ImRpdmluaXR5LXNvZnR3b3Jrcy5jb20ifQ.piAuzKKjmP961wSgXoPdnxu_JXEbzlQT-3u8AWC2UfvI7GJavExD9LXWhGG2oRz_UziBGKXbFOgDJ_uzql1Vd58lc9O3XkvU14fkzHozatlTu6YR0WHGC6IdTlrxyzyeMbHXPf7d4CIWnW3-GB3vExcRlqvI1sENr7t_NKCLmMV_GofVHqhSSj380AdLTeTwsAhS2zirL7k1ykfIUl5oVwnmZvT4_91nACZXmwJK9N-xhyLji6rafYY1mfkdok68STHwA5hHCAYZoti9c0SzEfy5362IokMUJAqCom_4PE9HhEO3_KIK-3O40rtyTplZBk1Sf5Fchqj6CL6hAYyiAw";

    [Fact]
    public void ReadRefreshToken() {
        //TokenGenerator tokenGenerator = new(null, null);
        //bool result = tokenGenerator.ReadRefreshToken(refreshToken, out string? subject, out string? authType, out string? identifier);
        //Assert.True(result);

        string pwd = "YtJPW1QGpNp12TC2AgJT#a1mgrxv84pFBv3nbWs1WHT%!quZFMaU56@Z1!Xm";

        string bliep = BCrypt.Net.BCrypt.HashPassword(pwd);

        if (!BCrypt.Net.BCrypt.Verify(pwd, bliep)) {
            throw new Exception();
        }

    }

}
