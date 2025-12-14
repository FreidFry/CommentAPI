//using Comment.Core.Interfaces;
//using RestSharp;

//namespace CommentAPI.Middleware
//{
//    public class AuthMiddleware
//    {
//        public class AuthMiddleware : IMiddleware
//        {
//            private readonly ITokenStorage _tokenStorage;
//            //private readonly IRestStorage _restStorage;

//            /// <summary>
//            /// Initializes a new instance of the <see cref="AuthMiddleware"/> class.
//            /// </summary>
//            /// <param name="tokenStorage">The token storage service.</param>
//            /// <param name="restStorage">The REST storage service.</param>
//            //public AuthMiddleware(ITokenStorage tokenStorage, IRestStorage restStorage)
//            public AuthMiddleware(ITokenStorage tokenStorage)
//            {
//                _tokenStorage = tokenStorage;
//                //_restStorage = restStorage;
//            }

//            /// <summary>
//            /// Invokes the authentication middleware.
//            /// </summary>
//            /// <param name="context">The HTTP context.</param>
//            /// <param name="next">The next middleware delegate.</param>
//            /// <returns>A task representing the asynchronous operation.</returns>
//            public async Task InvokeAsync(HttpContext context, RequestDelegate next)
//            {
//                try
//                {
//                    CancellationToken cancellationToken = context.RequestAborted;

//                    RestResponse response;

//                    if (!string.IsNullOrEmpty(_tokenStorage.Access_Token) && _tokenStorage.IsAccessTokenExpired())
//                    {
//                        response = await _restStorage.ExecuteGetTokenRequestAsync("refresh_token", cancellationToken);
//                        _tokenStorage.ParseAndSetTokens(response);
//                    }
//                    else if (string.IsNullOrEmpty(_tokenStorage.Access_Token))
//                    {
//                        response = await _restStorage.ExecuteGetTokenRequestAsync("password", cancellationToken);
//                        _tokenStorage.ParseAndSetTokens(response);
//                    }

//                    await next(context);
//                }
//                catch (UnauthorizedAccessException)
//                {
//                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
//                    await context.Response.WriteAsync("Unauthorized");
//                }
//            }


//        }
//    }
//}
