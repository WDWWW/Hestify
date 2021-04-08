using System;
using System.Collections.Generic;
using RichardSzalay.MockHttp;

namespace Hestify.Test
{
    public abstract class HestifyTestBase : IDisposable
    {
        private readonly bool _defaultUri;

        protected HestifyClient Client
        {
            get
            {
                HestifyClient client = new(MockHttp.ToHttpClient());
                _disposables.Add(client);

                if (_defaultUri)
                    client = client.WithUri(DefaultUri);

                return client;
            }
        }

        protected readonly Uri DefaultUri = new("https://test.com");

        private readonly IList<IDisposable> _disposables = new List<IDisposable>();

        protected MockHttpMessageHandler MockHttp;

        protected HestifyTestBase(bool defaultUri = false)
        {
            _defaultUri = defaultUri;
            MockHttp = new MockHttpMessageHandler();
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables) disposable?.Dispose();

            MockHttp?.Dispose();
        }
    }
}