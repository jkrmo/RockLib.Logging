﻿using System.Linq;
using System.Xml.Serialization;
using Rock.Collections;
using Rock.Defaults.Implementation;
using Rock.DependencyInjection;

namespace Rock.Logging.Configuration
{
    public class XmlDeserializingLoggerFactory : ILoggerFactory
    {
        private IApplicationInfo _applicationInfo = Default.ApplicationInfo;
        private IResolver _supplimentaryContainer;

        private IKeyedEnumerable<string, LogFormatterProxy> _formatters = new FunctionalKeyedCollection<string, LogFormatterProxy>(f => f.Name, Enumerable.Empty<LogFormatterProxy>());
        private IKeyedEnumerable<string, ThrottlingRuleEvaluatorProxy> _throttlingRuleEvaluators = new FunctionalKeyedCollection<string, ThrottlingRuleEvaluatorProxy>(f => f.Name, Enumerable.Empty<ThrottlingRuleEvaluatorProxy>());
        private IKeyedEnumerable<string, Category> _categories = new FunctionalKeyedCollection<string, Category>(f => f.Name, Enumerable.Empty<Category>());
        private ContextProviderProxy[] _contextProviders = new ContextProviderProxy[0];

        [XmlAttribute("isLoggingEnabled")]
        public bool IsLoggingEnabled { get; set; }

        [XmlAttribute("loggingLevel")]
        public LogLevel LoggingLevel { get; set; }

        [XmlElement("auditLogProvider")]
        public LogProviderProxy AuditLogProvider { get; set; }

        [XmlArray("formatters")]
        [XmlArrayItem("formatter")]
        public LogFormatterProxy[] Formatters
        {
            get { return _formatters.ToArray(); }
            set { _formatters = new FunctionalKeyedCollection<string, LogFormatterProxy>(f => f.Name, value); }
        }

        [XmlArray("throttlingRules")]
        [XmlArrayItem("throttlingRule")]
        public ThrottlingRuleEvaluatorProxy[] ThrottlingRules
        {
            get { return _throttlingRuleEvaluators.ToArray(); }
            set { _throttlingRuleEvaluators = new FunctionalKeyedCollection<string, ThrottlingRuleEvaluatorProxy>(f => f.Name, value); }
        }

        [XmlArray("categories")]
        [XmlArrayItem("category")]
        public Category[] Categories
        {
            get { return _categories.ToArray(); }
            set { _categories = new FunctionalKeyedCollection<string, Category>(f => f.Name, value); }
        }

        [XmlArray("contextProviders")]
        [XmlArrayItem("contextProvider")]
        public ContextProviderProxy[] ContextProviders
        {
            get { return _contextProviders; }
            set { _contextProviders = value ?? new ContextProviderProxy[0]; }
        }

        public void SetApplicationInfo(IApplicationInfo applicationInfo)
        {
            _applicationInfo = applicationInfo ?? Default.ApplicationInfo;
        }

        public void SetSupplimentaryContainer(IResolver supplimentaryContainer)
        {
            _supplimentaryContainer = supplimentaryContainer;
        }

        public TLogger Get<TLogger>(string categoryName = null) where TLogger : ILogger
        {
            var category =
                categoryName != null && _categories.Contains(categoryName)
                    ? _categories[categoryName]
                    : _categories.First();

            var configuration = new LoggerConfiguration
            {
                IsLoggingEnabled = IsLoggingEnabled,
                LoggingLevel = LoggingLevel
            };

            var logProviders = category.LogProviders.Select(x => x.CreateInstance(_formatters, _supplimentaryContainer)).ToList();

            var applicationInfo = _applicationInfo;

            var auditLogProvider =
                AuditLogProvider != null
                    ? AuditLogProvider.CreateInstance(_formatters, _supplimentaryContainer)
                    : null;

            var throttlingRuleEvaluator =
                category.ThrottlingRule != null && _throttlingRuleEvaluators.Contains(category.ThrottlingRule)
                    ? _throttlingRuleEvaluators[category.ThrottlingRule].CreateInstance(_supplimentaryContainer)
                    : NullThrottlingRuleEvaluator.Instance;

            var contextProviders = ContextProviders.Select(x => x.CreateInstance(_supplimentaryContainer)).ToList();

            var container = new AutoContainer(configuration, logProviders, applicationInfo, auditLogProvider, throttlingRuleEvaluator, contextProviders);

            if (_supplimentaryContainer != null)
            {
                container = container.MergeWith(_supplimentaryContainer);
            }

            return container.Get<TLogger>();
        }
    }
}