using System.Text;
using Microsoft.WindowsAzure.ServiceRuntime;
using NLog.Common;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;

namespace NLog.Azure.LayoutRenderers
{

    [LayoutRenderer("azure-local-resource")]
    public class LocalResourceLayoutRenderer : LayoutRenderer
    {
        [RequiredParameter]
        [DefaultParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the default value to be used when the azure local storage is not defined.
        /// </summary>
        public string Default { get; set; }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (RoleEnvironment.IsAvailable && Name != null)
            {
                try
                {
                    var localResouceName = RoleEnvironment.GetLocalResource(Name);
                    var layout = new SimpleLayout(localResouceName.RootPath);
                    builder.Append(layout.Render(logEvent));
                    return;
                }
                catch (RoleEnvironmentException exception)
                {
                    InternalLogger.Warn("RoleEnvironmentException {0}", exception.Message);
                }
            }
            if (Default != null)
            {
                InternalLogger.Info("RoleEnvironment not available. Use defaut value: {0}", Default);

                var layout = new SimpleLayout(Default);
                builder.Append(layout.Render(logEvent));
            }
        }
    }
}
