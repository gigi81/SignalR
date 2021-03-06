﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicValidationHelper;
using SignalR.Hosting;

namespace SignalR.Hosting.AspNet
{
    public class AspNetRequest : IRequest
    {
        private readonly HttpRequestBase _request;
        private NameValueCollection _form;
        private NameValueCollection _queryString;

        public AspNetRequest(HttpRequestBase request)
        {
            _request = request;

            Cookies = new RequestCookieCollection(request.Cookies.Cast<string>()
                .Select(key =>
                    {
                        var cookie = request.Cookies[key];
                        return new Cookie (
                            name: cookie.Name,
                            value: cookie.Value,
                            domain: cookie.Domain,
                            path: cookie.Path
                        );
                    }));

            ResolveFormAndQueryString();
        }

        public Uri Url
        {
            get
            {
                return _request.Url;
            }
        }

        public NameValueCollection QueryString
        {
            get
            {
                return _queryString;
            }
        }

        public NameValueCollection Headers
        {
            get
            {
                return _request.Headers;
            }
        }

        public NameValueCollection Form
        {
            get
            {
                return _form;
            }
        }

        public RequestCookieCollection Cookies
        {
            get;
            private set;
        }

        private void ResolveFormAndQueryString()
        {
            // Since the ValidationUtility has a dependency on HttpContext (not HttpContextBase) we
            // need to check if we're out of HttpContext to preserve testability.
            if (HttpContext.Current == null)
            {
                _form = _request.Form;
                _queryString = _request.QueryString;
            }
            else
            {
                try
                {
                    ResolveUnvalidatedCollections();
                }
                catch
                {
                    // TODO: Cache this
                    // Fallback to grabbing values from the request
                    _form = _request.Form;
                    _queryString = _request.QueryString;
                }
            }
        }

        private void ResolveUnvalidatedCollections()
        {
            Func<NameValueCollection> formGetter, queryGetter;
            ValidationUtility.GetUnvalidatedCollections(HttpContext.Current, out formGetter, out queryGetter);
            _form = formGetter();
            _queryString = queryGetter();
        }
    }
}
