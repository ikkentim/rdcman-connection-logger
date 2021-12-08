using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using RdcMan;

namespace RdcPlgTest
{
    [Export(typeof(IPlugin))]
    public class LoggerPlugin : IPlugin
    {
		private static string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName, Application.ProductName, "LoggerPlugin.config");
		
		private string _configuredLogger = "http://localhost:5001/";
	    public void PreLoad(IPluginContext context, XmlNode xmlNode)
	    {
			// Due to an rdcman bug I can't use the build-in settings :(
			LoadSettings();

			/*
            if (xmlNode is XmlElement el)
            {
                var att = el.GetAttribute("server");

                if (att != null)
                {
                    _configuredLogger = att;
                }
            }
			*/


			LoggerClient.Initialize(_configuredLogger);
        }
		
		private void LoadSettings()
        {
			if(File.Exists(SettingsPath))
			_configuredLogger = File.ReadAllText(SettingsPath).Trim();
        }

	    public async void PostLoad(IPluginContext context)
	    {
			Server.ConnectionStateChanged += ServerOnConnectionStateChanged;

			for(;;)
			{
				var avail = await LoggerClient.GetAvailable();

				if(avail)
                {
					return;
                }

				var cancel = false;
				var form = ((Form)context.MainForm);

				var action = new Action(() =>{
					var prompt = new LoggerServerEntry
					{
						Value = _configuredLogger
					};

					var result = prompt.ShowDialog();

					if(result == DialogResult.Cancel)
                    {
						cancel = true;
                    }
                    else
                    {
						_configuredLogger = prompt.Value;
						LoggerClient.Initialize(_configuredLogger);
                    }
				});

				if(form.InvokeRequired)
				{
					form.Invoke(action);
				}
                else
                {
					action();
                }
				
                if (cancel)
                {
					return;
                }
			}
	    }

	    private void ServerOnConnectionStateChanged(ConnectionStateChangedEventArgs obj)
	    {
			var entry = new LoggerEntry
            {
				UserName = Environment.UserName,
				Action = obj.State.ToString(),
				RemoteName = obj.Server.DisplayName,
				RemoteAddress = obj.Server.ServerName
            };

			LoggerClient.Send(entry);
	    }

	    public XmlNode SaveSettings()
	    {
			/*
		    var doc = new XmlDocument();
		    var el = doc.CreateElement("RdcManConnectionLogger");
		    el.SetAttribute("server", _configuredLogger);
			return el;
			*/

			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
				File.WriteAllText(SettingsPath, _configuredLogger);
			}
			catch {}

			return null;
	    }

	    public void Shutdown()
		{
		}

	    public void OnUndockServer(IUndockedServerForm form)
		{
		}

	    public void OnDockServer(ServerBase server)
		{
		}

	    public void OnContextMenu(ContextMenuStrip contextMenuStrip, RdcTreeNode node)
		{
		}
    }
}
