﻿using System;

namespace crossql.Config
{
    public class DbConfiguration
    {
        private readonly IDbProvider _provider;

        internal DbConfiguration(IDbProvider provider)
        {
            _provider = provider;
        }

        public void Configure<TModel>(Action<ITableOptions<TModel>> func) where TModel : class, new()
        {
            var tableOptions = new TableOptions<TModel>();
            func(tableOptions);
        }

        public void OverrideDialect(IDialect customDialect)
        {
            var provider =  _provider as DbProviderBase;
            provider?.OverrideDialect(customDialect);
        }
    }
}