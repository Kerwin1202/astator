﻿using Android.Graphics;
using Com.Baidu.Paddle.Lite.Ocr;
using System.Collections.Generic;

namespace astator.Core.ThirdParty
{
    public class CpuPowerMode
    {
        /// <summary>
        /// 不绑核运行模式（推荐）。系统根据负载自动调度任务到空闲的 CPU 核心上
        /// </summary>
        public const string LITE_POWER_NO_BIND = "LITE_POWER_NO_BIND";

        /// <summary>
        /// 绑定大核运行模式。如果 ARM CPU 支持 big.LITTLE，则优先使用并绑定 Big cluster，如果设置的线程数大于大核数量，则会将线程数自动缩放到大核数量。如果系统不存在大核或者在一些手机的低电量情况下会出现绑核失败，如果失败则进入不绑核模式
        /// </summary>
        public const string LITE_POWER_HIGH = "LITE_POWER_HIGH";

        /// <summary>
        /// 绑定小核运行模式。如果 ARM CPU 支持 big.LITTLE，则优先使用并绑定 Little cluster，如果设置的线程数大于小核数量，则会将线程数自动缩放到小核数量。如果找不到小核，则自动进入不绑核模式
        /// </summary>
        public const string LITE_POWER_LOW = "LITE_POWER_LOW";

        /// <summary>
        /// 大小核混用模式。线程数可以大于大核数量，当线程数大于核心数量时，则会自动将线程数缩放到核心数量
        /// </summary>
        public const string LITE_POWER_FULL = "LITE_POWER_FULL";

        /// <summary>
        /// 轮流绑定大核模式。如果 Big cluster 有多个核心，则每预测10次后切换绑定到下一个核心
        /// </summary>
        public const string LITE_POWER_RAND_HIGH = "LITE_POWER_RAND_HIGH";

        /// <summary>
        /// 轮流绑定小核模式。如果 Little cluster 有多个核心，则每预测10次后切换绑定到下一个核心
        /// </summary>
        public const string LITE_POWER_RAND_LOW = "LITE_POWER_RAND_LOW";
    }

    public struct PaddleOcrArgs
    {
        /// <summary>
        /// 模型所在目录
        /// </summary>
        public string ModelDir = "paddleocr/models";

        /// <summary>
        /// 字典路径
        /// </summary>
        public string LabelPath = "paddleocr/labels/keys.txt";

        /// <summary>
        /// cpu工作线程数
        /// </summary>
        public int ThreadNum = 4;

        /// <summary>
        /// cpu能耗模式
        /// </summary>
        public string PowerMode = CpuPowerMode.LITE_POWER_NO_BIND;

        public PaddleOcrArgs()
        {

        }
    }

    public class PaddleOcrHelper
    {

        private static PaddleOcrHelper instance;
        public static PaddleOcrHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PaddleOcrHelper();
                }
                return instance;
            }
        }


        public static PaddleOcrHelper Create(PaddleOcrArgs args)
        {
            instance?.Dispose();
            instance = null;
            instance = new PaddleOcrHelper(args);
            return instance;
        }


        public string ModelDir { get; private set; }
        public string LabelPath { get; private set; }
        public int ThreadNum { get; private set; }
        public string PowerMode { get; private set; }

        private readonly Predictor predictor;

        private PaddleOcrHelper(PaddleOcrArgs args)
        {
            this.ModelDir = args.ModelDir;
            this.LabelPath = args.LabelPath;
            this.ThreadNum = args.ThreadNum;
            this.PowerMode = args.PowerMode;

            this.predictor = new Predictor();
            this.predictor.Init(Globals.AppContext, this.ModelDir, this.LabelPath, this.ThreadNum, this.PowerMode);
        }

        private PaddleOcrHelper()
        {
            var args = new PaddleOcrArgs();

            this.ModelDir = args.ModelDir;
            this.LabelPath = args.LabelPath;
            this.ThreadNum = args.ThreadNum;
            this.PowerMode = args.PowerMode;

            this.predictor = new Predictor();
            this.predictor.Init(Globals.AppContext, this.ModelDir, this.LabelPath, this.ThreadNum, this.PowerMode);
        }

        public string Ocr(Bitmap bitmap)
        {
            var results = this.predictor.RunModel(bitmap);
            if (results is not null)
            {
                var result = new List<string>();

                foreach (var item in results)
                {
                    result.Add(item.Label);
                }

                return string.Join("\r\n", result);
            }
            else
            {
                return string.Empty;
            }
        }

        public void Dispose()
        {
            this.predictor?.ReleaseModel();
            this.predictor?.Dispose();
        }

    }
}
