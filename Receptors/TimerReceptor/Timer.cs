﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;				// So we can get a timer marshalled on the UI thread.

using Clifton.ExtensionMethods;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

using Clifton.Tools.Strings.Extensions;

namespace ATimerReceptor
{
	public class TimerReceptor : BaseReceptor
	{
		public override string Name { get { return "Interval Timer"; } }
		public override string ConfigurationUI { get { return "TimerConfig.xml"; } }

		protected bool ready;
		protected Timer timer;

		[UserConfigurableProperty("Days:")]
		public string DaySpan { get; set; }

		[UserConfigurableProperty("Hours:")]
		public string HourSpan { get; set; }

		[UserConfigurableProperty("Minutes:")]
		public string MinuteSpan { get; set; }

		[UserConfigurableProperty("Seconds:")]
		public string SecondSpan { get; set; }

		[UserConfigurableProperty("ProtocolName:")]
		public string ProtocolName { get; set; }

		protected DateTime triggerTime;
		protected int days = 0;
		protected int hours = 0;
		protected int minutes = 0;
		protected int seconds = 0;

		public TimerReceptor(IReceptorSystem rsys)
			: base(rsys)
		{
		}

		public override void Initialize()
		{
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();

			if (!String.IsNullOrEmpty(ProtocolName))
			{
				AddEmitProtocol(ProtocolName);
				InitializeTimerInterval();
			}
		}

		public override void Terminate()
		{
			DisposeOfTimer();
		}

		public override bool UserConfigurationUpdated()
		{
			bool ret = rsys.SemanticTypeSystem.VerifyProtocolExists(ProtocolName);

			if (ret)
			{
				RemoveEmitProtocols();
				AddEmitProtocol(ProtocolName);
				DisposeOfTimer();
				InitializeTimerInterval();
			}
			else
			{
				ConfigurationError = "The semantic type '" + ProtocolName + "' is not defined.";
			}

			return ret;
		}

		protected void InitializeTimerInterval()
		{
			DisposeOfTimer();

			days = 0;
			hours = 0;
			minutes = 0;
			seconds = 0;

			Int32.TryParse(DaySpan, out days);
			Int32.TryParse(HourSpan, out hours);
			Int32.TryParse(MinuteSpan, out minutes);
			Int32.TryParse(SecondSpan, out seconds);

			int totalms = (seconds + (minutes * 60) + (hours * 60 * 60) + (days * 24 * 60 * 60)) * 1000;

			if (totalms > 0)
			{
				triggerTime = DateTime.Now + new TimeSpan(days, hours, minutes, seconds);
				// Acquire a new timer.
				timer = new Timer();
				timer.Interval = 1000;
				timer.Tick += FireEvent;
				timer.Start();
			}
		}

		protected void DisposeOfTimer()
		{
			if (timer != null)
			{
				timer.Stop();
				timer.Dispose();
				timer = null;
			}
		}

		protected void FireEvent(object sender, EventArgs e)
		{
			if (triggerTime <= DateTime.Now)
			{
				triggerTime = DateTime.Now + new TimeSpan(days, hours, minutes, seconds);

				if (Enabled)
				{
					CreateCarrierIfReceiver(ProtocolName, signal => { });
				}
			}

			TimeSpan ts = triggerTime - DateTime.Now;

			Subname = String.Format("{0}:{1}:{2:D2}:{3:D2}", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
		}
	}
}


