﻿using Refit;

namespace Service.Echo;

public static class Prelude
{
    public static async Task<IApiResponse<T>> Retry<T>(Func<Task<IApiResponse<T>>> api)
    {
        for (var i = 0; i < 10; i++)
        {
            var res = await api.Invoke();

            //if (res.IsSuccessStatusCode)
            //{
            //    return res;
            //}

            //await (res.StatusCode switch
            //{
            //    HttpStatusCode.TooManyRequests => Task.Delay(TimeSpan.FromMilliseconds(100)),
            //    HttpStatusCode.ServiceUnavailable => Task.Delay(TimeSpan.FromMilliseconds(100)),
            //    _ => throw res.Error ?? new Exception(res.ReasonPhrase)
            //});
        }

        throw new Exception("Too many retries");
    }
}
