using System.Text;
using NLog.Common;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;

namespace NLog.Azure.LayoutRenderers
{

    [LayoutRenderer("azure-local-resource")]
    public class LocalResourceLayoutRenderer : LayoutRenderer
    {
        private string _cachedValue;
        private bool _isCached;


        [RequiredParameter]
        [DefaultParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the default value to be used when the azure local storage is not defined.
        /// </summary>
        public string Default { get; set; }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (!_isCached)
            {
                _cachedValue = GetValue();
            }

            var layout = new SimpleLayout(_cachedValue);
            builder.Append(layout.Render(logEvent));
        }
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();
            _cachedValue = null;
            _isCached = false;
        }

        /// <summary>
        /// Closes the layout renderer.
        /// </summary>
        protected override void CloseLayoutRenderer()
        {
            base.CloseLayoutRenderer();
            _cachedValue = null;
            _isCached = false;
        }

        private string GetValue()
        {
            var serviceRuntime = new ServiceRuntimeWrapper();
            _isCached = true;
            if (serviceRuntime.IsAvailable && Name != null)
            {
                var rootPath = serviceRuntime.GetLocalResouceRootPath(Name);
                if (rootPath != null)
                {
                    return rootPath;
                }
            }
            if (Default != null)
                InternalLogger.Info($"RoleEnvironment not available or local resouce ({Name}) not defined. Use defaut value: {Default}");

            return Default;
        }
    }
}
