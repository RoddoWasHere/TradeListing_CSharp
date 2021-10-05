
using Binance.Net.Enums;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradingAPI.Data;

namespace TradingAPI.Utilites
{
	public class PriceFetchConfig {
		public TimeSpan scheduleInterval;
		public KlineInterval klineInterval;
		public int batchSize = 50;

		public DateTime lastCompletedUpdateTime = DateTime.Now;
        public PriceFetchConfig(TimeSpan scheduleInterval)
        {
			this.scheduleInterval = scheduleInterval;
			var nowEpoch = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
			var scheduleMs = scheduleInterval.TotalSeconds;
			Console.WriteLine("now,schedule " + nowEpoch + ", " + scheduleMs);
			var nowRounded = nowEpoch - (nowEpoch % scheduleMs);
			var nowRoundedDate = DateTime.UnixEpoch.AddSeconds(nowRounded);

			lastCompletedUpdateTime = nowRoundedDate;//schedule now rounded
		}

		int lastBatchStart = 0;

		public bool hasCompeletedAllBatches() {//not implemented
			return false;
		}
		public bool isUpToDate()//not implemented
		{
			return false;
		}

		public int GetNextBatchOffset(int totalSize) {
			if (DateTime.UtcNow < lastCompletedUpdateTime) return -1;//already up to date
			lastBatchStart = lastBatchStart + batchSize;
			if (lastBatchStart > totalSize) {
				lastBatchStart = 0;//reset batch start
				lastCompletedUpdateTime = lastCompletedUpdateTime.Add(scheduleInterval);//set time to the future when all batches are done
				return -1; //batch complete
			}
			return lastBatchStart;
		}
	}

	public class PriceFetchBatchConfig {

		//static config (propbably a better way of doing this) - perhaps store state in DB
		public static List<PriceFetchConfig> configs = new List<PriceFetchConfig>() {
            new PriceFetchConfig(TimeSpan.FromMinutes(15)){ klineInterval = KlineInterval.FifteenMinutes },
			new PriceFetchConfig(TimeSpan.FromHours(1)){ klineInterval = KlineInterval.OneHour },
			new PriceFetchConfig(TimeSpan.FromDays(1)){ klineInterval = KlineInterval.OneDay },
		};
	}


	public class FetchPricesJob : IJob
	{
		private TradeListingDbContext _context;
		public FetchPricesJob(TradeListingDbContext context)
        {		
			_context = context;
		}
		public async Task Execute(IJobExecutionContext jobContext)
		{
			PriceFetchConfig config;
			DateTime prevUpdateTime;
			int batchStart;

			var totalSize = _context.InstrumentPair.Count();			

			var ct = 0;
			while (ct < PriceFetchBatchConfig.configs.Count)//find the highest priority batch that hasn't been done yet
			{
				config = PriceFetchBatchConfig.configs[ct];
				prevUpdateTime = config.lastCompletedUpdateTime;
				batchStart = config.GetNextBatchOffset(totalSize);
				if (batchStart != -1)//this job needs to be done
				{
					var newHistory = await TradingDbUtilies.GetHistoryBatchAsync(_context, config.klineInterval, batchStart, config.batchSize);

					Console.WriteLine("Completed API fetch...got new records " + newHistory.Count);
					Console.WriteLine(
						$"Got for interval ({config.scheduleInterval.ToString()}) batchStart:{batchStart} , totalSize {totalSize}, last update:{prevUpdateTime.ToLocalTime()}"
					);
					break;//exit now, do next job next time 
				}
				ct++;

			}

			if (ct >= PriceFetchBatchConfig.configs.Count)
				Console.WriteLine("All jobs have already completed on this run");
		}
	}

}