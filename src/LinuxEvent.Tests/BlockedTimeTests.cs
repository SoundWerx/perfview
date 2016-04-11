﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diagnostics.Tracing.StackSources;
using Xunit;

namespace LinuxTracing.Tests
{
	/// <summary>
	/// Completed Context Switch - Starts with a thread that blocked through a sched_switch event and ends with the same
	/// thread that was unblocked through a sched_switch.
	/// Induced Context Switch - Starts with a blocked thread through sched_switch, ends when a different thread is
	/// using the same CPU, in which we induce that the original thread was unblocked somewhere in between.
	/// Incomplete Context Switch - Starts with a thread that has been blocked through a sched_switch event and unblocked
	/// when the sample data is finished and the thread has not been unblocked.
	/// </summary>
	public class BlockedTimeTests
	{
		public static int? StartLook = null;
		private void TotalBlockedTimeTest(string source, double expectedTotalBlockedPeriod, bool concurrentTest = false)
		{
			Constants.WaitUntilFileIsReady(source);

			if (concurrentTest)
			{
				StartLook = 10;
			}

			ParallelLinuxPerfScriptStackSource stackSource = new ParallelLinuxPerfScriptStackSource(source, doThreadTime: true);

			StartLook = null;

			Assert.Equal(expectedTotalBlockedPeriod, stackSource.TotalBlockedTime);
		}

		[Fact]
		public void NoTimeBlocked1()
		{
			string path = Constants.GetTestingPerfDumpPath("onegeneric");
			this.TotalBlockedTimeTest(path, expectedTotalBlockedPeriod: 0.0);
		}

		[Fact]
		public void OneCompletedContextSwitch()
		{
			string path = Constants.GetTestingPerfDumpPath("one_complete_switch");
			this.TotalBlockedTimeTest(path, expectedTotalBlockedPeriod: 1.0);
		}

		[Fact]
		public void OneInducedContextSwitch()
		{
			string path = Constants.GetTestingPerfDumpPath("one_induced_switch");
			this.TotalBlockedTimeTest(path, expectedTotalBlockedPeriod: 1.0);
		}

		[Fact]
		public void OneIncomplateContextSwitch()
		{
			string path = Constants.GetTestingPerfDumpPath("one_incomplete_switch");
			this.TotalBlockedTimeTest(path, expectedTotalBlockedPeriod: 1.0);
		}

		[Fact]
		public void NoTimeBlocked2_Induced()
		{
			string path = Constants.GetTestingPerfDumpPath("notimeblocked_induced");
			this.TotalBlockedTimeTest(path, expectedTotalBlockedPeriod: 2.0);
		}

		[Fact]
		public void MixedBlocked()
		{
			string path = Constants.GetTestingPerfDumpPath("mixed_switches");
			this.TotalBlockedTimeTest(path, expectedTotalBlockedPeriod: 8.0);
		}

		[Fact]
		public void ConcurrentBlockedTime()
		{
			string path = Constants.GetTestingFilePath(@"C:\Users\t-lufern\Desktop\Luca\dev\helloworld.trace.zip");
			var linearStackSource = new LinuxPerfScriptStackSource(path, true);
			Constants.WaitUntilFileIsReady(path);
			var parallelStackSource = new ParallelLinuxPerfScriptStackSource(path, true);

			Assert.Equal(linearStackSource.TotalBlockedTime, parallelStackSource.TotalBlockedTime);
		}
	}
}